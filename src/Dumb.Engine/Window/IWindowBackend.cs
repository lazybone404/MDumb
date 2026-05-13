using Silk.NET.Core.Contexts;

namespace Dumb.Engine.Window;

public interface IWindowBackend : IDisposable
{
    nint NativeHandle { get; }
    INativeWindow? Native { get; }

    void Pump(WindowEventSink sink);
}
