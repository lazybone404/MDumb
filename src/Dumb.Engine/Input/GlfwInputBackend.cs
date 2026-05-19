using System.Numerics;
#if BROWSER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#endif
using Dumb.Engine.Window;
using Sia;
using Silk.NET.GLFW;
using EngineMouseButton = Dumb.Engine.Input.MouseButton;
using GlfwInputAction = Silk.NET.GLFW.InputAction;
using GlfwMouseButton = Silk.NET.GLFW.MouseButton;

namespace Dumb.Engine.Input;

public sealed unsafe class GlfwInputBackend : IInputBackend
{
    private static readonly KeyCode[] KeyCodes = Enum.GetValues<KeyCode>();
    private static readonly EngineMouseButton[] MouseButtons = Enum.GetValues<EngineMouseButton>();
    private static readonly GamepadButton[] GamepadButtons = Enum.GetValues<GamepadButton>();

    private readonly IWindowBackend _window;
    private Vector2 _scrollDelta;

#if !BROWSER
    private readonly Glfw _glfw;
    private readonly GlfwCallbacks.ScrollCallback _scrollCallback;
#else
    private static readonly Dictionary<nint, GlfwInputBackend> Instances = [];
#endif

    public GlfwInputBackend(IWindowBackend window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
#if !BROWSER
        _glfw = GlfwProvider.GLFW.Value;
        _scrollCallback = OnScroll;
        _glfw.SetScrollCallback((WindowHandle*)_window.NativeHandle, _scrollCallback);
#else
        var handle = (WindowHandle*)_window.NativeHandle;
        Instances[(nint)handle] = this;
        Dumb.Emscripten.GLFW.SetScrollCallback(handle, &OnScroll);
#endif
    }

    public void Poll(InputFrame frame)
    {
        var handle = (WindowHandle*)_window.NativeHandle;
        if (handle is null)
            return;

        UpdateKeyboard(frame, handle);
        UpdateMouse(frame, handle);
        UpdateGamepads(frame);
    }

    private void UpdateKeyboard(InputFrame frame, WindowHandle* handle)
    {
        foreach (var key in KeyCodes)
        {
            if (key == KeyCode.Unknown)
                continue;

#if BROWSER
            frame.SetKey(key, Dumb.Emscripten.GLFW.IsKeyDown(handle, (Keys)(int)key));
#else
            var state = _glfw.GetKey(handle, (Keys)(int)key);
            frame.SetKey(key, state is (int)GlfwInputAction.Press or (int)GlfwInputAction.Repeat);
#endif
        }
    }

    private void UpdateMouse(InputFrame frame, WindowHandle* handle)
    {
#if BROWSER
        Dumb.Emscripten.GLFW.GetCursorPos(handle, out var x, out var y);
#else
        _glfw.GetCursorPos(handle, out var x, out var y);
#endif
        frame.SetMousePosition(ScreenPosition.TopLeft(
            (float)x / _window.FramebufferWidth,
            (float)y / _window.FramebufferHeight));

        foreach (var button in MouseButtons)
        {
#if BROWSER
            var pressed = Dumb.Emscripten.GLFW.IsMouseButtonDown(handle, (GlfwMouseButton)(int)button);
#else
            var pressed = _glfw.GetMouseButton(handle, (int)button) == (int)GlfwInputAction.Press;
#endif
            frame.SetMouseButton(button, pressed);
        }

        frame.AddMouseScroll(ConsumeScrollDelta());
    }

    private void UpdateGamepads(InputFrame frame)
    {
        for (var gamepad = 0; gamepad < InputFrame.MAX_GAMEPADS; gamepad++)
        {
#if BROWSER
            if (!Dumb.Emscripten.GLFW.JoystickPresent(gamepad))
                continue;

            for (var axis = 0; axis < InputFrame.MAX_GAMEPAD_AXES; axis++)
            {
                if (Dumb.Emscripten.GLFW.TryGetJoystickAxis(gamepad, axis, out var value))
                    frame.SetGamepadAxis(gamepad, axis, value);
            }

            foreach (var button in GamepadButtons)
            {
                if (Dumb.Emscripten.GLFW.TryGetJoystickButton(gamepad, (int)button, out var pressed))
                    frame.SetGamepadButton(gamepad, button, pressed);
            }
#else
            if (!_glfw.JoystickPresent(gamepad))
                continue;

            var axes = _glfw.GetJoystickAxes(gamepad, out var axisCount);
            for (var axis = 0; axes is not null && axis < axisCount; axis++)
                frame.SetGamepadAxis(gamepad, axis, axes[axis]);

            var buttons = _glfw.GetJoystickButtons(gamepad, out var buttonCount);
            foreach (var button in GamepadButtons)
            {
                var index = (int)button;
                if (buttons is not null && index < buttonCount)
                    frame.SetGamepadButton(gamepad, button, buttons[index] == (byte)GlfwInputAction.Press);
            }
#endif
        }
    }

    private Vector2 ConsumeScrollDelta()
    {
        var delta = _scrollDelta;
        _scrollDelta = Vector2.Zero;
        return delta;
    }

    public void Dispose()
    {
#if !BROWSER
        var handle = (WindowHandle*)_window.NativeHandle;
        if (handle is not null)
            _glfw.SetScrollCallback(handle, null);
#else
        var handle = (WindowHandle*)_window.NativeHandle;
        Instances.Remove((nint)handle);
#endif
    }

#if BROWSER
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnScroll(WindowHandle* window, double xOffset, double yOffset)
    {
        if (Instances.TryGetValue((nint)window, out var backend))
            backend._scrollDelta += new Vector2((float)xOffset, (float)yOffset);
    }
#else
    private void OnScroll(WindowHandle* window, double xOffset, double yOffset)
    {
        _scrollDelta += new Vector2((float)xOffset, (float)yOffset);
    }
#endif
}
