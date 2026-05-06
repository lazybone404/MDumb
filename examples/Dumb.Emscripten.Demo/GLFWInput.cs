using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Shit.Emscripten;
using Silk.NET.GLFW;

namespace Shit.Emscripten.Demo;

public struct InputState
{
    public bool MouseLeft;
    public bool MouseRight;
    public float MouseX;
    public float MouseY;
    public float ScrollDelta;
    public bool KeyW, KeyA, KeyS, KeyD;
    public bool Key1, Key2, Key3, Key4;
    public bool KeyJ, KeyL;
    public float JoystickAxisX;
    public float JoystickAxisY;
    public bool JoystickButton0;
    public bool JoystickButton1;
    public bool JoystickButton2;
    public bool JoystickButton3;
}

public static unsafe class GLFWInput
{
    private static WindowHandle* _window;
    private static int _joystickId = -1;
    private static float _mouseX;
    private static float _mouseY;
    private static float _scrollDelta;

    public static void Init()
    {
        Emscripten.ConsoleLog("[GLFW] Initializing GLFW...");
        GLFW.Init();

        GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        GLFW.WindowHint(WindowHintBool.Visible, false);
        GLFW.WindowHint(WindowHintBool.Resizable, true);

        _window = GLFW.CreateWindow(640, 360, "Card Demo");
        if (_window is null)
            Emscripten.ConsoleLog("[GLFW] Warning: CreateWindow returned null");
        else
        {
            Emscripten.ConsoleLog($"[GLFW] Window created: 0x{(nint)_window:X}");
            GLFW.SetCursorPosCallback(_window, &OnCursorPos);
            GLFW.SetScrollCallback(_window, &OnScroll);
        }

        int joystickCount = 0;
        for (int jid = GLFW.Joystick1; jid <= GLFW.JoystickLast; jid++)
        {
            if (!GLFW.JoystickPresent(jid))
                continue;

            joystickCount++;
            var name = GLFW.GetJoystickName(jid);
            Emscripten.ConsoleLog($"[GLFW]   Joystick {jid}: {name}");
            _joystickId = _joystickId < 0 ? jid : _joystickId;
        }

        Emscripten.ConsoleLog($"[GLFW] Joysticks detected: {joystickCount}");
        if (_joystickId >= 0)
            Emscripten.ConsoleLog($"[GLFW] Joystick {_joystickId} selected.");

        Emscripten.ConsoleLog("[GLFW] Initialized successfully.");
    }

    public static void Poll(ref InputState state)
    {
        GLFW.PollEvents();

        state.ScrollDelta = _scrollDelta;
        _scrollDelta = 0;

        if (_window is not null)
        {
            state.MouseLeft = GLFW.IsMouseButtonDown(_window, MouseButton.Left);
            state.MouseRight = GLFW.IsMouseButtonDown(_window, MouseButton.Right);
            state.MouseX = _mouseX;
            state.MouseY = _mouseY;

            state.KeyW = GLFW.IsKeyDown(_window, Keys.W);
            state.KeyA = GLFW.IsKeyDown(_window, Keys.A);
            state.KeyS = GLFW.IsKeyDown(_window, Keys.S);
            state.KeyD = GLFW.IsKeyDown(_window, Keys.D);
            state.Key1 = GLFW.IsKeyDown(_window, Keys.Number1);
            state.Key2 = GLFW.IsKeyDown(_window, Keys.Number2);
            state.Key3 = GLFW.IsKeyDown(_window, Keys.Number3);
            state.Key4 = GLFW.IsKeyDown(_window, Keys.Number4);
            state.KeyJ = GLFW.IsKeyDown(_window, Keys.J);
            state.KeyL = GLFW.IsKeyDown(_window, Keys.L);
        }

        PollJoystick(ref state);
    }

    public static void Shutdown()
    {
        if (_window is not null)
        {
            GLFW.DestroyWindow(_window);
            _window = null;
        }

        _joystickId = -1;
        GLFW.Terminate();
        Emscripten.ConsoleLog("[GLFW] Shutdown complete.");
    }

    private static void PollJoystick(ref InputState state)
    {
        state.JoystickAxisX = 0;
        state.JoystickAxisY = 0;
        state.JoystickButton0 = false;
        state.JoystickButton1 = false;
        state.JoystickButton2 = false;
        state.JoystickButton3 = false;

        if (_joystickId < 0 || !GLFW.JoystickPresent(_joystickId))
            return;

        if (GLFW.TryGetJoystickAxis(_joystickId, 0, out var axisX))
            state.JoystickAxisX = axisX;
        if (GLFW.TryGetJoystickAxis(_joystickId, 1, out var axisY))
            state.JoystickAxisY = -axisY;
        if (GLFW.TryGetJoystickButton(_joystickId, 0, out var button0))
            state.JoystickButton0 = button0;
        if (GLFW.TryGetJoystickButton(_joystickId, 1, out var button1))
            state.JoystickButton1 = button1;
        if (GLFW.TryGetJoystickButton(_joystickId, 2, out var button2))
            state.JoystickButton2 = button2;
        if (GLFW.TryGetJoystickButton(_joystickId, 3, out var button3))
            state.JoystickButton3 = button3;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnCursorPos(WindowHandle* window, double x, double y)
    {
        _mouseX = (float)x;
        _mouseY = (float)y;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnScroll(WindowHandle* window, double xOffset, double yOffset)
    {
        _scrollDelta += (float)yOffset;
    }
}
