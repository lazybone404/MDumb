using System.Runtime.CompilerServices;
using Dumb.Graphics.Material;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics.Pipeline.Nodes;

public sealed class DeferredLightingNode : RenderNode
{
    private readonly GraphicsContext _ctx;
    private readonly CameraSyncSystem _cameraSync;
    private readonly LightSyncSystem _lightSync;
    private readonly GBuffer _gbuffer;
    private readonly TextureFormat _surfaceFormat;

    private Entity _sampler;
    private Entity _pipelineLayout;
    private Entity _pipeline;
    private Entity? _cachedGroup1;
    private Entity? _cachedGroup2;
    private Entity? _cachedFrameBg;
    private int _cachedCameraBufferId = -1;
    private int _cachedGbufferKey;
    private bool _materialCreated;

    public Entity SwapchainView { get; set; }

    public DeferredLightingNode(
        GraphicsContext ctx,
        CameraSyncSystem cameraSync,
        LightSyncSystem lightSync,
        GBuffer gbuffer,
        TextureFormat surfaceFormat)
    {
        _ctx = ctx;
        _cameraSync = cameraSync;
        _lightSync = lightSync;
        _gbuffer = gbuffer;
        _surfaceFormat = surfaceFormat;
    }

    public override void DeclareResources()
    {
        Inputs.Add(new ResourceHandle(_gbuffer.RT0View, "GBuffer_BaseColor"));
        Inputs.Add(new ResourceHandle(_gbuffer.RT1View, "GBuffer_NormalRoughness"));
        Inputs.Add(new ResourceHandle(_gbuffer.RT2View, "GBuffer_PBR"));
        Inputs.Add(new ResourceHandle(_gbuffer.DepthView, "GBuffer_Depth"));
        Outputs.Add(new ResourceHandle(SwapchainView, "SwapchainOutput"));
    }

    public override void Update(World world)
    {
        var cameraBuffer = _cameraSync.FirstBuffer;
        if (cameraBuffer is not { } camBuf) return;

        if (!_materialCreated)
        {
            _sampler = _ctx.Samplers.LinearClamp();
            (_pipeline, _pipelineLayout) = DeferredLightingMaterial.CreatePipeline(_ctx, _surfaceFormat);
            _materialCreated = true;
        }

        var cameraId = camBuf.Id.Value;
        var gbufferKey = HashCode.Combine(
            _gbuffer.RT0View.Id.Value, _gbuffer.RT1View.Id.Value,
            _gbuffer.RT2View.Id.Value, _gbuffer.DepthView.Id.Value);

        if (_cachedCameraBufferId == cameraId && _cachedGbufferKey == gbufferKey)
            return;

        var bindGroups = BuildMaterialConfig(camBuf).CreateBindGroups(_ctx, _pipelineLayout);
        var newGroup1 = bindGroups[1];
        var newGroup2 = bindGroups[2];

        ref var plData = ref _pipelineLayout.Get<PipelineLayoutData>();
        var frameBgl = plData.BindGroupLayouts?[0];
        Entity? newFrameBg = null;
        if (frameBgl?.Host != null)
        {
            newFrameBg = _ctx.Pipelines.BindGroup(frameBgl,
            [
                Binding.Buffer(0, camBuf, (nuint)Unsafe.SizeOf<CameraUniforms>()),
            ]);
        }

        // Release old only after new are created successfully.
        if (_cachedGroup1 is { } old1)
            _ctx.Pipelines.ReleaseBindGroup(old1);
        if (_cachedGroup2 is { } old2)
            _ctx.Pipelines.ReleaseBindGroup(old2);
        if (_cachedFrameBg is { } oldFg)
            _ctx.Pipelines.ReleaseBindGroup(oldFg);

        _cachedGroup1 = newGroup1;
        _cachedGroup2 = newGroup2;
        _cachedFrameBg = newFrameBg;

        _cachedCameraBufferId = cameraId;
        _cachedGbufferKey = gbufferKey;
    }

    private DeferredLightingMaterial BuildMaterialConfig(Entity camBuf)
    {
        return new DeferredLightingMaterial
        {
            GBufferRT0 = _gbuffer.RT0View,
            GBufferRT1 = _gbuffer.RT1View,
            GBufferRT2 = _gbuffer.RT2View,
            GBufferDepth = _gbuffer.DepthView,
            Sampler = _sampler,
            CameraBuffer = camBuf,
            LightBuffer = _lightSync.LightBuffer,
        };
    }

    public override void Execute(World world, RenderContext renderCtx)
    {
        if (!_materialCreated
            || _cachedGroup1 is null
            || _cachedGroup2 is null
            || _cachedFrameBg is null
            || SwapchainView.Host == null)
            return;

        var item = new PhaseItem(
            DrawEntity: default,
            Pipeline: _pipeline,
            PipelineLayout: _pipelineLayout,
            BindGroups: [_cachedFrameBg, _cachedGroup1, _cachedGroup2],
            Mesh: default!,
            SubMeshIndex: 0
        );

        unsafe
        {
            var colorAttachment = Commands.ColorAttachment(_ctx, SwapchainView,
                new Color { R = 0, G = 0, B = 0, A = 1 });

            var renderDesc = Commands.RenderPass(&colorAttachment);
            var encoder = Commands.CreateEncoder(_ctx);
            var pass = encoder.BeginRenderPass(&renderDesc);

            DrawCommand.DrawFullScreen(_ctx, ref pass, item);

            pass.End();
            var cb = encoder.Finish();
            renderCtx.AddCommandBuffer(cb);
        }
    }

}
