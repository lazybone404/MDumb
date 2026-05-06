namespace Shit.Emscripten;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public static partial class Emscripten
{
    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_set_main_loop(nint action, int fps, [MarshalAs(UnmanagedType.I1)] bool simulateInfiniteLoop);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_set_main_loop_arg(nint action, nint args, int fps, [MarshalAs(UnmanagedType.I1)] bool simulateInfiniteLoop);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_pause_main_loop();

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_resume_main_loop();

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_request_animation_frame(nint callback, nint userData);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_set_timeout(nint callback, double ms, nint userData);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_sleep(double ms);

    [LibraryImport("__Internal_emscripten")]
    private static partial int emscripten_sample_gamepad_data();

    [LibraryImport("__Internal_emscripten")]
    private static unsafe partial void emscripten_console_log(byte* message);

    [LibraryImport("__Internal_emscripten")]
    private static unsafe partial int emscripten_set_canvas_element_size(byte* target, int width, int height);

    [LibraryImport("__Internal_emscripten")]
    private static unsafe partial int emscripten_get_element_css_size(byte* target, double* width, double* height);

    [LibraryImport("__Internal_emscripten")]
    private static unsafe partial void emscripten_request_animation_frame_loop(nint callback, nint userData);

    [Conditional("BROWSER")]
    public static unsafe void SetMainLoop(delegate* unmanaged[Cdecl]<void> action, int fps, bool simulateInfiniteLoop)
    {
        emscripten_set_main_loop((nint)action, fps, simulateInfiniteLoop);
    }

    [Conditional("BROWSER")]
    public static unsafe void SetMainLoop(delegate* unmanaged[Cdecl]<void*, void> action, void* args, int fps, bool simulateInfiniteLoop)
    {
        emscripten_set_main_loop_arg((nint)action, (nint)args, fps, simulateInfiniteLoop);
    }

    [Conditional("BROWSER")]
    public static void ResumeMainLoop()
    {
        emscripten_resume_main_loop();
    }

    [Conditional("BROWSER")]
    public static void PauseMainLoop()
    {
        emscripten_pause_main_loop();
    }

    [Conditional("BROWSER")]
    public static void ClearMainLoop()
    {
        emscripten_set_main_loop((nint)null, 0, false);
    }

    [Conditional("BROWSER")]
    public static unsafe void RequestAnimationFrame(delegate* unmanaged[Cdecl]<double, void*, void> callback, void* userData)
    {
        emscripten_request_animation_frame((nint)callback, (nint)userData);
    }

    [Conditional("BROWSER")]
    public static unsafe void SetTimeout(delegate* unmanaged[Cdecl]<void*, void> callback, void* userData, double ms)
    {
        emscripten_set_timeout((nint)callback, ms, (nint)userData);
    }

    [Conditional("BROWSER")]
    public static unsafe void RequestAnimationFrameLoop(delegate* unmanaged[Cdecl]<double, void*, int> callback, void* userData)
    {
        emscripten_request_animation_frame_loop((nint)callback, (nint)userData);
    }

    [Conditional("BROWSER")]
    public static void SampleGamepadData()
    {
        emscripten_sample_gamepad_data();
    }

    [Conditional("BROWSER")]
    public static void Sleep(double ms)
    {
        emscripten_sleep(ms);
    }

    [Conditional("BROWSER")]
    public static unsafe void ConsoleLog(string message)
    {
        var handle = GCHandle.Alloc(Encoding.UTF8.GetBytes(message + '\0'), GCHandleType.Pinned);
        emscripten_console_log((byte*)handle.AddrOfPinnedObject());
        handle.Free();
    }

    [Conditional("BROWSER")]
    public static unsafe void SetCanvasElementSize(string target, int width, int height)
    {
        var handle = GCHandle.Alloc(Encoding.UTF8.GetBytes(target + '\0'), GCHandleType.Pinned);
        emscripten_set_canvas_element_size((byte*)handle.AddrOfPinnedObject(), width, height);
        handle.Free();
    }

    public static unsafe bool TryGetElementCSSSize(string target, out int width, out int height)
    {
        var handle = GCHandle.Alloc(Encoding.UTF8.GetBytes(target + '\0'), GCHandleType.Pinned);
        double w, h;
        int result = emscripten_get_element_css_size((byte*)handle.AddrOfPinnedObject(), &w, &h);
        handle.Free();
        width = (int)w;
        height = (int)h;
        return result == 0;
    }
}
