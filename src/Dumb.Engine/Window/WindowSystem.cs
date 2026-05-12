using Sia;

namespace Dumb.Engine.Window;

public sealed class WindowSystem : SystemBase
{
    private readonly WindowHost _host;
    private bool _initialized;
    private int _prevWidth, _prevHeight, _prevFbWidth, _prevFbHeight;
    private bool _prevShouldClose;

    public WindowSystem(WindowHost host) : base(Matchers.Of<Window>())
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public override void Execute(World world, IEntityQuery query)
    {
        if (!_host.Entity.IsValid)
            return;

        _host.Glfw.PollEvents();

        ref var window = ref _host.Entity.Get<Window>();
        window.Width = _host.Glfw.Width;
        window.Height = _host.Glfw.Height;
        window.FramebufferWidth = _host.Glfw.FramebufferWidth;
        window.FramebufferHeight = _host.Glfw.FramebufferHeight;
        window.ShouldClose = _host.Glfw.ShouldClose;

        if (_initialized)
        {
            if (window.Width != _prevWidth || window.Height != _prevHeight ||
                window.FramebufferWidth != _prevFbWidth || window.FramebufferHeight != _prevFbHeight)
            {
                world.Send(_host.Entity, new WindowResizeEvent(
                    window.Width, window.Height,
                    window.FramebufferWidth, window.FramebufferHeight));
            }

            if (window.ShouldClose && !_prevShouldClose)
                world.Send(_host.Entity, new WindowCloseEvent());
        }
        else
        {
            _initialized = true;
        }

        _prevWidth = window.Width;
        _prevHeight = window.Height;
        _prevFbWidth = window.FramebufferWidth;
        _prevFbHeight = window.FramebufferHeight;
        _prevShouldClose = window.ShouldClose;
    }
}
