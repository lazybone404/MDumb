using Sia;

namespace Dumb.Graphics.Pipeline;

public abstract class RenderNode
{
    public virtual void Update(World world) { }
    public abstract void Execute(World world, RenderContext ctx);
}

public abstract class ViewNode : RenderNode, IDisposable
{
    private IEntityQuery? _viewQuery;

    protected abstract IEntityMatcher ViewMatcher { get; }

    public override void Update(World world)
    {
        _viewQuery ??= world.Query(ViewMatcher);
    }

    public override void Execute(World world, RenderContext ctx)
    {
        if (_viewQuery == null) return;
        foreach (var host in _viewQuery.Hosts)
        {
            foreach (var entity in host)
                ExecuteView(world, ctx, entity);
        }
    }

    protected abstract void ExecuteView(World world, RenderContext ctx, Entity viewEntity);

    public void Dispose()
    {
        _viewQuery?.Dispose();
        _viewQuery = null;
    }
}
