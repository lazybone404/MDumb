using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sia;
using Silk.NET.WebGPU;
using Dumb.Graphics.Interfaces;
#if BROWSER
using Dumb.Emscripten;
#endif

namespace Dumb.Graphics;

public class GraphicsContext : IDisposable
{
    public readonly World _world = new();

    public World World => _world;

    // Resource managers (instance classes, replacing former static classes)
    public BufferManager Buffers { get; }
    public TextureManager Textures { get; }
    public ShaderManager Shaders { get; }
    public SamplerManager Samplers { get; }
    public PipelineManager Pipelines { get; }
    public MeshManager Meshes { get; }
    public MaterialManager Materials { get; }

    // Surface lifecycle manager
    public SurfaceManager Surfaces { get; }

    public void AttachToParentWorld(World parentWorld)
    {
        var context = _world.AddAddon<SubWorldContext>();
        context.Parent = parentWorld;
    }

    public Entity GetFirstPipelineLayout()
        => _pipelineLayouts.First();

    public Entity CreateMaterialResource(Entity pipeline, Entity pipelineLayout, Entity?[] bindGroups)
    {
        return _materials.Create(HList.From(new MaterialResourceData
        {
            Pipeline = pipeline,
            PipelineLayout = pipelineLayout,
            BindGroups = bindGroups,
            RefCount = 1
        }));
    }

    public readonly IEntityHost<HList<BufferData, EmptyHList>> _buffers;
    public readonly IEntityHost<HList<TextureData, EmptyHList>> _textures;
    public readonly IEntityHost<HList<TextureViewData, EmptyHList>> _textureViews;
    public readonly IEntityHost<HList<SamplerData, EmptyHList>> _samplers;
    public readonly IEntityHost<HList<ShaderData, EmptyHList>> _shaders;
    public readonly IEntityHost<HList<BindGroupLayoutData, EmptyHList>> _bindGroupLayouts;
    public readonly IEntityHost<HList<BindGroupData, EmptyHList>> _bindGroups;
    public readonly IEntityHost<HList<PipelineLayoutData, EmptyHList>> _pipelineLayouts;
    public readonly IEntityHost<HList<RenderPipelineData, EmptyHList>> _renderPipelines;
    public readonly IEntityHost<HList<ComputePipelineData, EmptyHList>> _computePipelines;
    public readonly IEntityHost<HList<MeshResourceData, EmptyHList>> _meshes;
    public readonly IEntityHost<HList<MaterialResourceData, EmptyHList>> _materials;

    public nint NativeInstance;
    public nint NativeAdapter;
    public nint NativeDevice;
    public nint NativeQueue;

    public readonly IDeviceBackend Device;
    public readonly ICommandBackend Command;
    public readonly ISwapChainBackend SwapChain;

#if BROWSER
    public readonly WGPUBrowser _wgpu;
#else
    public readonly WebGPU _wgpu;
#endif

    private bool _disposed;

