using Silk.NET.GLFW;

namespace Dumb.Engine.Window;

public sealed unsafe class GlfwWindow : IWindow
{
#if !BROWSER
    private readonly Glfw _glfw;
#endif

    private WindowHandle* _handle;
    private bool _disposed;

    public GlfwWindow(int width, int height, string title, bool visible = true)
    {
        Width = width;
        Height = height;
#if BROWSER
        Dumb.Emscripten.GLFW.Init();
        Dumb.Emscripten.GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        Dumb.Emscripten.GLFW.WindowHint(WindowHintBool.Visible, visible);
        _handle = Dumb.Emscripten.GLFW.CreateWindow(width, height, title);
#else
        _glfw = GlfwProvider.GLFW.Value;
        _glfw.Init();
        _glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        _glfw.WindowHint(WindowHintBool.Visible, visible);
        _handle = _glfw.CreateWindow(width, height, title, null, null);
#endif
        if (_handle is null)
            throw new InvalidOperationException("Failed to create GLFW window.");
    }

    public nint NativeHandle => (nint)_handle;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public bool ShouldClose
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
#if BROWSER
            return Dumb.Emscripten.GLFW.WindowShouldClose(_handle);
#else
            return _glfw.WindowShouldClose(_handle);
#endif
        }
    }

    public void PollEvents()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
#if BROWSER
        Dumb.Emscripten.GLFW.PollEvents();
        Dumb.Emscripten.GLFW.GetFramebufferSize(_handle, out var width, out var height);
#else
        _glfw.PollEvents();
        _glfw.GetFramebufferSize(_handle, out var width, out var height);
#endif
        Width = width;
        Height = height;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_handle is null)
            return;

#if BROWSER
        Dumb.Emscripten.GLFW.DestroyWindow(_handle);
        Dumb.Emscripten.GLFW.Terminate();
#else
        _glfw.DestroyWindow(_handle);
        _glfw.Terminate();
#endif
        _handle = null;
    }
}
