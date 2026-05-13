using Sia;

namespace Dumb.Engine.Window;

public static class WindowExtensions
{
    public static Entity CreateWindow(this World world, WindowDescriptor descriptor)
    {
        var window = new GlfwWindowBackend(descriptor);
        var input = new Input.GlfwInputBackend(window);

        return world.Create(HList.From(
            new WindowState(
                descriptor.Width,
                descriptor.Height,
                descriptor.Width,
                descriptor.Height,
                false),
            new WindowRuntime(window, input),
            new Input.WindowInput()));
    }

    public static void DestroyWindow(this Entity entity)
    {
        if (entity.IsValid && entity.Contains<WindowRuntime>())
            entity.Get<WindowRuntime>().Dispose();

        if (entity.IsValid)
            entity.Destroy();
    }
}