    public GraphicsContext()
    {
        _buffers = _world.AcquireHost<HList<BufferData, EmptyHList>, ArrayEntityHost<HList<BufferData, EmptyHList>>>();
        _textures = _world.AcquireHost<HList<TextureData, EmptyHList>, ArrayEntityHost<HList<TextureData, EmptyHList>>>();
        _textureViews = _world.AcquireHost<HList<TextureViewData, EmptyHList>, ArrayEntityHost<HList<TextureViewData, EmptyHList>>>();
        _samplers = _world.AcquireHost<HList<SamplerData, EmptyHList>, ArrayEntityHost<HList<SamplerData, EmptyHList>>>();
        _shaders = _world.AcquireHost<HList<ShaderData, EmptyHList>, ArrayEntityHost<HList<ShaderData, EmptyHList>>>();
        _bindGroupLayouts = _world.AcquireHost<HList<BindGroupLayoutData, EmptyHList>, ArrayEntityHost<HList<BindGroupLayoutData, EmptyHList>>>();
        _bindGroups = _world.AcquireHost<HList<BindGroupData, EmptyHList>, ArrayEntityHost<HList<BindGroupData, EmptyHList>>>();
        _pipelineLayouts = _world.AcquireHost<HList<PipelineLayoutData, EmptyHList>, ArrayEntityHost<HList<PipelineLayoutData, EmptyHList>>>();
        _renderPipelines = _world.AcquireHost<HList<RenderPipelineData, EmptyHList>, ArrayEntityHost<HList<RenderPipelineData, EmptyHList>>>();
        _computePipelines = _world.AcquireHost<HList<ComputePipelineData, EmptyHList>, ArrayEntityHost<HList<ComputePipelineData, EmptyHList>>>();
        _meshes = _world.AcquireHost<HList<MeshResourceData, EmptyHList>, ArrayEntityHost<HList<MeshResourceData, EmptyHList>>>();
        _materials = _world.AcquireHost<HList<MaterialResourceData, EmptyHList>, ArrayEntityHost<HList<MaterialResourceData, EmptyHList>>>();

#if BROWSER
        _wgpu = new WGPUBrowser();
#else
        _wgpu = global::Silk.NET.WebGPU.WebGPU.GetApi();
#endif
        Device = new DeviceBackend(_wgpu);
        Command = new CommandBackend(_wgpu);
        SwapChain = new SwapChainBackend(_wgpu);

        // Initialize resource managers (after Device/Command/SwapChain are set)
        Buffers = new BufferManager(this);
        Textures = new TextureManager(this);
        Shaders = new ShaderManager(this);
        Samplers = new SamplerManager(this);
        Pipelines = new PipelineManager(this);
        Meshes = new MeshManager(this);
        Materials = new MaterialManager(this);
        Surfaces = new SurfaceManager(this, Device, SwapChain);
    }

#if BROWSER
    public WGPUBrowser NativeApi => _wgpu;
#else
    public WebGPU NativeApi => _wgpu;
#endif

    public nint NativeInstanceHandle => NativeInstance;
    public nint NativeAdapterHandle => NativeAdapter;
    public nint NativeDeviceHandle => NativeDevice;
    public nint NativeQueueHandle => NativeQueue;

    public GraphicsSurface CreateSurfaceFromNative(nint handle)
        => Surfaces.CreateFromNative(handle);

    public static unsafe void SetCompatibleSurface(
        ref RequestAdapterOptions options, in GraphicsSurface surface)
    {
        options.CompatibleSurface = (Surface*)surface.Handle;
    }

