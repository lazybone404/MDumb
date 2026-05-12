using Sia;

namespace Dumb.Engine.Window;

public static class WindowExtensions
{
    public static WindowHost CreateWindow(this World world, WindowDescriptor descriptor)
        => new(world, descriptor);
}
