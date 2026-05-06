namespace Dumb.Emscripten;

using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.GLFW;

public static unsafe class GLFW
{
    public const int Joystick1 = 0;
    public const int JoystickLast = 15;

    public static void Init()
    {
        if (GLFWNative.Init() == Bool.False)
            throw new InvalidOperationException("glfwInit failed.");
    }

    public static void Terminate()
    {
        GLFWNative.Terminate();
    }

    public static void WindowHint(WindowHintBool hint, bool value)
    {
        GLFWNative.WindowHint((int)hint, value ? (int)Bool.True : (int)Bool.False);
    }

    public static void WindowHint(WindowHintClientApi hint, ClientApi value)
    {
        GLFWNative.WindowHint((int)hint, (int)value);
    }

    public static WindowHandle* CreateWindow(int width, int height, string title, nint monitor = 0, WindowHandle* share = null)
    {
        var utf8Title = Encoding.UTF8.GetBytes(title + '\0');
        fixed (byte* titlePtr = utf8Title)
            return GLFWNative.CreateWindow(width, height, titlePtr, monitor, share);
    }

    public static void DestroyWindow(WindowHandle* window)
    {
        GLFWNative.DestroyWindow(window);
    }

    public static void PollEvents()
    {
        GLFWNative.PollEvents();
    }

    public static bool IsKeyDown(WindowHandle* window, Keys key)
    {
        var state = GLFWNative.GetKey(window, key);
        return state is InputAction.Press or InputAction.Repeat;
    }

    public static bool IsMouseButtonDown(WindowHandle* window, MouseButton button)
    {
        return GLFWNative.GetMouseButton(window, button) == InputAction.Press;
    }

    public static nint SetKeyCallback(WindowHandle* window, delegate* unmanaged[Cdecl]<WindowHandle*, Keys, int, InputAction, KeyModifiers, void> callback)
    {
        return GLFWNative.SetKeyCallback(window, (nint)callback);
    }

    public static nint SetMouseButtonCallback(WindowHandle* window, delegate* unmanaged[Cdecl]<WindowHandle*, MouseButton, InputAction, KeyModifiers, void> callback)
    {
        return GLFWNative.SetMouseButtonCallback(window, (nint)callback);
    }

    public static nint SetCursorPosCallback(WindowHandle* window, delegate* unmanaged[Cdecl]<WindowHandle*, double, double, void> callback)
    {
        return GLFWNative.SetCursorPosCallback(window, (nint)callback);
    }

    public static nint SetScrollCallback(WindowHandle* window, delegate* unmanaged[Cdecl]<WindowHandle*, double, double, void> callback)
    {
        return GLFWNative.SetScrollCallback(window, (nint)callback);
    }

    public static bool JoystickPresent(int jid)
    {
        return GLFWNative.JoystickPresent(jid) == Bool.True;
    }

    public static string GetJoystickName(int jid)
    {
        return Marshal.PtrToStringUTF8((nint)GLFWNative.GetJoystickName(jid)) ?? string.Empty;
    }

    public static bool TryGetJoystickAxis(int jid, int axis, out float value)
    {
        int count;
        var axes = GLFWNative.GetJoystickAxes(jid, &count);
        if (axes is null || axis < 0 || axis >= count)
        {
            value = 0;
            return false;
        }

        value = axes[axis];
        return true;
    }

    public static bool TryGetJoystickButton(int jid, int button, out bool pressed)
    {
        int count;
        var buttons = GLFWNative.GetJoystickButtons(jid, &count);
        if (buttons is null || button < 0 || button >= count)
        {
            pressed = false;
            return false;
        }

        pressed = buttons[button] == (byte)InputAction.Press;
        return true;
    }
}