    public TextureFormat GetSurfacePreferredFormat(GraphicsSurface surface)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Surfaces.GetPreferredFormat(surface);
    }

    public void ConfigureSurface(in GraphicsSurface surface,
        TextureUsage usage = TextureUsage.RenderAttachment,
        PresentMode presentMode = PresentMode.Fifo)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Surfaces.Configure(surface, usage, presentMode);
    }

    public SurfaceFrame BeginFrame(in GraphicsSurface surface)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Surfaces.BeginFrame(surface);
    }

    public void PresentSurface(nint surface)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Surfaces.Present(surface);
    }

    public void UnconfigureSurface(GraphicsSurface surface)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Surfaces.Unconfigure(surface.Handle);
    }

    public unsafe void DestroySurface(ref GraphicsSurface surface)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Surfaces.Destroy(ref surface);
    }

    public Task InitializeAsync(RequestAdapterOptions options, DeviceDescriptor descriptor)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return InitializeInternal(options, descriptor);
    }

    private async Task InitializeInternal(RequestAdapterOptions options, DeviceDescriptor descriptor)
    {
        if (NativeInstance == 0)
            unsafe { NativeInstance = Device.CreateInstance(null); }

        var adapter = await RequestAdapterAsync(NativeInstance, options).ConfigureAwait(false);
        NativeAdapter = adapter;

        var device = await RequestDeviceAsync(NativeAdapter, descriptor).ConfigureAwait(false);
        NativeDevice = device;

        NativeQueue = Device.DeviceGetQueue(NativeDevice);
    }

    private unsafe Task<nint> RequestAdapterAsync(nint instance, RequestAdapterOptions options)
    {
        var tcs = new TaskCompletionSource<nint>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = GCHandle.Alloc(tcs);
        Device.InstanceRequestAdapter(instance, &options, &AdapterCallback, (void*)GCHandle.ToIntPtr(handle));
        return tcs.Task;
    }

    private unsafe Task<nint> RequestDeviceAsync(nint adapter, DeviceDescriptor descriptor)
    {
        var tcs = new TaskCompletionSource<nint>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = GCHandle.Alloc(tcs);
        Device.AdapterRequestDevice(adapter, &descriptor, &DeviceCallback, (void*)GCHandle.ToIntPtr(handle));
        return tcs.Task;
    }

    public void Tick()
    {
        if (NativeInstance != 0)
            Device.InstanceProcessEvents(NativeInstance);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _computePipelines.ForSlice<ComputePipelineData>((ref ComputePipelineData d) => Device.ReleaseComputePipeline(d.NativePtr));
        _renderPipelines.ForSlice<RenderPipelineData>((ref RenderPipelineData d) => Device.ReleaseRenderPipeline(d.NativePtr));
        _pipelineLayouts.ForSlice<PipelineLayoutData>((ref PipelineLayoutData d) => Device.ReleasePipelineLayout(d.NativePtr));
        _bindGroups.ForSlice<BindGroupData>((ref BindGroupData d) => Device.ReleaseBindGroup(d.NativePtr));
        _bindGroupLayouts.ForSlice<BindGroupLayoutData>((ref BindGroupLayoutData d) => Device.ReleaseBindGroupLayout(d.NativePtr));
        _shaders.ForSlice<ShaderData>((ref ShaderData d) => Device.ReleaseShaderModule(d.NativePtr));
        _samplers.ForSlice<SamplerData>((ref SamplerData d) => Device.ReleaseSampler(d.NativePtr));
        _textureViews.ForSlice<TextureViewData>((ref TextureViewData d) => Device.ReleaseTextureView(d.NativePtr));
        _textures.ForSlice<TextureData>((ref TextureData d) => Device.ReleaseTexture(d.NativePtr));
        _buffers.ForSlice<BufferData>((ref BufferData d) => Device.ReleaseBuffer(d.NativePtr));
        _meshes.ForSlice<MeshResourceData>((ref MeshResourceData m) =>
        {
            foreach (var vb in m.VertexBuffers)
            {
                if (vb.Host != null)
                    Buffers.Release(vb);
            }
            if (m.IndexBuffer.Host != null)
                Buffers.Release(m.IndexBuffer);
        });
        _materials.ForSlice<MaterialResourceData>((ref MaterialResourceData m) =>
        {
            if (m.Pipeline.Host != null)
                Pipelines.ReleaseRenderPipeline(m.Pipeline);
            if (m.PipelineLayout.Host != null)
                Pipelines.ReleasePipelineLayout(m.PipelineLayout);
            if (m.BindGroups != null)
            {
                foreach (var bg in m.BindGroups)
                {
                    if (bg?.Host != null)
                        Pipelines.ReleaseBindGroup(bg);
                }
            }
        });

        _world.Dispose();

        if (NativeDevice != 0) { Device.ReleaseDevice(NativeDevice); NativeDevice = 0; }
        if (NativeAdapter != 0) { Device.ReleaseAdapter(NativeAdapter); NativeAdapter = 0; }
        if (NativeInstance != 0) { Device.ReleaseInstance(NativeInstance); NativeInstance = 0; }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void AdapterCallback(RequestAdapterStatus status, nint adapter, byte* message, void* userdata)
    {
        var handle = GCHandle.FromIntPtr((nint)userdata);
        var tcs = (TaskCompletionSource<nint>)handle.Target!;
        handle.Free();
        if (status == RequestAdapterStatus.Success)
            tcs.TrySetResult(adapter);
        else
            tcs.TrySetException(new InvalidOperationException(
                message != null ? Marshal.PtrToStringUTF8((nint)message) : "Adapter request failed"));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void DeviceCallback(RequestDeviceStatus status, nint device, byte* message, void* userdata)
    {
        var handle = GCHandle.FromIntPtr((nint)userdata);
        var tcs = (TaskCompletionSource<nint>)handle.Target!;
        handle.Free();
        if (status == RequestDeviceStatus.Success)
            tcs.TrySetResult(device);
        else
            tcs.TrySetException(new InvalidOperationException(
                message != null ? Marshal.PtrToStringUTF8((nint)message) : "Device request failed"));
    }
}
