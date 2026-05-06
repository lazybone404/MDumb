namespace Dumb.Emscripten.WGPU;

using System.Runtime.CompilerServices;
using Silk.NET.WebGPU;

public static unsafe partial class WgpuCast
{
    // -- Instance descriptor has different field layouts between emscripten and Silk --

    public static WGPUInstanceDescriptor Cast(this InstanceDescriptor v)
    {
        var native = default(WGPUInstanceDescriptor);
        native.NextInChain = (WGPUChainedStruct*)v.NextInChain;
        // Features left as zero-init (default WGPUInstanceFeatures)
        return native;
    }

    public static InstanceDescriptor Cast(this WGPUInstanceDescriptor v) =>
        new() { NextInChain = (ChainedStruct*)v.NextInChain };

    // -- Structs with field count mismatches not handled by the source generator --

    public static WGPURequestAdapterOptions Cast(this RequestAdapterOptions v) =>
        Unsafe.As<RequestAdapterOptions, WGPURequestAdapterOptions>(ref v);
    public static RequestAdapterOptions Cast(this WGPURequestAdapterOptions v) =>
        Unsafe.As<WGPURequestAdapterOptions, RequestAdapterOptions>(ref v);

    public static WGPUAdapterProperties Cast(this AdapterProperties v) =>
        Unsafe.As<AdapterProperties, WGPUAdapterProperties>(ref v);
    public static AdapterProperties Cast(this WGPUAdapterProperties v) =>
        Unsafe.As<WGPUAdapterProperties, AdapterProperties>(ref v);

    public static WGPUShaderModuleDescriptor Cast(this ShaderModuleDescriptor v) =>
        Unsafe.As<ShaderModuleDescriptor, WGPUShaderModuleDescriptor>(ref v);
    public static ShaderModuleDescriptor Cast(this WGPUShaderModuleDescriptor v) =>
        Unsafe.As<WGPUShaderModuleDescriptor, ShaderModuleDescriptor>(ref v);
}
