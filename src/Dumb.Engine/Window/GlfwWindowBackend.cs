using Silk.NET.Core.Contexts;
using Silk.NET.GLFW;

namespace Dumb.Engine.Window;

public sealed unsafe class GlfwWindowBackend : IWindowBackend
{
#if !BROWSER
    private readonly Glfw _glfw;
    private INativeWindow? _native;
#endif

    private WindowHandle* _handle;
    private int _width;
    private int _height;
    private int _framebufferWidth;
    private int _framebufferHeight;
    private bool _disposed;

    public GlfwWindowBackend(WindowDescriptor descriptor)
    {
#if BROWSER
        Dumb.Emscripten.GLFW.Init();
        Dumb.Emscripten.GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        Dumb.Emscripten.GLFW.WindowHint(WindowHintBool.Visible, descriptor.Visible);
        _handle = Dumb.Emscripten.GLFW.CreateWindow(descriptor.Width, descriptor.Height, descriptor.Title);
#else
        _glfw = GlfwProvider.GLFW.Value;
        _glfw.Init();
        _glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        _glfw.WindowHint(WindowHintBool.Visible, descriptor.Visible);
        _handle = _glfw.CreateWindow(descriptor.Width, descriptor.Height, descriptor.Title, null, null);
#endif
        if (_handle is null)
            throw new InvalidOperationException("Failed to create GLFW window.");

        _width = descriptor.Width;
        _height = descriptor.Height;
        _framebufferWidth = descriptor.Width;
        _framebufferHeight = descriptor.Height;
    }

    public nint NativeHandle => (nint)_handle;

    public INativeWindow? Native =>
#if BROWSER
        null;
#else
        _native ??= new GlfwNativeWindow(_glfw, _handle);
#endif

    public void Pump(WindowEventSink sink)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

#if BROWSER
        Dumb.Emscripten.GLFW.PollEvents();
        Dumb.Emscripten.GLFW.GetFramebufferSize(_handle, out var width, out var height);
        var framebufferWidth = width;
        var framebufferHeight = height;
        var shouldClose = Dumb.Emscripten.GLFW.WindowShouldClose(_handle);
#else
        _glfw.PollEvents();
        _glfw.GetWindowSize(_handle, out var width, out var height);
        _glfw.GetFramebufferSize(_handle, out var framebufferWidth, out var framebufferHeight);
        var shouldClose = _glfw.WindowShouldClose(_handle);
#endif

        if (width != _width || height != _height ||
            framebufferWidth != _framebufferWidth || framebufferHeight != _framebufferHeight)
        {
            _width = width;
            _height = height;
            _framebufferWidth = framebufferWidth;
            _framebufferHeight = framebufferHeight;
            sink.Resize(width, height, framebufferWidth, framebufferHeight);
        }

        if (shouldClose)
            sink.CloseRequested();
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
