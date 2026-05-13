using Dumb.Engine.Input;

namespace Dumb.Engine.Window;

public sealed class WindowRuntime(IWindowBackend window, IInputBackend input) : IDisposable
{
    private bool _disposed;

    public IWindowBackend Window { get; } = window;
    public IInputBackend Input { get; } = input;
    public WindowEventSink Events { get; } = new();

    internal bool Initialized { get; set; }

    public nint NativeHandle => Window.NativeHandle;
    public Silk.NET.Core.Contexts.INativeWindow? Native => Window.Native;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Input.Dispose();
        Window.Dispose();
    }
}
