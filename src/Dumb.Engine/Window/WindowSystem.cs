using Sia;

namespace Dumb.Engine.Window;

public sealed class WindowSystem() : SystemBase(Matchers.Of<WindowState, WindowRuntime>())
{
    public override void Execute(World world, IEntityQuery query)
    {
        foreach (var entity in query)
        {
            ref var window = ref entity.Get<WindowState>();
            var runtime = entity.Get<WindowRuntime>();

            runtime.Window.Pump(runtime.Events);

            if (runtime.Events.ConsumeResize(
                    out var width,
                    out var height,
                    out var framebufferWidth,
                    out var framebufferHeight))
            {
                window.Width = width;
                window.Height = height;
                window.FramebufferWidth = framebufferWidth;
                window.FramebufferHeight = framebufferHeight;

                if (runtime.Initialized)
                    world.Send(entity, new WindowResizeEvent(width, height, framebufferWidth, framebufferHeight));
            }

            if (runtime.Events.ConsumeCloseRequested() && !window.ShouldClose)
            {
                window.ShouldClose = true;

                if (runtime.Initialized)
                    world.Send(entity, new WindowCloseEvent());
            }

            if (!runtime.Initialized)
                runtime.Initialized = true;
        }
    }

    public override void Uninitialize(World world)
    {
        world.Query(Matchers.Of<WindowRuntime>(), static entity =>
        {
            entity.Get<WindowRuntime>().Dispose();
        });
    }
}
