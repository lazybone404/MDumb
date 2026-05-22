using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics.Pipeline.Nodes;

public sealed class CompositeNode : RenderNode
{
    private readonly GraphicsContext _ctx;

    public Entity? InputView { get; set; }
    public Entity? OutputView { get; set; }

    public CompositeNode(GraphicsContext ctx)
    {
        _ctx = ctx;
    }

    public override void Execute(World world, RenderContext renderCtx)
    {
        if (InputView?.Host == null || OutputView?.Host == null)
            return;

        unsafe
        {
            var colorAttachment = Commands.ColorAttachment(_ctx, OutputView,
                new Color { R = 0, G = 0, B = 0, A = 1 });

            var renderDesc = Commands.RenderPass(&colorAttachment);
            var encoder = Commands.CreateEncoder(_ctx);
            var pass = encoder.BeginRenderPass(&renderDesc);
            pass.End();
            var cb = encoder.Finish();
            renderCtx.AddCommandBuffer(cb);
        }
    }
}
