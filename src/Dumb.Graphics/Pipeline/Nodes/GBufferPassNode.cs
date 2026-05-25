using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics.Pipeline.Nodes;

public sealed class GBufferPassNode : RenderNode
{
    private readonly GraphicsContext _ctx;
    private readonly PhaseQueueSystem _phaseQueue;
    private readonly GBuffer _gbuffer;

    public GBufferPassNode(GraphicsContext ctx, PhaseQueueSystem phaseQueue, GBuffer gbuffer)
    {
        _ctx = ctx;
        _phaseQueue = phaseQueue;
        _gbuffer = gbuffer;
    }

    public override void DeclareResources()
    {
        Outputs.Add(new ResourceHandle(_gbuffer.RT0View, "GBuffer_Albedo"));
        Outputs.Add(new ResourceHandle(_gbuffer.RT1View, "GBuffer_NormalRoughness"));
        Outputs.Add(new ResourceHandle(_gbuffer.RT2View, "GBuffer_PBR"));
        Outputs.Add(new ResourceHandle(_gbuffer.DepthView, "GBuffer_Depth"));
    }

    public override void Execute(World world, RenderContext renderCtx)
    {
        var colorAttachments = _gbuffer.ColorAttachments(_ctx);
        var depthAttachment = _gbuffer.DepthAttachment(_ctx);

        unsafe
        {
            fixed (RenderPassColorAttachment* caPtr = colorAttachments)
            {
                var renderDesc = new RenderPassDescriptor
                {
                    ColorAttachmentCount = (nuint)colorAttachments.Length,
                    ColorAttachments = caPtr,
                    DepthStencilAttachment = &depthAttachment,
                    OcclusionQuerySet = null,
                    TimestampWrites = null,
                    Label = null
                };

                var encoder = Commands.CreateEncoder(_ctx);
                var pass = encoder.BeginRenderPass(&renderDesc);
                pass.SetViewport(0, 0, _gbuffer.Width, _gbuffer.Height, 0, 1);
                pass.SetScissorRect(0, 0, _gbuffer.Width, _gbuffer.Height);

                foreach (var binItems in _phaseQueue.OpaquePhase.Bins)
                {
                    foreach (var item in binItems)
                        DrawCommand.DrawMesh(_ctx, ref pass, item);
                }

                pass.End();
                var cb = encoder.Finish();
                renderCtx.AddCommandBuffer(cb);
            }
        }
    }
}
