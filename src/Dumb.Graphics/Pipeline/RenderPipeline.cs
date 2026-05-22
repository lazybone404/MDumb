using Sia;

namespace Dumb.Graphics.Pipeline;

public sealed class RenderPipeline : IDisposable
{
    private readonly GraphicsContext _ctx;
    private readonly RenderGraph _graph;
    private readonly SystemStage _syncStage;
    private bool _disposed;

    public PhaseQueueSystem PhaseQueue { get; }
    public RenderGraph Graph => _graph;

    public RenderPipeline(
        GraphicsContext ctx,
        CameraSyncSystem cameraSync,
        TransformSyncSystem transformSync,
        LightSyncSystem lightSync,
        PhaseQueueSystem phaseQueue)
    {
        _ctx = ctx;
        PhaseQueue = phaseQueue;
        _graph = new RenderGraph(ctx);

        _syncStage = SystemChain.Empty
            .Add(() => cameraSync)
            .Add(() => transformSync)
            .Add(() => lightSync)
            .Add(() => phaseQueue)
            .CreateStage(ctx.World);
    }

    public void Tick()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _syncStage.Tick();
        _graph.Run(_ctx.World);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _syncStage.Dispose();
    }
}
