using Dumb.Engine.Input;
using Dumb.Engine.Window;

namespace Dumb.Engine.Tests.Stubs;

/// <summary>
/// IInputBackend 的测试桩 — 允许测试代码注入 Poll 行为。
/// </summary>
public sealed class StubInputBackend : IInputBackend
{
    public Action<InputFrame>? OnPoll { get; set; }

    public void Poll(InputFrame frame)
    {
        OnPoll?.Invoke(frame);
    }

    public void Dispose() { }
}

/// <summary>
/// IWindowBackend 的测试桩 — 允许测试代码注入 Pump 行为。
/// </summary>
public sealed class StubWindowBackend : IWindowBackend
{
    public nint NativeHandle => 0;
    public Silk.NET.Core.Contexts.INativeWindow? Native => null;
    public int FramebufferWidth => 1280;
    public int FramebufferHeight => 720;

    public Action<WindowEventSink>? OnPump { get; set; }

    public void Pump(WindowEventSink sink)
    {
        OnPump?.Invoke(sink);
    }

    public void Dispose() { }
}
