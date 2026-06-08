using Silk.NET.WebGPU;
using Dumb.Graphics.Interfaces;

namespace Dumb.Graphics;

/// <summary>
/// 统一 Device 后端 — 封装 Browser (WGPUBrowser) 和 Native (Silk.NET WebGPU) 的差异。
/// 两个平台的方法签名完全相同，仅在 InstanceProcessEvents 和回调处理上有细微差别。
/// </summary>
public sealed unsafe class DeviceBackend : IDeviceBackend
{
#if BROWSER
    private readonly Dumb.Emscripten.WGPUBrowser _wgpu;

    public DeviceBackend(Dumb.Emscripten.WGPUBrowser wgpu) => _wgpu = wgpu;
#else
    private readonly WebGPU _wgpu;

    public DeviceBackend(WebGPU wgpu) => _wgpu = wgpu;
#endif

    // ── Instance & adapter ──────────────────────────────────────

    public nint CreateInstance(InstanceDescriptor* descriptor) =>
        (nint)_wgpu.CreateInstance(descriptor);

    public nint GetProcAddress(nint device, byte* procName) =>
        _wgpu.GetProcAddress((Device*)device, procName);

    public void InstanceProcessEvents(nint instance)
    {
#if BROWSER
        _wgpu.InstanceProcessEvents((Instance*)instance);
#endif
        // Native: no-op (Silk.NET WebGPU doesn't expose instance process events)
    }

    public void InstanceRequestAdapter(nint instance, RequestAdapterOptions* options,
        delegate* unmanaged[Cdecl]<RequestAdapterStatus, nint, byte*, void*, void> callback, void* userdata)
    {
#if BROWSER
        // Browser: function pointer直接作为callback传递
        var cb = (delegate* unmanaged[Cdecl]<RequestAdapterStatus, Adapter*, byte*, void*, void>)(void*)callback;
        _wgpu.InstanceRequestAdapter((Instance*)instance, options, cb, userdata);
#else
        // Native: 需要用 Silk.NET 的 Pfn 包装器
        var cb = new PfnRequestAdapterCallback(
            (delegate* unmanaged[Cdecl]<RequestAdapterStatus, Adapter*, byte*, void*, void>)(void*)callback);
        _wgpu.InstanceRequestAdapter((Instance*)instance, options, cb, userdata);
#endif
    }

    public void AdapterGetProperties(nint adapter, AdapterProperties* properties) =>
        _wgpu.AdapterGetProperties((Adapter*)adapter, properties);

    public void AdapterRequestDevice(nint adapter, DeviceDescriptor* descriptor,
        delegate* unmanaged[Cdecl]<RequestDeviceStatus, nint, byte*, void*, void> callback, void* userdata)
    {
#if BROWSER
        var cb = (delegate* unmanaged[Cdecl]<RequestDeviceStatus, Device*, byte*, void*, void>)(void*)callback;
        _wgpu.AdapterRequestDevice((Adapter*)adapter, descriptor, cb, userdata);
#else
        var cb = new PfnRequestDeviceCallback(
            (delegate* unmanaged[Cdecl]<RequestDeviceStatus, Device*, byte*, void*, void>)(void*)callback);
        _wgpu.AdapterRequestDevice((Adapter*)adapter, descriptor, cb, userdata);
#endif
    }

    public nint DeviceGetQueue(nint device) =>
        (nint)_wgpu.DeviceGetQueue((Device*)device);

    // ── Error scopes ────────────────────────────────────────────

    public void DevicePushErrorScope(nint device, ErrorFilter filter) =>
        _wgpu.DevicePushErrorScope((Device*)device, filter);

    public void DevicePopErrorScope(nint device,
        delegate* unmanaged[Cdecl]<ErrorType, byte*, void*, void> callback, void* userdata) =>
        _wgpu.DevicePopErrorScope((Device*)device, callback, userdata);

