using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.WebGPU;
using Dumb.Engine.Graph;
#if BROWSER
using Dumb.Graphics.Browser;
#else
using Dumb.Graphics.Native;
#endif

namespace Dumb.Graphics;

public class GraphicsContext : IDisposable
{
    internal readonly Storage<BufferData> _buffers = new();
    internal readonly Storage<TextureData> _textures = new();
    internal readonly Storage<TextureViewData> _textureViews = new();
    internal readonly Storage<SamplerData> _samplers = new();
    internal readonly Storage<ShaderData> _shaders = new();
    internal readonly Storage<BindGroupLayoutData> _bindGroupLayouts = new();
    internal readonly Storage<BindGroupData> _bindGroups = new();
    internal readonly Storage<PipelineLayoutData> _pipelineLayouts = new();
    internal readonly Storage<RenderPipelineData> _renderPipelines = new();
    internal readonly Storage<ComputePipelineData> _computePipelines = new();

    internal nint NativeInstance;
    internal nint NativeAdapter;
    internal nint NativeDevice;
    internal nint NativeQueue;

    internal readonly IDeviceBackend Device;
    internal readonly ICommandBackend Command;

    bool _disposed;

    public GraphicsContext()
    {
#if BROWSER
        var wgpu = new Dumb.Emscripten.WGPUBrowser();
        Device = new BrowserDeviceBackend(wgpu);
        Command = new BrowserCommandBackend(wgpu);
#else
        var wgpu = global::Silk.NET.WebGPU.WebGPU.GetApi();
        Device = new NativeDeviceBackend(wgpu);
        Command = new NativeCommandBackend(wgpu);
#endif
    }

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

    public unsafe void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Release compute pipelines
        { var c = _computePipelines.Cursor(); while (c.Next(out _, out var it, out _)) Device.ReleaseComputePipeline(it.Value.NativePtr); }
        _computePipelines.Dispose();

        // Release render pipelines
        { var c = _renderPipelines.Cursor(); while (c.Next(out _, out var it, out _)) Device.ReleaseRenderPipeline(it.Value.NativePtr); }
        _renderPipelines.Dispose();

        // Release pipeline layouts (free dynamic BGL handle arrays)
        { var c = _pipelineLayouts.Cursor(); while (c.Next(out _, out var it, out _)) {
            ref var pl = ref it.Value;
            if (pl.BindGroupLayoutHandles != null) NativeMemory.Free(pl.BindGroupLayoutHandles);
            Device.ReleasePipelineLayout(pl.NativePtr);
        }}
        _pipelineLayouts.Dispose();

        // Release bind groups
        { var c = _bindGroups.Cursor(); while (c.Next(out _, out var it, out _)) Device.ReleaseBindGroup(it.Value.NativePtr); }
        _bindGroups.Dispose();

        // Release bind group layouts
        { var c = _bindGroupLayouts.Cursor(); while (c.Next(out _, out var it, out _)) Device.ReleaseBindGroupLayout(it.Value.NativePtr); }
        _bindGroupLayouts.Dispose();

        // Release shaders
        { var c = _shaders.Cursor(); while (c.Next(out _, out var it, out _)) Device.ReleaseShaderModule(it.Value.NativePtr); }
        _shaders.Dispose();

        // Release samplers
        { var c = _samplers.Cursor(); while (c.Next(out _, out var it, out _)) Device.ReleaseSampler(it.Value.NativePtr); }
        _samplers.Dispose();

        // Release texture views
        { var c = _textureViews.Cursor(); while (c.Next(out _, out var it, out _)) Device.ReleaseTextureView(it.Value.NativePtr); }
        _textureViews.Dispose();

        // Release textures
        { var c = _textures.Cursor(); while (c.Next(out _, out var it, out _)) Device.ReleaseTexture(it.Value.NativePtr); }
        _textures.Dispose();

        // Release buffers
        { var c = _buffers.Cursor(); while (c.Next(out _, out var it, out _)) Device.ReleaseBuffer(it.Value.NativePtr); }
        _buffers.Dispose();

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
