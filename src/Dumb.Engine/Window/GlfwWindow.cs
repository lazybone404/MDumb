using Silk.NET.Core.Contexts;
using Silk.NET.GLFW;

namespace Dumb.Engine.Window;

internal sealed unsafe class GlfwWindow
{
#if !BROWSER
    private readonly Glfw _glfw;
#endif

    private WindowHandle* _handle;
#if !BROWSER
    private INativeWindow? _native;
#endif
    private bool _disposed;

    public GlfwWindow(int width, int height, string title, bool visible = true)
    {
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

        Width = width;
        Height = height;
        FramebufferWidth = width;
        FramebufferHeight = height;
    }

    public nint NativeHandle => (nint)_handle;

    public INativeWindow? Native =>
#if BROWSER
        null;
#else
        _native ??= new GlfwNativeWindow(_glfw, _handle);
#endif

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int FramebufferWidth { get; private set; }

    public int FramebufferHeight { get; private set; }

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
        Dumb.Emscripten.GLFW.GetFramebufferSize(_handle, out var w, out var h);
        Width = w;
        Height = h;
        FramebufferWidth = w;
        FramebufferHeight = h;
#else
        _glfw.PollEvents();
        _glfw.GetWindowSize(_handle, out var w, out var h);
        _glfw.GetFramebufferSize(_handle, out var fw, out var fh);
        Width = w;
        Height = h;
        FramebufferWidth = fw;
        FramebufferHeight = fh;
#endif
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
