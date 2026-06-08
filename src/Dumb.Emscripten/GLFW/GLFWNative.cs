namespace Dumb.Emscripten;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.GLFW;

public static unsafe partial class GLFWNative
{
    private const string LibraryName = "__Internal_glfw";

    [LibraryImport(LibraryName, EntryPoint = "glfwInit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial Bool Init();

    [LibraryImport(LibraryName, EntryPoint = "glfwTerminate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Terminate();

    [LibraryImport(LibraryName, EntryPoint = "glfwWindowHint")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void WindowHint(int hint, int value);

    [LibraryImport(LibraryName, EntryPoint = "glfwCreateWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial WindowHandle* CreateWindow(int width, int height, byte* title, nint monitor, WindowHandle* share);

    [LibraryImport(LibraryName, EntryPoint = "glfwDestroyWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void DestroyWindow(WindowHandle* window);

    [LibraryImport(LibraryName, EntryPoint = "glfwWindowShouldClose")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial Bool WindowShouldClose(WindowHandle* window);

    [LibraryImport(LibraryName, EntryPoint = "glfwGetFramebufferSize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetFramebufferSize(WindowHandle* window, int* width, int* height);

    [LibraryImport(LibraryName, EntryPoint = "glfwGetCursorPos")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetCursorPos(WindowHandle* window, double* xpos, double* ypos);

    [LibraryImport(LibraryName, EntryPoint = "glfwPollEvents")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void PollEvents();

    [LibraryImport(LibraryName, EntryPoint = "glfwGetKey")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial InputAction GetKey(WindowHandle* window, Keys key);

    [LibraryImport(LibraryName, EntryPoint = "glfwGetMouseButton")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial InputAction GetMouseButton(WindowHandle* window, MouseButton button);

    [LibraryImport(LibraryName, EntryPoint = "glfwSetKeyCallback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint SetKeyCallback(WindowHandle* window, nint callback);

    [LibraryImport(LibraryName, EntryPoint = "glfwSetMouseButtonCallback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint SetMouseButtonCallback(WindowHandle* window, nint callback);

    [LibraryImport(LibraryName, EntryPoint = "glfwSetCursorPosCallback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint SetCursorPosCallback(WindowHandle* window, nint callback);

    [LibraryImport(LibraryName, EntryPoint = "glfwSetScrollCallback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint SetScrollCallback(WindowHandle* window, nint callback);

    [LibraryImport(LibraryName, EntryPoint = "glfwJoystickPresent")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial Bool JoystickPresent(int jid);

    [LibraryImport(LibraryName, EntryPoint = "glfwGetJoystickAxes")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial float* GetJoystickAxes(int jid, int* count);

    [LibraryImport(LibraryName, EntryPoint = "glfwGetJoystickButtons")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial byte* GetJoystickButtons(int jid, int* count);

    [LibraryImport(LibraryName, EntryPoint = "glfwGetJoystickName")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial byte* GetJoystickName(int jid);
}
