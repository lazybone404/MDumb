using Silk.NET.WebGPU;

namespace Dumb.Graphics;

unsafe interface IDeviceBackend
{
    // Instance & adapter
    nint CreateInstance(InstanceDescriptor* descriptor);
    nint GetProcAddress(nint device, byte* procName);
    void InstanceProcessEvents(nint instance);
    void InstanceRequestAdapter(nint instance, RequestAdapterOptions* options,
        delegate* unmanaged[Cdecl]<RequestAdapterStatus, nint, byte*, void*, void> callback, void* userdata);
    void AdapterGetProperties(nint adapter, AdapterProperties* properties);
    void AdapterRequestDevice(nint adapter, DeviceDescriptor* descriptor,
        delegate* unmanaged[Cdecl]<RequestDeviceStatus, nint, byte*, void*, void> callback, void* userdata);
    nint DeviceGetQueue(nint device);

    // Error scopes
    void DevicePushErrorScope(nint device, ErrorFilter filter);
    void DevicePopErrorScope(nint device,
        delegate* unmanaged[Cdecl]<ErrorType, byte*, void*, void> callback, void* userdata);
    void DeviceSetUncapturedErrorCallback(nint device,
        delegate* unmanaged[Cdecl]<ErrorType, byte*, void*, void> callback, void* userdata);

    // Resources
    nint CreateBuffer(nint device, BufferDescriptor* descriptor);
    nint CreateTexture(nint device, TextureDescriptor* descriptor);
    nint CreateTextureView(nint texture, TextureViewDescriptor* descriptor);
    nint CreateSampler(nint device, SamplerDescriptor* descriptor);
    nint CreateShaderModule(nint device, ShaderModuleDescriptor* descriptor);
    nint CreateBindGroupLayout(nint device, BindGroupLayoutDescriptor* descriptor);
    nint CreateBindGroup(nint device, BindGroupDescriptor* descriptor);
    nint CreatePipelineLayout(nint device, PipelineLayoutDescriptor* descriptor);
    nint CreateRenderPipeline(nint device, RenderPipelineDescriptor* descriptor);
    nint CreateComputePipeline(nint device, ComputePipelineDescriptor* descriptor);

    void ReleaseInstance(nint instance);
    void ReleaseAdapter(nint adapter);
    void ReleaseDevice(nint device);
    void ReleaseBuffer(nint buffer);
    void ReleaseTexture(nint texture);
    void ReleaseTextureView(nint textureView);
    void ReleaseSampler(nint sampler);
    void ReleaseShaderModule(nint shaderModule);
    void ReleaseBindGroupLayout(nint bindGroupLayout);
    void ReleaseBindGroup(nint bindGroup);
    void ReleasePipelineLayout(nint pipelineLayout);
    void ReleaseRenderPipeline(nint renderPipeline);
    void ReleaseComputePipeline(nint computePipeline);
}
