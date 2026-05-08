using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Sia;
using Silk.NET.WebGPU;
#if BROWSER
using Dumb.Emscripten;
using Dumb.Graphics.Browser;
#else
using Dumb.Graphics.Native;
#endif

namespace Dumb.Graphics;

public class GraphicsContext : IDisposable
{
    internal readonly World _world = new();

    internal readonly IEntityHost<HList<BufferData, EmptyHList>> _buffers;
    internal readonly IEntityHost<HList<TextureData, EmptyHList>> _textures;
    internal readonly IEntityHost<HList<TextureViewData, EmptyHList>> _textureViews;
    internal readonly IEntityHost<HList<SamplerData, EmptyHList>> _samplers;
    internal readonly IEntityHost<HList<ShaderData, EmptyHList>> _shaders;
    internal readonly IEntityHost<HList<BindGroupLayoutData, EmptyHList>> _bindGroupLayouts;
    internal readonly IEntityHost<HList<BindGroupData, EmptyHList>> _bindGroups;
    internal readonly IEntityHost<HList<PipelineLayoutData, EmptyHList>> _pipelineLayouts;
    internal readonly IEntityHost<HList<RenderPipelineData, EmptyHList>> _renderPipelines;
    internal readonly IEntityHost<HList<ComputePipelineData, EmptyHList>> _computePipelines;

    internal nint NativeInstance;
    internal nint NativeAdapter;
    internal nint NativeDevice;
    internal nint NativeQueue;

    internal readonly IDeviceBackend Device;
    internal readonly ICommandBackend Command;

#if BROWSER
    readonly WGPUBrowser _wgpu;
#endif

    bool _disposed;

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

#if BROWSER
        _wgpu = new WGPUBrowser();
        Device = new BrowserDeviceBackend(_wgpu);
        Command = new BrowserCommandBackend(_wgpu);
#else
        var wgpu = global::Silk.NET.WebGPU.WebGPU.GetApi();
        Device = new NativeDeviceBackend(wgpu);
        Command = new NativeCommandBackend(wgpu);
#endif
    }

    public nint NativeInstanceHandle => NativeInstance;

    public nint NativeAdapterHandle => NativeAdapter;

    public nint NativeDeviceHandle => NativeDevice;

    public nint NativeQueueHandle => NativeQueue;

    public Task InitializeAsync(RequestAdapterOptions options, DeviceDescriptor descriptor)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return InitializeInternal(options, descriptor);
    }

    async Task InitializeInternal(RequestAdapterOptions options, DeviceDescriptor descriptor)
    {
        NativeInstance = CreateWgpuInstance();

        var adapter = await RequestAdapterAsync(NativeInstance, options).ConfigureAwait(false);
        NativeAdapter = adapter;

        var device = await RequestDeviceAsync(NativeAdapter, descriptor).ConfigureAwait(false);
        NativeDevice = device;

        NativeQueue = Device.DeviceGetQueue(NativeDevice);
    }

    unsafe nint CreateWgpuInstance() => Device.CreateInstance(null);

    unsafe Task<nint> RequestAdapterAsync(nint instance, RequestAdapterOptions options)
    {
        var tcs = new TaskCompletionSource<nint>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = GCHandle.Alloc(tcs);
        Device.InstanceRequestAdapter(instance, &options, &AdapterCallback, (void*)GCHandle.ToIntPtr(handle));
        return tcs.Task;
    }

    unsafe Task<nint> RequestDeviceAsync(nint adapter, DeviceDescriptor descriptor)
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

        _computePipelines.ForSlice<ComputePipelineData>(
            (ref ComputePipelineData cp) => Device.ReleaseComputePipeline(cp.NativePtr));
        _renderPipelines.ForSlice<RenderPipelineData>(
            (ref RenderPipelineData rp) => Device.ReleaseRenderPipeline(rp.NativePtr));
        _pipelineLayouts.ForSlice<PipelineLayoutData>(
            (ref PipelineLayoutData pl) => Device.ReleasePipelineLayout(pl.NativePtr));
        _bindGroups.ForSlice<BindGroupData>(
            (ref BindGroupData bg) => Device.ReleaseBindGroup(bg.NativePtr));
        _bindGroupLayouts.ForSlice<BindGroupLayoutData>(
            (ref BindGroupLayoutData bgl) => Device.ReleaseBindGroupLayout(bgl.NativePtr));
        _shaders.ForSlice<ShaderData>(
            (ref ShaderData s) => Device.ReleaseShaderModule(s.NativePtr));
        _samplers.ForSlice<SamplerData>(
            (ref SamplerData s) => Device.ReleaseSampler(s.NativePtr));
        _textureViews.ForSlice<TextureViewData>(
            (ref TextureViewData tv) => Device.ReleaseTextureView(tv.NativePtr));
        _textures.ForSlice<TextureData>(
            (ref TextureData t) => Device.ReleaseTexture(t.NativePtr));
        _buffers.ForSlice<BufferData>(
            (ref BufferData b) => Device.ReleaseBuffer(b.NativePtr));

        _world.Dispose();

        if (NativeDevice != 0) { Device.ReleaseDevice(NativeDevice); NativeDevice = 0; }
        if (NativeAdapter != 0) { Device.ReleaseAdapter(NativeAdapter); NativeAdapter = 0; }
        if (NativeInstance != 0) { Device.ReleaseInstance(NativeInstance); NativeInstance = 0; }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static unsafe void AdapterCallback(RequestAdapterStatus status, nint adapter, byte* message, void* userdata)
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
    static unsafe void DeviceCallback(RequestDeviceStatus status, nint device, byte* message, void* userdata)
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
