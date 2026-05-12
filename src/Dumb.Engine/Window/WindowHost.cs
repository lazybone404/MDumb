using Silk.NET.Core.Contexts;
using Sia;

namespace Dumb.Engine.Window;

public sealed class WindowHost : IDisposable
{
    private readonly GlfwWindow _glfw;
    private bool _disposed;

    internal WindowHost(World world, WindowDescriptor descriptor)
    {
        World = world;
        _glfw = new GlfwWindow(descriptor.Width, descriptor.Height, descriptor.Title, descriptor.Visible);
        Entity = world.Create(HList.From(new Window(descriptor.Width, descriptor.Height, descriptor.Width, descriptor.Height, false)));
    }

    public World World { get; }
    public Entity Entity { get; }

    public nint NativeHandle => _glfw.NativeHandle;
    public INativeWindow? Native => _glfw.Native;
    public int FramebufferWidth => _glfw.FramebufferWidth;
    public int FramebufferHeight => _glfw.FramebufferHeight;

    internal GlfwWindow Glfw => _glfw;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (Entity.IsValid)
            Entity.Destroy();
        _glfw.Dispose();
    }
}
