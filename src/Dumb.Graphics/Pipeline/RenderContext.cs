using System.Runtime.InteropServices;

namespace Dumb.Graphics.Pipeline;

public sealed class RenderContext
{
    private readonly GraphicsContext _ctx;
    private readonly List<nint> _commandBuffers = [];

    public RenderContext(GraphicsContext ctx) => _ctx = ctx;
    public GraphicsContext Graphics => _ctx;

    public void AddCommandBuffer(nint cb) => _commandBuffers.Add(cb);

    public void Submit()
    {
        if (_commandBuffers.Count == 0) return;
        Commands.Submit(_ctx, CollectionsMarshal.AsSpan(_commandBuffers));
        foreach (var cb in _commandBuffers)
            Commands.ReleaseCommandBuffer(_ctx, cb);
        _commandBuffers.Clear();
    }
}
