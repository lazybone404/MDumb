namespace Shit.Graphics;

public interface IGraphicsDevice : IAsyncDisposable
{
    GraphicsDeviceDescriptor Descriptor { get; }
}
