using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public class MaterialManager
{
    private readonly GraphicsContext _ctx;

    public MaterialManager(GraphicsContext ctx)
    {
        _ctx = ctx;
    }

    public Entity Create<T>(T material)
        where T : IMaterial
    {
        var shader = material.GetShader(_ctx);

        var config = T.Config;
        var bglDescs = config.BindGroupLayouts;
        var bglEntities = new Entity[bglDescs.Length];
        for (var i = 0; i < bglDescs.Length; i++)
            bglEntities[i] = _ctx.Pipelines.BindGroupLayout(bglDescs[i]);

        var pipelineLayout = _ctx.Pipelines.Layout(bglEntities);

        var bufferLayouts = MeshManager.ToVertexBufferLayouts(config.VertexDescriptor.Streams);

        var colorFormats = config.ColorFormats;
        Entity pipeline;
        if (colorFormats.Length == 1)
        {
            pipeline = _ctx.Pipelines.Render(
                shader, pipelineLayout,
                colorFormats[0],
                TextureFormat.Depth32float,
                bufferLayouts,
                config.Blend);
        }
        else
        {
            pipeline = _ctx.Pipelines.RenderMRT(
                shader, pipelineLayout,
                colorFormats,
                TextureFormat.Depth32float,
                bufferLayouts,
                config.Blend);
        }

        var bindGroups = material.CreateBindGroups(_ctx, pipelineLayout);

        return _ctx._materials.Create(HList.From(new MaterialResourceData
        {
            Pipeline = pipeline,
            PipelineLayout = pipelineLayout,
            BindGroups = bindGroups,
            RefCount = 1
        }));
    }

    public void Retain(Entity material)
    {
        ref var data = ref material.Get<MaterialResourceData>();
        Interlocked.Increment(ref data.RefCount);
    }

    public void Release(Entity material)
    {
        ref var data = ref material.Get<MaterialResourceData>();
        if (Interlocked.Decrement(ref data.RefCount) == 0)
        {
            _ctx.Pipelines.ReleaseRenderPipeline(data.Pipeline);
            _ctx.Pipelines.ReleasePipelineLayout(data.PipelineLayout);
            if (data.BindGroups != null)
            {
                foreach (var bg in data.BindGroups)
                {
                    if (bg?.Host != null)
                        _ctx.Pipelines.ReleaseBindGroup(bg);
                }
            }
            material.Destroy();
        }
    }
}