    public void DeviceSetUncapturedErrorCallback(nint device,
        delegate* unmanaged[Cdecl]<ErrorType, byte*, void*, void> callback, void* userdata) =>
        _wgpu.DeviceSetUncapturedErrorCallback((Device*)device, callback, userdata);

    // ── Resource creation ───────────────────────────────────────

    public nint CreateBuffer(nint device, BufferDescriptor* descriptor) =>
        (nint)_wgpu.DeviceCreateBuffer((Device*)device, descriptor);

    public nint CreateTexture(nint device, TextureDescriptor* descriptor) =>
        (nint)_wgpu.DeviceCreateTexture((Device*)device, descriptor);

    public nint CreateTextureView(nint texture, TextureViewDescriptor* descriptor) =>
        (nint)_wgpu.TextureCreateView((Texture*)texture, descriptor);

    public nint CreateSampler(nint device, SamplerDescriptor* descriptor) =>
        (nint)_wgpu.DeviceCreateSampler((Device*)device, descriptor);

    public nint CreateShaderModule(nint device, ShaderModuleDescriptor* descriptor) =>
        (nint)_wgpu.DeviceCreateShaderModule((Device*)device, descriptor);

    public nint CreateBindGroupLayout(nint device, BindGroupLayoutDescriptor* descriptor) =>
        (nint)_wgpu.DeviceCreateBindGroupLayout((Device*)device, descriptor);

    public nint CreateBindGroup(nint device, BindGroupDescriptor* descriptor) =>
        (nint)_wgpu.DeviceCreateBindGroup((Device*)device, descriptor);

    public nint CreatePipelineLayout(nint device, PipelineLayoutDescriptor* descriptor) =>
        (nint)_wgpu.DeviceCreatePipelineLayout((Device*)device, descriptor);

    public nint CreateRenderPipeline(nint device, RenderPipelineDescriptor* descriptor) =>
        (nint)_wgpu.DeviceCreateRenderPipeline((Device*)device, descriptor);

    public nint CreateComputePipeline(nint device, ComputePipelineDescriptor* descriptor) =>
        (nint)_wgpu.DeviceCreateComputePipeline((Device*)device, descriptor);

    // ── Resource release ────────────────────────────────────────

    public void ReleaseInstance(nint instance) =>
        _wgpu.InstanceRelease((Instance*)instance);

    public void ReleaseAdapter(nint adapter) =>
        _wgpu.AdapterRelease((Adapter*)adapter);

    public void ReleaseDevice(nint device) =>
        _wgpu.DeviceRelease((Device*)device);

    public void ReleaseBuffer(nint buffer) =>
        _wgpu.BufferRelease((Silk.NET.WebGPU.Buffer*)buffer);

    public void ReleaseTexture(nint texture) =>
        _wgpu.TextureRelease((Texture*)texture);

    public void ReleaseTextureView(nint textureView) =>
        _wgpu.TextureViewRelease((TextureView*)textureView);

    public void ReleaseSampler(nint sampler) =>
        _wgpu.SamplerRelease((Sampler*)sampler);

    public void ReleaseShaderModule(nint shaderModule) =>
        _wgpu.ShaderModuleRelease((ShaderModule*)shaderModule);

    public void ReleaseBindGroupLayout(nint bindGroupLayout) =>
        _wgpu.BindGroupLayoutRelease((BindGroupLayout*)bindGroupLayout);

    public void ReleaseBindGroup(nint bindGroup) =>
        _wgpu.BindGroupRelease((BindGroup*)bindGroup);

    public void ReleasePipelineLayout(nint pipelineLayout) =>
        _wgpu.PipelineLayoutRelease((PipelineLayout*)pipelineLayout);

    public void ReleaseRenderPipeline(nint renderPipeline) =>
        _wgpu.RenderPipelineRelease((RenderPipeline*)renderPipeline);

    public void ReleaseComputePipeline(nint computePipeline) =>
        _wgpu.ComputePipelineRelease((ComputePipeline*)computePipeline);
}
