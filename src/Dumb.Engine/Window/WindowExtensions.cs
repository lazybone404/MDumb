using Sia;

namespace Dumb.Engine.Window;

public static class WindowExtensions
{
    public static Entity CreateWindow(
        this World world,
        WindowDescriptor descriptor,
        IWindowBackend? backend = null,
        Input.IInputBackend? inputBackend = null)
    {
        var window = backend ?? new GlfwWindowBackend(descriptor);
        var input = inputBackend ?? new Input.GlfwInputBackend(window);

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
