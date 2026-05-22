using Sia;

namespace Dumb.Graphics.Pipeline;

public sealed class RenderGraph
{
    private readonly List<RenderNode> _nodes = [];
    private readonly RenderContext _renderContext;

    public RenderGraph(GraphicsContext ctx)
    {
        _renderContext = new RenderContext(ctx);
    }

    public void AddNode(RenderNode node) => _nodes.Add(node);
    public void RemoveNode(RenderNode node) => _nodes.Remove(node);
    public void Clear() => _nodes.Clear();
    public IReadOnlyList<RenderNode> Nodes => _nodes;

    public void Run(World world)
    {
        foreach (var node in _nodes)
            node.Update(world);

        foreach (var node in _nodes)
            node.Execute(world, _renderContext);

        _renderContext.Submit();
    }
}
