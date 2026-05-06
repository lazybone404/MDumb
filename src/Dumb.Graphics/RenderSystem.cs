using Shit.Engine;

namespace Shit.Graphics;

public sealed class RenderSystem(IGraphicsDevice device) : IEngineSystem
{
    public string Name => "Render";

    public IGraphicsDevice Device { get; } = device;

    public void Tick(TimeSpan deltaTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(deltaTime, TimeSpan.Zero);
    }
}
