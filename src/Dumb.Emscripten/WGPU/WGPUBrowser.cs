namespace Dumb.Emscripten;

using System.Runtime.CompilerServices;
using Dumb.Emscripten.WGPU;
using Silk.NET.WebGPU;
using Dawn = Silk.NET.WebGPU.Extensions.Dawn;

public partial class WGPUBrowser : IDisposable
{
    public unsafe Instance* CreateInstance(InstanceDescriptor* descriptor)
    {
        WGPUInstanceDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.CreateInstance(nativeDescriptor));
    }

    public unsafe Instance* CreateInstance(in InstanceDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.CreateInstance(in nativeDescriptor));
    }

    public unsafe nint GetProcAddress(Device* device, byte* procName)
    {
        return WGPUBrowserNative.GetProcAddress(WgpuCast.Cast(device), procName);
    }

    public unsafe nint GetProcAddress(Device* device, in byte procName)
    {
        return WGPUBrowserNative.GetProcAddress(WgpuCast.Cast(device), in procName);
    }

    public unsafe nint GetProcAddress(Device* device, string procName)
    {
        return WGPUBrowserNative.GetProcAddress(WgpuCast.Cast(device), procName);
    }

    public unsafe nuint AdapterEnumerateFeatures(Adapter* adapter, FeatureName* features)
    {
        return WGPUBrowserNative.AdapterEnumerateFeatures(WgpuCast.Cast(adapter), (WGPUFeatureName*)features);
    }

    public unsafe nuint AdapterEnumerateFeatures(Adapter* adapter, ref FeatureName features)
    {
        return WGPUBrowserNative.AdapterEnumerateFeatures(WgpuCast.Cast(adapter),
            ref Unsafe.As<FeatureName, WGPUFeatureName>(ref features));
    }

    public unsafe bool AdapterGetLimits(Adapter* adapter, SupportedLimits* limits)
    {
        if (limits == null)
            return WGPUBrowserNative.AdapterGetLimits(WgpuCast.Cast(adapter), null);
        var nativeLimits = default(WGPUSupportedLimits);
        var result = WGPUBrowserNative.AdapterGetLimits(WgpuCast.Cast(adapter), &nativeLimits);
        *limits = nativeLimits.Cast();
        return result;
    }

    public unsafe bool AdapterGetLimits(Adapter* adapter, ref SupportedLimits limits)
    {
        return WGPUBrowserNative.AdapterGetLimits(WgpuCast.Cast(adapter),
            ref Unsafe.As<SupportedLimits, WGPUSupportedLimits>(ref limits));
    }

    public unsafe void AdapterGetProperties(Adapter* adapter, AdapterProperties* properties)
    {
        if (properties == null)
        {
            WGPUBrowserNative.AdapterGetProperties(WgpuCast.Cast(adapter), null);
            return;
        }

        var nativeProperties = default(WGPUAdapterProperties);
        WGPUBrowserNative.AdapterGetProperties(WgpuCast.Cast(adapter), &nativeProperties);
        *properties = nativeProperties.Cast();
    }

    public unsafe void AdapterGetProperties(Adapter* adapter, ref AdapterProperties properties)
    {
        WGPUBrowserNative.AdapterGetProperties(WgpuCast.Cast(adapter),
            ref Unsafe.As<AdapterProperties, WGPUAdapterProperties>(ref properties));
    }

    public unsafe bool AdapterHasFeature(Adapter* adapter, FeatureName feature)
    {
        return WGPUBrowserNative.AdapterHasFeature(WgpuCast.Cast(adapter), feature.Cast());
    }

    public unsafe void AdapterRequestDevice(Adapter* adapter, DeviceDescriptor* descriptor,
        delegate* unmanaged[Cdecl]<RequestDeviceStatus, Device*, byte*, void*, void> callback, void* userdata)
    {
        WGPUDeviceDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        WGPUBrowserNative.AdapterRequestDevice(WgpuCast.Cast(adapter), nativeDescriptor, (nint)callback, userdata);
    }

    public unsafe void AdapterRequestDevice(Adapter* adapter, in DeviceDescriptor descriptor,
        delegate* unmanaged[Cdecl]<RequestDeviceStatus, Device*, byte*, void*, void> callback, void* userdata)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        WGPUBrowserNative.AdapterRequestDevice(WgpuCast.Cast(adapter), in nativeDescriptor, (nint)callback, userdata);
    }

    public unsafe void AdapterReference(Adapter* adapter)
    {
        WGPUBrowserNative.AdapterReference(WgpuCast.Cast(adapter));
    }

    public unsafe void AdapterRelease(Adapter* adapter)
    {
        WGPUBrowserNative.AdapterRelease(WgpuCast.Cast(adapter));
    }

    public unsafe void BindGroupSetLabel(BindGroup* bindGroup, byte* label)
    {
        WGPUBrowserNative.BindGroupSetLabel(WgpuCast.Cast(bindGroup), label);
    }

    public unsafe void BindGroupSetLabel(BindGroup* bindGroup, in byte label)
    {
        WGPUBrowserNative.BindGroupSetLabel(WgpuCast.Cast(bindGroup), in label);
    }

    public unsafe void BindGroupSetLabel(BindGroup* bindGroup, string label)
    {
        WGPUBrowserNative.BindGroupSetLabel(WgpuCast.Cast(bindGroup), label);
    }

    public unsafe void BindGroupReference(BindGroup* bindGroup)
    {
        WGPUBrowserNative.BindGroupReference(WgpuCast.Cast(bindGroup));
    }

    public unsafe void BindGroupRelease(BindGroup* bindGroup)
    {
        WGPUBrowserNative.BindGroupRelease(WgpuCast.Cast(bindGroup));
    }

    public unsafe void BindGroupLayoutSetLabel(BindGroupLayout* bindGroupLayout, byte* label)
    {
        WGPUBrowserNative.BindGroupLayoutSetLabel(WgpuCast.Cast(bindGroupLayout), label);
    }

    public unsafe void BindGroupLayoutSetLabel(BindGroupLayout* bindGroupLayout, in byte label)
    {
        WGPUBrowserNative.BindGroupLayoutSetLabel(WgpuCast.Cast(bindGroupLayout), in label);
    }

    public unsafe void BindGroupLayoutSetLabel(BindGroupLayout* bindGroupLayout, string label)
    {
        WGPUBrowserNative.BindGroupLayoutSetLabel(WgpuCast.Cast(bindGroupLayout), label);
    }

    public unsafe void BindGroupLayoutReference(BindGroupLayout* bindGroupLayout)
    {
        WGPUBrowserNative.BindGroupLayoutReference(WgpuCast.Cast(bindGroupLayout));
    }

    public unsafe void BindGroupLayoutRelease(BindGroupLayout* bindGroupLayout)
    {
        WGPUBrowserNative.BindGroupLayoutRelease(WgpuCast.Cast(bindGroupLayout));
    }

    public unsafe void BufferDestroy(Buffer* buffer)
    {
        WGPUBrowserNative.BufferDestroy(WgpuCast.Cast(buffer));
    }

    public unsafe void* BufferGetConstMappedRange(Buffer* buffer, nuint offset, nuint size)
    {
        return WGPUBrowserNative.BufferGetConstMappedRange(WgpuCast.Cast(buffer), offset, size);
    }

    public unsafe BufferMapState BufferGetMapState(Buffer* buffer)
    {
        return WGPUBrowserNative.BufferGetMapState(WgpuCast.Cast(buffer)).Cast();
    }

    public unsafe void* BufferGetMappedRange(Buffer* buffer, nuint offset, nuint size)
    {
        return WGPUBrowserNative.BufferGetMappedRange(WgpuCast.Cast(buffer), offset, size);
    }

    public unsafe ulong BufferGetSize(Buffer* buffer)
    {
        return WGPUBrowserNative.BufferGetSize(WgpuCast.Cast(buffer));
    }

    public unsafe BufferUsage BufferGetUsage(Buffer* buffer)
    {
        return (BufferUsage)WGPUBrowserNative.BufferGetUsage(WgpuCast.Cast(buffer));
    }

    public unsafe void BufferMapAsync(Buffer* buffer, MapMode mode, nuint offset, nuint size, nint callback,
        void* userdata)
    {
        WGPUBrowserNative.BufferMapAsync(WgpuCast.Cast(buffer), (WGPUMapMode)mode, offset, size, callback, userdata);
    }

    public unsafe void BufferSetLabel(Buffer* buffer, byte* label)
    {
        WGPUBrowserNative.BufferSetLabel(WgpuCast.Cast(buffer), label);
    }

    public unsafe void BufferSetLabel(Buffer* buffer, in byte label)
    {
        WGPUBrowserNative.BufferSetLabel(WgpuCast.Cast(buffer), in label);
    }

    public unsafe void BufferSetLabel(Buffer* buffer, string label)
    {
        WGPUBrowserNative.BufferSetLabel(WgpuCast.Cast(buffer), label);
    }

    public unsafe void BufferUnmap(Buffer* buffer)
    {
        WGPUBrowserNative.BufferUnmap(WgpuCast.Cast(buffer));
    }

    public unsafe void BufferReference(Buffer* buffer)
    {
        WGPUBrowserNative.BufferReference(WgpuCast.Cast(buffer));
    }

    public unsafe void BufferRelease(Buffer* buffer)
    {
        WGPUBrowserNative.BufferRelease(WgpuCast.Cast(buffer));
    }

    public unsafe void CommandBufferSetLabel(CommandBuffer* commandBuffer, byte* label)
    {
        WGPUBrowserNative.CommandBufferSetLabel(WgpuCast.Cast(commandBuffer), label);
    }

    public unsafe void CommandBufferSetLabel(CommandBuffer* commandBuffer, in byte label)
    {
        WGPUBrowserNative.CommandBufferSetLabel(WgpuCast.Cast(commandBuffer), in label);
    }

    public unsafe void CommandBufferSetLabel(CommandBuffer* commandBuffer, string label)
    {
        WGPUBrowserNative.CommandBufferSetLabel(WgpuCast.Cast(commandBuffer), label);
    }

    public unsafe void CommandBufferReference(CommandBuffer* commandBuffer)
    {
        WGPUBrowserNative.CommandBufferReference(WgpuCast.Cast(commandBuffer));
    }

    public unsafe void CommandBufferRelease(CommandBuffer* commandBuffer)
    {
        WGPUBrowserNative.CommandBufferRelease(WgpuCast.Cast(commandBuffer));
    }

    public unsafe ComputePassEncoder* CommandEncoderBeginComputePass(CommandEncoder* commandEncoder,
        ComputePassDescriptor* descriptor)
    {
        WGPUComputePassDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(
            WGPUBrowserNative.CommandEncoderBeginComputePass(WgpuCast.Cast(commandEncoder), nativeDescriptor));
    }

    public unsafe ComputePassEncoder* CommandEncoderBeginComputePass(CommandEncoder* commandEncoder,
        in ComputePassDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(
            WGPUBrowserNative.CommandEncoderBeginComputePass(WgpuCast.Cast(commandEncoder), in nativeDescriptor));
    }

    public unsafe RenderPassEncoder* CommandEncoderBeginRenderPass(CommandEncoder* commandEncoder,
        RenderPassDescriptor* descriptor)
    {
        WGPURenderPassDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(
            WGPUBrowserNative.CommandEncoderBeginRenderPass(WgpuCast.Cast(commandEncoder), nativeDescriptor));
    }

    public unsafe RenderPassEncoder* CommandEncoderBeginRenderPass(CommandEncoder* commandEncoder,
        in RenderPassDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(
            WGPUBrowserNative.CommandEncoderBeginRenderPass(WgpuCast.Cast(commandEncoder), in nativeDescriptor));
    }

    public unsafe void CommandEncoderClearBuffer(CommandEncoder* commandEncoder, Buffer* buffer, ulong offset,
        ulong size)
    {
        WGPUBrowserNative.CommandEncoderClearBuffer(WgpuCast.Cast(commandEncoder), WgpuCast.Cast(buffer), offset, size);
    }

    public unsafe void CommandEncoderCopyBufferToBuffer(CommandEncoder* commandEncoder, Buffer* source,
        ulong sourceOffset, Buffer* destination, ulong destinationOffset, ulong size)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToBuffer(WgpuCast.Cast(commandEncoder), WgpuCast.Cast(source),
            sourceOffset, WgpuCast.Cast(destination), destinationOffset, size);
    }

    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source,
        ImageCopyTexture* destination, Extent3D* copySize)
    {
        WGPUImageCopyBuffer* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(WgpuCast.Cast(commandEncoder), nativeSource, nativeDest,
            (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source,
        ImageCopyTexture* destination, in Extent3D copySize)
    {
        WGPUImageCopyBuffer* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(WgpuCast.Cast(commandEncoder), nativeSource, nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source,
        in ImageCopyTexture destination, Extent3D* copySize)
    {
        WGPUImageCopyBuffer* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(WgpuCast.Cast(commandEncoder), nativeSource, in nativeDest,
            (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source,
        in ImageCopyTexture destination, in Extent3D copySize)
    {
        WGPUImageCopyBuffer* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(WgpuCast.Cast(commandEncoder), nativeSource, in nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source,
        ImageCopyTexture* destination, Extent3D* copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(WgpuCast.Cast(commandEncoder), in nativeSource, nativeDest,
            (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source,
        ImageCopyTexture* destination, in Extent3D copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(WgpuCast.Cast(commandEncoder), in nativeSource, nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source,
        in ImageCopyTexture destination, Extent3D* copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(WgpuCast.Cast(commandEncoder), in nativeSource,
            in nativeDest, (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source,
        in ImageCopyTexture destination, in Extent3D copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(WgpuCast.Cast(commandEncoder), in nativeSource,
            in nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source,
        ImageCopyBuffer* destination, Extent3D* copySize)
    {
        WGPUImageCopyTexture* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        WGPUImageCopyBuffer* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(WgpuCast.Cast(commandEncoder), nativeSource, nativeDest,
            (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source,
        ImageCopyBuffer* destination, in Extent3D copySize)
    {
        WGPUImageCopyTexture* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        WGPUImageCopyBuffer* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(WgpuCast.Cast(commandEncoder), nativeSource, nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source,
        in ImageCopyBuffer destination, Extent3D* copySize)
    {
        WGPUImageCopyTexture* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(WgpuCast.Cast(commandEncoder), nativeSource, in nativeDest,
            (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source,
        in ImageCopyBuffer destination, in Extent3D copySize)
    {
        WGPUImageCopyTexture* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(WgpuCast.Cast(commandEncoder), nativeSource, in nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source,
        ImageCopyBuffer* destination, Extent3D* copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        WGPUImageCopyBuffer* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(WgpuCast.Cast(commandEncoder), in nativeSource, nativeDest,
            (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source,
        ImageCopyBuffer* destination, in Extent3D copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        WGPUImageCopyBuffer* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(WgpuCast.Cast(commandEncoder), in nativeSource, nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source,
        in ImageCopyBuffer destination, Extent3D* copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(WgpuCast.Cast(commandEncoder), in nativeSource,
            in nativeDest, (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source,
        in ImageCopyBuffer destination, in Extent3D copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(WgpuCast.Cast(commandEncoder), in nativeSource,
            in nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source,
        ImageCopyTexture* destination, Extent3D* copySize)
    {
        WGPUImageCopyTexture* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(WgpuCast.Cast(commandEncoder), nativeSource, nativeDest,
            (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source,
        ImageCopyTexture* destination, in Extent3D copySize)
    {
        WGPUImageCopyTexture* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(WgpuCast.Cast(commandEncoder), nativeSource, nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source,
        in ImageCopyTexture destination, Extent3D* copySize)
    {
        WGPUImageCopyTexture* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(WgpuCast.Cast(commandEncoder), nativeSource, in nativeDest,
            (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source,
        in ImageCopyTexture destination, in Extent3D copySize)
    {
        WGPUImageCopyTexture* nativeSource = null;
        if (source != null)
        {
            var nativeSourceValue = (*source).Cast();
            nativeSource = &nativeSourceValue;
        }

        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(WgpuCast.Cast(commandEncoder), nativeSource, in nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source,
        ImageCopyTexture* destination, Extent3D* copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(WgpuCast.Cast(commandEncoder), in nativeSource, nativeDest,
            (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source,
        ImageCopyTexture* destination, in Extent3D copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(WgpuCast.Cast(commandEncoder), in nativeSource, nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source,
        in ImageCopyTexture destination, Extent3D* copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(WgpuCast.Cast(commandEncoder), in nativeSource,
            in nativeDest, (WGPUExtent3D*)copySize);
    }

    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source,
        in ImageCopyTexture destination, in Extent3D copySize)
    {
        var s = source;
        var nativeSource = s.Cast();
        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(WgpuCast.Cast(commandEncoder), in nativeSource,
            in nativeDest,
            in Unsafe.AsRef(in copySize).Cast());
    }

    public unsafe CommandBuffer* CommandEncoderFinish(CommandEncoder* commandEncoder,
        CommandBufferDescriptor* descriptor)
    {
        WGPUCommandBufferDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.CommandEncoderFinish(WgpuCast.Cast(commandEncoder), nativeDescriptor));
    }

    public unsafe CommandBuffer* CommandEncoderFinish(CommandEncoder* commandEncoder,
        in CommandBufferDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(
            WGPUBrowserNative.CommandEncoderFinish(WgpuCast.Cast(commandEncoder), in nativeDescriptor));
    }

    public unsafe void CommandEncoderInsertDebugMarker(CommandEncoder* commandEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.CommandEncoderInsertDebugMarker(WgpuCast.Cast(commandEncoder), markerLabel);
    }

    public unsafe void CommandEncoderInsertDebugMarker(CommandEncoder* commandEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.CommandEncoderInsertDebugMarker(WgpuCast.Cast(commandEncoder), in markerLabel);
    }

    public unsafe void CommandEncoderInsertDebugMarker(CommandEncoder* commandEncoder, string markerLabel)
    {
        WGPUBrowserNative.CommandEncoderInsertDebugMarker(WgpuCast.Cast(commandEncoder), markerLabel);
    }

    public unsafe void CommandEncoderPopDebugGroup(CommandEncoder* commandEncoder)
    {
        WGPUBrowserNative.CommandEncoderPopDebugGroup(WgpuCast.Cast(commandEncoder));
    }

    public unsafe void CommandEncoderPushDebugGroup(CommandEncoder* commandEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.CommandEncoderPushDebugGroup(WgpuCast.Cast(commandEncoder), groupLabel);
    }

    public unsafe void CommandEncoderPushDebugGroup(CommandEncoder* commandEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.CommandEncoderPushDebugGroup(WgpuCast.Cast(commandEncoder), in groupLabel);
    }

    public unsafe void CommandEncoderPushDebugGroup(CommandEncoder* commandEncoder, string groupLabel)
    {
        WGPUBrowserNative.CommandEncoderPushDebugGroup(WgpuCast.Cast(commandEncoder), groupLabel);
    }

    public unsafe void CommandEncoderResolveQuerySet(CommandEncoder* commandEncoder, QuerySet* querySet,
        uint firstQuery, uint queryCount, Buffer* destination, ulong destinationOffset)
    {
        WGPUBrowserNative.CommandEncoderResolveQuerySet(WgpuCast.Cast(commandEncoder), WgpuCast.Cast(querySet),
            firstQuery, queryCount, WgpuCast.Cast(destination), destinationOffset);
    }

    public unsafe void CommandEncoderSetLabel(CommandEncoder* commandEncoder, byte* label)
    {
        WGPUBrowserNative.CommandEncoderSetLabel(WgpuCast.Cast(commandEncoder), label);
    }

    public unsafe void CommandEncoderSetLabel(CommandEncoder* commandEncoder, in byte label)
    {
        WGPUBrowserNative.CommandEncoderSetLabel(WgpuCast.Cast(commandEncoder), in label);
    }

    public unsafe void CommandEncoderSetLabel(CommandEncoder* commandEncoder, string label)
    {
        WGPUBrowserNative.CommandEncoderSetLabel(WgpuCast.Cast(commandEncoder), label);
    }

    public unsafe void CommandEncoderWriteTimestamp(CommandEncoder* commandEncoder, QuerySet* querySet, uint queryIndex)
    {
        WGPUBrowserNative.CommandEncoderWriteTimestamp(WgpuCast.Cast(commandEncoder), WgpuCast.Cast(querySet),
            queryIndex);
    }

    public unsafe void CommandEncoderReference(CommandEncoder* commandEncoder)
    {
        WGPUBrowserNative.CommandEncoderReference(WgpuCast.Cast(commandEncoder));
    }

    public unsafe void CommandEncoderRelease(CommandEncoder* commandEncoder)
    {
        WGPUBrowserNative.CommandEncoderRelease(WgpuCast.Cast(commandEncoder));
    }

    public unsafe void ComputePassEncoderBeginPipelineStatisticsQuery(ComputePassEncoder* computePassEncoder,
        QuerySet* querySet, uint queryIndex)
    {
        WGPUBrowserNative.ComputePassEncoderBeginPipelineStatisticsQuery(WgpuCast.Cast(computePassEncoder),
            WgpuCast.Cast(querySet), queryIndex);
    }

    public unsafe void ComputePassEncoderDispatchWorkgroups(ComputePassEncoder* computePassEncoder,
        uint workgroupCountX, uint workgroupCountY, uint workgroupCountZ)
    {
        WGPUBrowserNative.ComputePassEncoderDispatchWorkgroups(WgpuCast.Cast(computePassEncoder), workgroupCountX,
            workgroupCountY, workgroupCountZ);
    }

    public unsafe void ComputePassEncoderDispatchWorkgroupsIndirect(ComputePassEncoder* computePassEncoder,
        Buffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.ComputePassEncoderDispatchWorkgroupsIndirect(WgpuCast.Cast(computePassEncoder),
            WgpuCast.Cast(indirectBuffer), indirectOffset);
    }

    public unsafe void ComputePassEncoderEnd(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderEnd(WgpuCast.Cast(computePassEncoder));
    }

    public unsafe void ComputePassEncoderEndPipelineStatisticsQuery(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderEndPipelineStatisticsQuery(WgpuCast.Cast(computePassEncoder));
    }

    public unsafe void ComputePassEncoderInsertDebugMarker(ComputePassEncoder* computePassEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.ComputePassEncoderInsertDebugMarker(WgpuCast.Cast(computePassEncoder), markerLabel);
    }

    public unsafe void ComputePassEncoderInsertDebugMarker(ComputePassEncoder* computePassEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.ComputePassEncoderInsertDebugMarker(WgpuCast.Cast(computePassEncoder), in markerLabel);
    }

    public unsafe void ComputePassEncoderInsertDebugMarker(ComputePassEncoder* computePassEncoder, string markerLabel)
    {
        WGPUBrowserNative.ComputePassEncoderInsertDebugMarker(WgpuCast.Cast(computePassEncoder), markerLabel);
    }

    public unsafe void ComputePassEncoderPopDebugGroup(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderPopDebugGroup(WgpuCast.Cast(computePassEncoder));
    }

    public unsafe void ComputePassEncoderPushDebugGroup(ComputePassEncoder* computePassEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.ComputePassEncoderPushDebugGroup(WgpuCast.Cast(computePassEncoder), groupLabel);
    }

    public unsafe void ComputePassEncoderPushDebugGroup(ComputePassEncoder* computePassEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.ComputePassEncoderPushDebugGroup(WgpuCast.Cast(computePassEncoder), in groupLabel);
    }

    public unsafe void ComputePassEncoderPushDebugGroup(ComputePassEncoder* computePassEncoder, string groupLabel)
    {
        WGPUBrowserNative.ComputePassEncoderPushDebugGroup(WgpuCast.Cast(computePassEncoder), groupLabel);
    }

    public unsafe void ComputePassEncoderSetBindGroup(ComputePassEncoder* computePassEncoder, uint groupIndex,
        BindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets)
    {
        WGPUBrowserNative.ComputePassEncoderSetBindGroup(WgpuCast.Cast(computePassEncoder), groupIndex,
            WgpuCast.Cast(group), dynamicOffsetCount, dynamicOffsets);
    }

    public unsafe void ComputePassEncoderSetBindGroup(ComputePassEncoder* computePassEncoder, uint groupIndex,
        BindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets)
    {
        WGPUBrowserNative.ComputePassEncoderSetBindGroup(WgpuCast.Cast(computePassEncoder), groupIndex,
            WgpuCast.Cast(group), dynamicOffsetCount, in dynamicOffsets);
    }

    public unsafe void ComputePassEncoderSetLabel(ComputePassEncoder* computePassEncoder, byte* label)
    {
        WGPUBrowserNative.ComputePassEncoderSetLabel(WgpuCast.Cast(computePassEncoder), label);
    }

    public unsafe void ComputePassEncoderSetLabel(ComputePassEncoder* computePassEncoder, in byte label)
    {
        WGPUBrowserNative.ComputePassEncoderSetLabel(WgpuCast.Cast(computePassEncoder), in label);
    }

    public unsafe void ComputePassEncoderSetLabel(ComputePassEncoder* computePassEncoder, string label)
    {
        WGPUBrowserNative.ComputePassEncoderSetLabel(WgpuCast.Cast(computePassEncoder), label);
    }

    public unsafe void ComputePassEncoderSetPipeline(ComputePassEncoder* computePassEncoder, ComputePipeline* pipeline)
    {
        WGPUBrowserNative.ComputePassEncoderSetPipeline(WgpuCast.Cast(computePassEncoder), WgpuCast.Cast(pipeline));
    }

    public unsafe void ComputePassEncoderReference(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderReference(WgpuCast.Cast(computePassEncoder));
    }

    public unsafe void ComputePassEncoderRelease(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderRelease(WgpuCast.Cast(computePassEncoder));
    }

    public unsafe BindGroupLayout* ComputePipelineGetBindGroupLayout(ComputePipeline* computePipeline, uint groupIndex)
    {
        return WgpuCast.Cast(
            WGPUBrowserNative.ComputePipelineGetBindGroupLayout(WgpuCast.Cast(computePipeline), groupIndex));
    }

    public unsafe void ComputePipelineSetLabel(ComputePipeline* computePipeline, byte* label)
    {
        WGPUBrowserNative.ComputePipelineSetLabel(WgpuCast.Cast(computePipeline), label);
    }

    public unsafe void ComputePipelineSetLabel(ComputePipeline* computePipeline, in byte label)
    {
        WGPUBrowserNative.ComputePipelineSetLabel(WgpuCast.Cast(computePipeline), in label);
    }

    public unsafe void ComputePipelineSetLabel(ComputePipeline* computePipeline, string label)
    {
        WGPUBrowserNative.ComputePipelineSetLabel(WgpuCast.Cast(computePipeline), label);
    }

    public unsafe void ComputePipelineReference(ComputePipeline* computePipeline)
    {
        WGPUBrowserNative.ComputePipelineReference(WgpuCast.Cast(computePipeline));
    }

    public unsafe void ComputePipelineRelease(ComputePipeline* computePipeline)
    {
        WGPUBrowserNative.ComputePipelineRelease(WgpuCast.Cast(computePipeline));
    }

    public unsafe BindGroup* DeviceCreateBindGroup(Device* device, BindGroupDescriptor* descriptor)
    {
        WGPUBindGroupDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateBindGroup(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe BindGroup* DeviceCreateBindGroup(Device* device, in BindGroupDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateBindGroup(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe BindGroupLayout* DeviceCreateBindGroupLayout(Device* device, BindGroupLayoutDescriptor* descriptor)
    {
        WGPUBindGroupLayoutDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateBindGroupLayout(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe BindGroupLayout* DeviceCreateBindGroupLayout(Device* device, in BindGroupLayoutDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateBindGroupLayout(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe Buffer* DeviceCreateBuffer(Device* device, BufferDescriptor* descriptor)
    {
        WGPUBufferDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateBuffer(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe Buffer* DeviceCreateBuffer(Device* device, in BufferDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateBuffer(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe CommandEncoder* DeviceCreateCommandEncoder(Device* device, CommandEncoderDescriptor* descriptor)
    {
        WGPUCommandEncoderDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateCommandEncoder(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe CommandEncoder* DeviceCreateCommandEncoder(Device* device, in CommandEncoderDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateCommandEncoder(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe ComputePipeline* DeviceCreateComputePipeline(Device* device, ComputePipelineDescriptor* descriptor)
    {
        WGPUComputePipelineDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateComputePipeline(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe ComputePipeline* DeviceCreateComputePipeline(Device* device, in ComputePipelineDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateComputePipeline(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe void DeviceCreateComputePipelineAsync(Device* device, ComputePipelineDescriptor* descriptor,
        nint callback, void* userdata)
    {
        WGPUComputePipelineDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        WGPUBrowserNative.DeviceCreateComputePipelineAsync(WgpuCast.Cast(device), nativeDescriptor, callback, userdata);
    }

    public unsafe void DeviceCreateComputePipelineAsync(Device* device, in ComputePipelineDescriptor descriptor,
        nint callback, void* userdata)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        WGPUBrowserNative.DeviceCreateComputePipelineAsync(WgpuCast.Cast(device), in nativeDescriptor, callback,
            userdata);
    }

    public unsafe PipelineLayout* DeviceCreatePipelineLayout(Device* device, PipelineLayoutDescriptor* descriptor)
    {
        WGPUPipelineLayoutDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreatePipelineLayout(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe PipelineLayout* DeviceCreatePipelineLayout(Device* device, in PipelineLayoutDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreatePipelineLayout(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe QuerySet* DeviceCreateQuerySet(Device* device, QuerySetDescriptor* descriptor)
    {
        WGPUQuerySetDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateQuerySet(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe QuerySet* DeviceCreateQuerySet(Device* device, in QuerySetDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateQuerySet(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe RenderBundleEncoder* DeviceCreateRenderBundleEncoder(Device* device,
        RenderBundleEncoderDescriptor* descriptor)
    {
        WGPURenderBundleEncoderDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(
            WGPUBrowserNative.DeviceCreateRenderBundleEncoder(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe RenderBundleEncoder* DeviceCreateRenderBundleEncoder(Device* device,
        in RenderBundleEncoderDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(
            WGPUBrowserNative.DeviceCreateRenderBundleEncoder(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe RenderPipeline* DeviceCreateRenderPipeline(Device* device, RenderPipelineDescriptor* descriptor)
    {
        WGPURenderPipelineDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateRenderPipeline(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe RenderPipeline* DeviceCreateRenderPipeline(Device* device, in RenderPipelineDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateRenderPipeline(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe void DeviceCreateRenderPipelineAsync(Device* device, RenderPipelineDescriptor* descriptor,
        nint callback, void* userdata)
    {
        WGPURenderPipelineDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        WGPUBrowserNative.DeviceCreateRenderPipelineAsync(WgpuCast.Cast(device), nativeDescriptor, callback, userdata);
    }

    public unsafe void DeviceCreateRenderPipelineAsync(Device* device, in RenderPipelineDescriptor descriptor,
        nint callback, void* userdata)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        WGPUBrowserNative.DeviceCreateRenderPipelineAsync(WgpuCast.Cast(device), in nativeDescriptor, callback,
            userdata);
    }

    public unsafe Sampler* DeviceCreateSampler(Device* device, SamplerDescriptor* descriptor)
    {
        WGPUSamplerDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateSampler(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe Sampler* DeviceCreateSampler(Device* device, in SamplerDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateSampler(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe ShaderModule* DeviceCreateShaderModule(Device* device, ShaderModuleDescriptor* descriptor)
    {
        WGPUShaderModuleDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateShaderModule(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe ShaderModule* DeviceCreateShaderModule(Device* device, in ShaderModuleDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateShaderModule(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe Texture* DeviceCreateTexture(Device* device, TextureDescriptor* descriptor)
    {
        WGPUTextureDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateTexture(WgpuCast.Cast(device), nativeDescriptor));
    }

    public unsafe Texture* DeviceCreateTexture(Device* device, in TextureDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.DeviceCreateTexture(WgpuCast.Cast(device), in nativeDescriptor));
    }

    public unsafe void DeviceDestroy(Device* device)
    {
        WGPUBrowserNative.DeviceDestroy(WgpuCast.Cast(device));
    }

    public unsafe nuint DeviceEnumerateFeatures(Device* device, FeatureName* features)
    {
        return WGPUBrowserNative.DeviceEnumerateFeatures(WgpuCast.Cast(device), (WGPUFeatureName*)features);
    }

    public unsafe nuint DeviceEnumerateFeatures(Device* device, ref FeatureName features)
    {
        return WGPUBrowserNative.DeviceEnumerateFeatures(WgpuCast.Cast(device),
            ref Unsafe.As<FeatureName, WGPUFeatureName>(ref features));
    }

    public unsafe bool DeviceGetLimits(Device* device, SupportedLimits* limits)
    {
        if (limits == null)
            return WGPUBrowserNative.DeviceGetLimits(WgpuCast.Cast(device), null);
        var nativeLimits = default(WGPUSupportedLimits);
        var result = WGPUBrowserNative.DeviceGetLimits(WgpuCast.Cast(device), &nativeLimits);
        *limits = nativeLimits.Cast();
        return result;
    }

    public unsafe bool DeviceGetLimits(Device* device, ref SupportedLimits limits)
    {
        return WGPUBrowserNative.DeviceGetLimits(WgpuCast.Cast(device),
            ref Unsafe.As<SupportedLimits, WGPUSupportedLimits>(ref limits));
    }

    public unsafe Queue* DeviceGetQueue(Device* device)
    {
        return WgpuCast.Cast(WGPUBrowserNative.DeviceGetQueue(WgpuCast.Cast(device)));
    }

    public unsafe bool DeviceHasFeature(Device* device, FeatureName feature)
    {
        return WGPUBrowserNative.DeviceHasFeature(WgpuCast.Cast(device), feature.Cast());
    }

    public unsafe void DevicePopErrorScope(Device* device, PfnErrorCallback callback, void* userdata)
    {
        WGPUBrowserNative.DevicePopErrorScope(WgpuCast.Cast(device), callback, userdata);
    }

    public unsafe void DevicePushErrorScope(Device* device, ErrorFilter filter)
    {
        WGPUBrowserNative.DevicePushErrorScope(WgpuCast.Cast(device), filter.Cast());
    }

    public unsafe void DeviceSetLabel(Device* device, byte* label)
    {
        WGPUBrowserNative.DeviceSetLabel(WgpuCast.Cast(device), label);
    }

    public unsafe void DeviceSetLabel(Device* device, in byte label)
    {
        WGPUBrowserNative.DeviceSetLabel(WgpuCast.Cast(device), in label);
    }

    public unsafe void DeviceSetLabel(Device* device, string label)
    {
        WGPUBrowserNative.DeviceSetLabel(WgpuCast.Cast(device), label);
    }

    public unsafe void DeviceSetUncapturedErrorCallback(Device* device, PfnErrorCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceSetUncapturedErrorCallback(WgpuCast.Cast(device), callback, userdata);
    }

    public unsafe void DeviceReference(Device* device)
    {
        WGPUBrowserNative.DeviceReference(WgpuCast.Cast(device));
    }

    public unsafe void DeviceRelease(Device* device)
    {
        WGPUBrowserNative.DeviceRelease(WgpuCast.Cast(device));
    }

    public unsafe Surface* InstanceCreateSurface(Instance* instance, SurfaceDescriptor* descriptor)
    {
        WGPUSurfaceDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.InstanceCreateSurface(WgpuCast.Cast(instance), nativeDescriptor));
    }

    public unsafe Surface* InstanceCreateSurface(Instance* instance, in SurfaceDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.InstanceCreateSurface(WgpuCast.Cast(instance), in nativeDescriptor));
    }

    public unsafe void InstanceProcessEvents(Instance* instance)
    {
        WGPUBrowserNative.InstanceProcessEvents(WgpuCast.Cast(instance));
    }

    public unsafe void InstanceRequestAdapter(Instance* instance, RequestAdapterOptions* options,
        delegate* unmanaged[Cdecl]<RequestAdapterStatus, Adapter*, byte*, void*, void> callback, void* userdata)
    {
        WGPURequestAdapterOptions* nativeOptions = null;
        if (options != null)
        {
            var nativeOptionsValue = (*options).Cast();
            nativeOptions = &nativeOptionsValue;
        }

        WGPUBrowserNative.InstanceRequestAdapter(WgpuCast.Cast(instance), nativeOptions, (nint)callback, userdata);
    }

    public unsafe void InstanceRequestAdapter(Instance* instance, in RequestAdapterOptions options,
        delegate* unmanaged[Cdecl]<RequestAdapterStatus, Adapter*, byte*, void*, void> callback, void* userdata)
    {
        var opts = options;
        var nativeOptions = opts.Cast();
        WGPUBrowserNative.InstanceRequestAdapter(WgpuCast.Cast(instance), in nativeOptions, (nint)callback, userdata);
    }

    public unsafe void InstanceReference(Instance* instance)
    {
        WGPUBrowserNative.InstanceReference(WgpuCast.Cast(instance));
    }

    public unsafe void InstanceRelease(Instance* instance)
    {
        WGPUBrowserNative.InstanceRelease(WgpuCast.Cast(instance));
    }

    public unsafe void PipelineLayoutSetLabel(PipelineLayout* pipelineLayout, byte* label)
    {
        WGPUBrowserNative.PipelineLayoutSetLabel(WgpuCast.Cast(pipelineLayout), label);
    }

    public unsafe void PipelineLayoutSetLabel(PipelineLayout* pipelineLayout, in byte label)
    {
        WGPUBrowserNative.PipelineLayoutSetLabel(WgpuCast.Cast(pipelineLayout), in label);
    }

    public unsafe void PipelineLayoutSetLabel(PipelineLayout* pipelineLayout, string label)
    {
        WGPUBrowserNative.PipelineLayoutSetLabel(WgpuCast.Cast(pipelineLayout), label);
    }

    public unsafe void PipelineLayoutReference(PipelineLayout* pipelineLayout)
    {
        WGPUBrowserNative.PipelineLayoutReference(WgpuCast.Cast(pipelineLayout));
    }

    public unsafe void PipelineLayoutRelease(PipelineLayout* pipelineLayout)
    {
        WGPUBrowserNative.PipelineLayoutRelease(WgpuCast.Cast(pipelineLayout));
    }

    public unsafe void QuerySetDestroy(QuerySet* querySet)
    {
        WGPUBrowserNative.QuerySetDestroy(WgpuCast.Cast(querySet));
    }

    public unsafe uint QuerySetGetCount(QuerySet* querySet)
    {
        return WGPUBrowserNative.QuerySetGetCount(WgpuCast.Cast(querySet));
    }

    public unsafe QueryType QuerySetGetType(QuerySet* querySet)
    {
        return WGPUBrowserNative.QuerySetGetType(WgpuCast.Cast(querySet)).Cast();
    }

    public unsafe void QuerySetSetLabel(QuerySet* querySet, byte* label)
    {
        WGPUBrowserNative.QuerySetSetLabel(WgpuCast.Cast(querySet), label);
    }

    public unsafe void QuerySetSetLabel(QuerySet* querySet, in byte label)
    {
        WGPUBrowserNative.QuerySetSetLabel(WgpuCast.Cast(querySet), in label);
    }

    public unsafe void QuerySetSetLabel(QuerySet* querySet, string label)
    {
        WGPUBrowserNative.QuerySetSetLabel(WgpuCast.Cast(querySet), label);
    }

    public unsafe void QuerySetReference(QuerySet* querySet)
    {
        WGPUBrowserNative.QuerySetReference(WgpuCast.Cast(querySet));
    }

    public unsafe void QuerySetRelease(QuerySet* querySet)
    {
        WGPUBrowserNative.QuerySetRelease(WgpuCast.Cast(querySet));
    }

    public unsafe void QueueOnSubmittedWorkDone(Queue* queue, PfnQueueWorkDoneCallback callback, void* userdata)
    {
        WGPUBrowserNative.QueueOnSubmittedWorkDone(WgpuCast.Cast(queue), callback, userdata);
    }

    public unsafe void QueueSetLabel(Queue* queue, byte* label)
    {
        WGPUBrowserNative.QueueSetLabel(WgpuCast.Cast(queue), label);
    }

    public unsafe void QueueSetLabel(Queue* queue, in byte label)
    {
        WGPUBrowserNative.QueueSetLabel(WgpuCast.Cast(queue), in label);
    }

    public unsafe void QueueSetLabel(Queue* queue, string label)
    {
        WGPUBrowserNative.QueueSetLabel(WgpuCast.Cast(queue), label);
    }

    public unsafe void QueueSubmit(Queue* queue, nuint commandCount, CommandBuffer** commands)
    {
        WGPUBrowserNative.QueueSubmit(WgpuCast.Cast(queue), commandCount, (WGPUCommandBuffer**)commands);
    }

    public unsafe void QueueSubmit(Queue* queue, nuint commandCount, ref CommandBuffer* commands)
    {
        fixed (CommandBuffer** commandsPtr = &commands)
        {
            WGPUBrowserNative.QueueSubmit(WgpuCast.Cast(queue), commandCount, (WGPUCommandBuffer**)commandsPtr);
        }
    }

    public unsafe void QueueWriteBuffer(Queue* queue, Buffer* buffer, ulong bufferOffset, void* data, nuint size)
    {
        WGPUBrowserNative.QueueWriteBuffer(WgpuCast.Cast(queue), WgpuCast.Cast(buffer), bufferOffset, data, size);
    }

    public unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize,
        TextureDataLayout* dataLayout, Extent3D* writeSize)
    {
        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUTextureDataLayout* nativeLayout = null;
        if (dataLayout != null)
        {
            var nativeLayoutValue = (*dataLayout).Cast();
            nativeLayout = &nativeLayoutValue;
        }

        WGPUBrowserNative.QueueWriteTexture(WgpuCast.Cast(queue), nativeDest, data, dataSize, nativeLayout,
            (WGPUExtent3D*)writeSize);
    }

    public unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize,
        TextureDataLayout* dataLayout, in Extent3D writeSize)
    {
        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        WGPUTextureDataLayout* nativeLayout = null;
        if (dataLayout != null)
        {
            var nativeLayoutValue = (*dataLayout).Cast();
            nativeLayout = &nativeLayoutValue;
        }

        WGPUBrowserNative.QueueWriteTexture(WgpuCast.Cast(queue), nativeDest, data, dataSize, nativeLayout,
            in Unsafe.AsRef(in writeSize).Cast());
    }

    public unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize,
        in TextureDataLayout dataLayout, Extent3D* writeSize)
    {
        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        var dl = dataLayout;
        var nativeLayout = dl.Cast();
        WGPUBrowserNative.QueueWriteTexture(WgpuCast.Cast(queue), nativeDest, data, dataSize, in nativeLayout,
            (WGPUExtent3D*)writeSize);
    }

    public unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize,
        in TextureDataLayout dataLayout, in Extent3D writeSize)
    {
        WGPUImageCopyTexture* nativeDest = null;
        if (destination != null)
        {
            var nativeDestValue = (*destination).Cast();
            nativeDest = &nativeDestValue;
        }

        var dl = dataLayout;
        var nativeLayout = dl.Cast();
        WGPUBrowserNative.QueueWriteTexture(WgpuCast.Cast(queue), nativeDest, data, dataSize, in nativeLayout,
            in Unsafe.AsRef(in writeSize).Cast());
    }

    public unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize,
        TextureDataLayout* dataLayout, Extent3D* writeSize)
    {
        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUTextureDataLayout* nativeLayout = null;
        if (dataLayout != null)
        {
            var nativeLayoutValue = (*dataLayout).Cast();
            nativeLayout = &nativeLayoutValue;
        }

        WGPUBrowserNative.QueueWriteTexture(WgpuCast.Cast(queue), in nativeDest, data, dataSize, nativeLayout,
            (WGPUExtent3D*)writeSize);
    }

    public unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize,
        TextureDataLayout* dataLayout, in Extent3D writeSize)
    {
        var dst = destination;
        var nativeDest = dst.Cast();
        WGPUTextureDataLayout* nativeLayout = null;
        if (dataLayout != null)
        {
            var nativeLayoutValue = (*dataLayout).Cast();
            nativeLayout = &nativeLayoutValue;
        }

        WGPUBrowserNative.QueueWriteTexture(WgpuCast.Cast(queue), in nativeDest, data, dataSize, nativeLayout,
            in Unsafe.AsRef(in writeSize).Cast());
    }

    public unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize,
        in TextureDataLayout dataLayout, Extent3D* writeSize)
    {
        var dst = destination;
        var nativeDest = dst.Cast();
        var dl = dataLayout;
        var nativeLayout = dl.Cast();
        WGPUBrowserNative.QueueWriteTexture(WgpuCast.Cast(queue), in nativeDest, data, dataSize, in nativeLayout,
            (WGPUExtent3D*)writeSize);
    }

    public unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize,
        in TextureDataLayout dataLayout, in Extent3D writeSize)
    {
        var dst = destination;
        var nativeDest = dst.Cast();
        var dl = dataLayout;
        var nativeLayout = dl.Cast();
        WGPUBrowserNative.QueueWriteTexture(WgpuCast.Cast(queue), in nativeDest, data, dataSize, in nativeLayout,
            in Unsafe.AsRef(in writeSize).Cast());
    }

    public unsafe void QueueReference(Queue* queue)
    {
        WGPUBrowserNative.QueueReference(WgpuCast.Cast(queue));
    }

    public unsafe void QueueRelease(Queue* queue)
    {
        WGPUBrowserNative.QueueRelease(WgpuCast.Cast(queue));
    }

    public unsafe void RenderBundleSetLabel(RenderBundle* renderBundle, byte* label)
    {
        WGPUBrowserNative.RenderBundleSetLabel(WgpuCast.Cast(renderBundle), label);
    }

    public unsafe void RenderBundleSetLabel(RenderBundle* renderBundle, in byte label)
    {
        WGPUBrowserNative.RenderBundleSetLabel(WgpuCast.Cast(renderBundle), in label);
    }

    public unsafe void RenderBundleSetLabel(RenderBundle* renderBundle, string label)
    {
        WGPUBrowserNative.RenderBundleSetLabel(WgpuCast.Cast(renderBundle), label);
    }

    public unsafe void RenderBundleReference(RenderBundle* renderBundle)
    {
        WGPUBrowserNative.RenderBundleReference(WgpuCast.Cast(renderBundle));
    }

    public unsafe void RenderBundleRelease(RenderBundle* renderBundle)
    {
        WGPUBrowserNative.RenderBundleRelease(WgpuCast.Cast(renderBundle));
    }

    public unsafe void RenderBundleEncoderDraw(RenderBundleEncoder* renderBundleEncoder, uint vertexCount,
        uint instanceCount, uint firstVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderBundleEncoderDraw(WgpuCast.Cast(renderBundleEncoder), vertexCount, instanceCount,
            firstVertex, firstInstance);
    }

    public unsafe void RenderBundleEncoderDrawIndexed(RenderBundleEncoder* renderBundleEncoder, uint indexCount,
        uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderBundleEncoderDrawIndexed(WgpuCast.Cast(renderBundleEncoder), indexCount, instanceCount,
            firstIndex, baseVertex, firstInstance);
    }

    public unsafe void RenderBundleEncoderDrawIndexedIndirect(RenderBundleEncoder* renderBundleEncoder,
        Buffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderBundleEncoderDrawIndexedIndirect(WgpuCast.Cast(renderBundleEncoder),
            WgpuCast.Cast(indirectBuffer), indirectOffset);
    }

    public unsafe void RenderBundleEncoderDrawIndirect(RenderBundleEncoder* renderBundleEncoder, Buffer* indirectBuffer,
        ulong indirectOffset)
    {
        WGPUBrowserNative.RenderBundleEncoderDrawIndirect(WgpuCast.Cast(renderBundleEncoder),
            WgpuCast.Cast(indirectBuffer), indirectOffset);
    }

    public unsafe RenderBundle* RenderBundleEncoderFinish(RenderBundleEncoder* renderBundleEncoder,
        RenderBundleDescriptor* descriptor)
    {
        WGPURenderBundleDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(
            WGPUBrowserNative.RenderBundleEncoderFinish(WgpuCast.Cast(renderBundleEncoder), nativeDescriptor));
    }

    public unsafe RenderBundle* RenderBundleEncoderFinish(RenderBundleEncoder* renderBundleEncoder,
        in RenderBundleDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(
            WGPUBrowserNative.RenderBundleEncoderFinish(WgpuCast.Cast(renderBundleEncoder), in nativeDescriptor));
    }

    public unsafe void RenderBundleEncoderInsertDebugMarker(RenderBundleEncoder* renderBundleEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderInsertDebugMarker(WgpuCast.Cast(renderBundleEncoder), markerLabel);
    }

    public unsafe void RenderBundleEncoderInsertDebugMarker(RenderBundleEncoder* renderBundleEncoder,
        in byte markerLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderInsertDebugMarker(WgpuCast.Cast(renderBundleEncoder), in markerLabel);
    }

    public unsafe void RenderBundleEncoderInsertDebugMarker(RenderBundleEncoder* renderBundleEncoder,
        string markerLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderInsertDebugMarker(WgpuCast.Cast(renderBundleEncoder), markerLabel);
    }

    public unsafe void RenderBundleEncoderPopDebugGroup(RenderBundleEncoder* renderBundleEncoder)
    {
        WGPUBrowserNative.RenderBundleEncoderPopDebugGroup(WgpuCast.Cast(renderBundleEncoder));
    }

    public unsafe void RenderBundleEncoderPushDebugGroup(RenderBundleEncoder* renderBundleEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderPushDebugGroup(WgpuCast.Cast(renderBundleEncoder), groupLabel);
    }

    public unsafe void RenderBundleEncoderPushDebugGroup(RenderBundleEncoder* renderBundleEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderPushDebugGroup(WgpuCast.Cast(renderBundleEncoder), in groupLabel);
    }

    public unsafe void RenderBundleEncoderPushDebugGroup(RenderBundleEncoder* renderBundleEncoder, string groupLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderPushDebugGroup(WgpuCast.Cast(renderBundleEncoder), groupLabel);
    }

    public unsafe void RenderBundleEncoderSetBindGroup(RenderBundleEncoder* renderBundleEncoder, uint groupIndex,
        BindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets)
    {
        WGPUBrowserNative.RenderBundleEncoderSetBindGroup(WgpuCast.Cast(renderBundleEncoder), groupIndex,
            WgpuCast.Cast(group), dynamicOffsetCount, dynamicOffsets);
    }

    public unsafe void RenderBundleEncoderSetBindGroup(RenderBundleEncoder* renderBundleEncoder, uint groupIndex,
        BindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets)
    {
        WGPUBrowserNative.RenderBundleEncoderSetBindGroup(WgpuCast.Cast(renderBundleEncoder), groupIndex,
            WgpuCast.Cast(group), dynamicOffsetCount, in dynamicOffsets);
    }

    public unsafe void RenderBundleEncoderSetIndexBuffer(RenderBundleEncoder* renderBundleEncoder, Buffer* buffer,
        IndexFormat format, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderBundleEncoderSetIndexBuffer(WgpuCast.Cast(renderBundleEncoder), WgpuCast.Cast(buffer),
            format.Cast(), offset, size);
    }

    public unsafe void RenderBundleEncoderSetLabel(RenderBundleEncoder* renderBundleEncoder, byte* label)
    {
        WGPUBrowserNative.RenderBundleEncoderSetLabel(WgpuCast.Cast(renderBundleEncoder), label);
    }

    public unsafe void RenderBundleEncoderSetLabel(RenderBundleEncoder* renderBundleEncoder, in byte label)
    {
        WGPUBrowserNative.RenderBundleEncoderSetLabel(WgpuCast.Cast(renderBundleEncoder), in label);
    }

    public unsafe void RenderBundleEncoderSetLabel(RenderBundleEncoder* renderBundleEncoder, string label)
    {
        WGPUBrowserNative.RenderBundleEncoderSetLabel(WgpuCast.Cast(renderBundleEncoder), label);
    }

    public unsafe void RenderBundleEncoderSetPipeline(RenderBundleEncoder* renderBundleEncoder,
        RenderPipeline* pipeline)
    {
        WGPUBrowserNative.RenderBundleEncoderSetPipeline(WgpuCast.Cast(renderBundleEncoder), WgpuCast.Cast(pipeline));
    }

    public unsafe void RenderBundleEncoderSetVertexBuffer(RenderBundleEncoder* renderBundleEncoder, uint slot,
        Buffer* buffer, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderBundleEncoderSetVertexBuffer(WgpuCast.Cast(renderBundleEncoder), slot,
            WgpuCast.Cast(buffer), offset, size);
    }

    public unsafe void RenderBundleEncoderReference(RenderBundleEncoder* renderBundleEncoder)
    {
        WGPUBrowserNative.RenderBundleEncoderReference(WgpuCast.Cast(renderBundleEncoder));
    }

    public unsafe void RenderBundleEncoderRelease(RenderBundleEncoder* renderBundleEncoder)
    {
        WGPUBrowserNative.RenderBundleEncoderRelease(WgpuCast.Cast(renderBundleEncoder));
    }

    public unsafe void RenderPassEncoderBeginOcclusionQuery(RenderPassEncoder* renderPassEncoder, uint queryIndex)
    {
        WGPUBrowserNative.RenderPassEncoderBeginOcclusionQuery(WgpuCast.Cast(renderPassEncoder), queryIndex);
    }

    public unsafe void RenderPassEncoderBeginPipelineStatisticsQuery(RenderPassEncoder* renderPassEncoder,
        QuerySet* querySet, uint queryIndex)
    {
        WGPUBrowserNative.RenderPassEncoderBeginPipelineStatisticsQuery(WgpuCast.Cast(renderPassEncoder),
            WgpuCast.Cast(querySet), queryIndex);
    }

    public unsafe void RenderPassEncoderDraw(RenderPassEncoder* renderPassEncoder, uint vertexCount, uint instanceCount,
        uint firstVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderPassEncoderDraw(WgpuCast.Cast(renderPassEncoder), vertexCount, instanceCount,
            firstVertex, firstInstance);
    }

    public unsafe void RenderPassEncoderDrawIndexed(RenderPassEncoder* renderPassEncoder, uint indexCount,
        uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderPassEncoderDrawIndexed(WgpuCast.Cast(renderPassEncoder), indexCount, instanceCount,
            firstIndex, baseVertex, firstInstance);
    }

    public unsafe void RenderPassEncoderDrawIndexedIndirect(RenderPassEncoder* renderPassEncoder,
        Buffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderPassEncoderDrawIndexedIndirect(WgpuCast.Cast(renderPassEncoder),
            WgpuCast.Cast(indirectBuffer), indirectOffset);
    }

    public unsafe void RenderPassEncoderDrawIndirect(RenderPassEncoder* renderPassEncoder, Buffer* indirectBuffer,
        ulong indirectOffset)
    {
        WGPUBrowserNative.RenderPassEncoderDrawIndirect(WgpuCast.Cast(renderPassEncoder), WgpuCast.Cast(indirectBuffer),
            indirectOffset);
    }

    public unsafe void RenderPassEncoderEnd(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderEnd(WgpuCast.Cast(renderPassEncoder));
    }

    public unsafe void RenderPassEncoderEndOcclusionQuery(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderEndOcclusionQuery(WgpuCast.Cast(renderPassEncoder));
    }

    public unsafe void RenderPassEncoderEndPipelineStatisticsQuery(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderEndPipelineStatisticsQuery(WgpuCast.Cast(renderPassEncoder));
    }

    public unsafe void RenderPassEncoderExecuteBundles(RenderPassEncoder* renderPassEncoder, nuint bundleCount,
        RenderBundle** bundles)
    {
        WGPUBrowserNative.RenderPassEncoderExecuteBundles(WgpuCast.Cast(renderPassEncoder), bundleCount,
            (WGPURenderBundle**)bundles);
    }

    public unsafe void RenderPassEncoderExecuteBundles(RenderPassEncoder* renderPassEncoder, nuint bundleCount,
        ref RenderBundle* bundles)
    {
        fixed (RenderBundle** bundlesPtr = &bundles)
        {
            WGPUBrowserNative.RenderPassEncoderExecuteBundles(WgpuCast.Cast(renderPassEncoder), bundleCount,
                (WGPURenderBundle**)bundlesPtr);
        }
    }

    public unsafe void RenderPassEncoderInsertDebugMarker(RenderPassEncoder* renderPassEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.RenderPassEncoderInsertDebugMarker(WgpuCast.Cast(renderPassEncoder), markerLabel);
    }

    public unsafe void RenderPassEncoderInsertDebugMarker(RenderPassEncoder* renderPassEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.RenderPassEncoderInsertDebugMarker(WgpuCast.Cast(renderPassEncoder), in markerLabel);
    }

    public unsafe void RenderPassEncoderInsertDebugMarker(RenderPassEncoder* renderPassEncoder, string markerLabel)
    {
        WGPUBrowserNative.RenderPassEncoderInsertDebugMarker(WgpuCast.Cast(renderPassEncoder), markerLabel);
    }

    public unsafe void RenderPassEncoderPopDebugGroup(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderPopDebugGroup(WgpuCast.Cast(renderPassEncoder));
    }

    public unsafe void RenderPassEncoderPushDebugGroup(RenderPassEncoder* renderPassEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.RenderPassEncoderPushDebugGroup(WgpuCast.Cast(renderPassEncoder), groupLabel);
    }

    public unsafe void RenderPassEncoderPushDebugGroup(RenderPassEncoder* renderPassEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.RenderPassEncoderPushDebugGroup(WgpuCast.Cast(renderPassEncoder), in groupLabel);
    }

    public unsafe void RenderPassEncoderPushDebugGroup(RenderPassEncoder* renderPassEncoder, string groupLabel)
    {
        WGPUBrowserNative.RenderPassEncoderPushDebugGroup(WgpuCast.Cast(renderPassEncoder), groupLabel);
    }

    public unsafe void RenderPassEncoderSetBindGroup(RenderPassEncoder* renderPassEncoder, uint groupIndex,
        BindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets)
    {
        WGPUBrowserNative.RenderPassEncoderSetBindGroup(WgpuCast.Cast(renderPassEncoder), groupIndex,
            WgpuCast.Cast(group), dynamicOffsetCount, dynamicOffsets);
    }

    public unsafe void RenderPassEncoderSetBindGroup(RenderPassEncoder* renderPassEncoder, uint groupIndex,
        BindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets)
    {
        WGPUBrowserNative.RenderPassEncoderSetBindGroup(WgpuCast.Cast(renderPassEncoder), groupIndex,
            WgpuCast.Cast(group), dynamicOffsetCount, in dynamicOffsets);
    }

    public unsafe void RenderPassEncoderSetBlendConstant(RenderPassEncoder* renderPassEncoder, Color* color)
    {
        WGPUBrowserNative.RenderPassEncoderSetBlendConstant(WgpuCast.Cast(renderPassEncoder), (WGPUColor*)color);
    }

    public unsafe void RenderPassEncoderSetBlendConstant(RenderPassEncoder* renderPassEncoder, in Color color)
    {
        WGPUBrowserNative.RenderPassEncoderSetBlendConstant(WgpuCast.Cast(renderPassEncoder),
            in Unsafe.AsRef(in color).Cast());
    }

    public unsafe void RenderPassEncoderSetIndexBuffer(RenderPassEncoder* renderPassEncoder, Buffer* buffer,
        IndexFormat format, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderPassEncoderSetIndexBuffer(WgpuCast.Cast(renderPassEncoder), WgpuCast.Cast(buffer),
            format.Cast(), offset, size);
    }

    public unsafe void RenderPassEncoderSetLabel(RenderPassEncoder* renderPassEncoder, byte* label)
    {
        WGPUBrowserNative.RenderPassEncoderSetLabel(WgpuCast.Cast(renderPassEncoder), label);
    }

    public unsafe void RenderPassEncoderSetLabel(RenderPassEncoder* renderPassEncoder, in byte label)
    {
        WGPUBrowserNative.RenderPassEncoderSetLabel(WgpuCast.Cast(renderPassEncoder), in label);
    }

    public unsafe void RenderPassEncoderSetLabel(RenderPassEncoder* renderPassEncoder, string label)
    {
        WGPUBrowserNative.RenderPassEncoderSetLabel(WgpuCast.Cast(renderPassEncoder), label);
    }

    public unsafe void RenderPassEncoderSetPipeline(RenderPassEncoder* renderPassEncoder, RenderPipeline* pipeline)
    {
        WGPUBrowserNative.RenderPassEncoderSetPipeline(WgpuCast.Cast(renderPassEncoder), WgpuCast.Cast(pipeline));
    }

    public unsafe void RenderPassEncoderSetScissorRect(RenderPassEncoder* renderPassEncoder, uint x, uint y, uint width,
        uint height)
    {
        WGPUBrowserNative.RenderPassEncoderSetScissorRect(WgpuCast.Cast(renderPassEncoder), x, y, width, height);
    }

    public unsafe void RenderPassEncoderSetStencilReference(RenderPassEncoder* renderPassEncoder, uint reference)
    {
        WGPUBrowserNative.RenderPassEncoderSetStencilReference(WgpuCast.Cast(renderPassEncoder), reference);
    }

    public unsafe void RenderPassEncoderSetVertexBuffer(RenderPassEncoder* renderPassEncoder, uint slot, Buffer* buffer,
        ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderPassEncoderSetVertexBuffer(WgpuCast.Cast(renderPassEncoder), slot,
            WgpuCast.Cast(buffer), offset, size);
    }

    public unsafe void RenderPassEncoderSetViewport(RenderPassEncoder* renderPassEncoder, float x, float y, float width,
        float height, float minDepth, float maxDepth)
    {
        WGPUBrowserNative.RenderPassEncoderSetViewport(WgpuCast.Cast(renderPassEncoder), x, y, width, height, minDepth,
            maxDepth);
    }

    public unsafe void RenderPassEncoderReference(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderReference(WgpuCast.Cast(renderPassEncoder));
    }

    public unsafe void RenderPassEncoderRelease(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderRelease(WgpuCast.Cast(renderPassEncoder));
    }

    public unsafe BindGroupLayout* RenderPipelineGetBindGroupLayout(RenderPipeline* renderPipeline, uint groupIndex)
    {
        return WgpuCast.Cast(
            WGPUBrowserNative.RenderPipelineGetBindGroupLayout(WgpuCast.Cast(renderPipeline), groupIndex));
    }

    public unsafe void RenderPipelineSetLabel(RenderPipeline* renderPipeline, byte* label)
    {
        WGPUBrowserNative.RenderPipelineSetLabel(WgpuCast.Cast(renderPipeline), label);
    }

    public unsafe void RenderPipelineSetLabel(RenderPipeline* renderPipeline, in byte label)
    {
        WGPUBrowserNative.RenderPipelineSetLabel(WgpuCast.Cast(renderPipeline), in label);
    }

    public unsafe void RenderPipelineSetLabel(RenderPipeline* renderPipeline, string label)
    {
        WGPUBrowserNative.RenderPipelineSetLabel(WgpuCast.Cast(renderPipeline), label);
    }

    public unsafe void RenderPipelineReference(RenderPipeline* renderPipeline)
    {
        WGPUBrowserNative.RenderPipelineReference(WgpuCast.Cast(renderPipeline));
    }

    public unsafe void RenderPipelineRelease(RenderPipeline* renderPipeline)
    {
        WGPUBrowserNative.RenderPipelineRelease(WgpuCast.Cast(renderPipeline));
    }

    public unsafe void SamplerSetLabel(Sampler* sampler, byte* label)
    {
        WGPUBrowserNative.SamplerSetLabel(WgpuCast.Cast(sampler), label);
    }

    public unsafe void SamplerSetLabel(Sampler* sampler, in byte label)
    {
        WGPUBrowserNative.SamplerSetLabel(WgpuCast.Cast(sampler), in label);
    }

    public unsafe void SamplerSetLabel(Sampler* sampler, string label)
    {
        WGPUBrowserNative.SamplerSetLabel(WgpuCast.Cast(sampler), label);
    }

    public unsafe void SamplerReference(Sampler* sampler)
    {
        WGPUBrowserNative.SamplerReference(WgpuCast.Cast(sampler));
    }

    public unsafe void SamplerRelease(Sampler* sampler)
    {
        WGPUBrowserNative.SamplerRelease(WgpuCast.Cast(sampler));
    }

    public unsafe void ShaderModuleGetCompilationInfo(ShaderModule* shaderModule, PfnCompilationInfoCallback callback,
        void* userdata)
    {
        WGPUBrowserNative.ShaderModuleGetCompilationInfo(WgpuCast.Cast(shaderModule), callback, userdata);
    }

    public unsafe void ShaderModuleSetLabel(ShaderModule* shaderModule, byte* label)
    {
        WGPUBrowserNative.ShaderModuleSetLabel(WgpuCast.Cast(shaderModule), label);
    }

    public unsafe void ShaderModuleSetLabel(ShaderModule* shaderModule, in byte label)
    {
        WGPUBrowserNative.ShaderModuleSetLabel(WgpuCast.Cast(shaderModule), in label);
    }

    public unsafe void ShaderModuleSetLabel(ShaderModule* shaderModule, string label)
    {
        WGPUBrowserNative.ShaderModuleSetLabel(WgpuCast.Cast(shaderModule), label);
    }

    public unsafe void ShaderModuleReference(ShaderModule* shaderModule)
    {
        WGPUBrowserNative.ShaderModuleReference(WgpuCast.Cast(shaderModule));
    }

    public unsafe void ShaderModuleRelease(ShaderModule* shaderModule)
    {
        WGPUBrowserNative.ShaderModuleRelease(WgpuCast.Cast(shaderModule));
    }

    public unsafe TextureFormat SurfaceGetPreferredFormat(Surface* surface, Adapter* adapter)
    {
        return WGPUBrowserNative.SurfaceGetPreferredFormat(WgpuCast.Cast(surface), WgpuCast.Cast(adapter)).Cast();
    }

    public unsafe void SurfacePresent(Surface* surface)
    {
        WGPUBrowserNative.SurfacePresent(WgpuCast.Cast(surface));
    }

    public unsafe void SurfaceUnconfigure(Surface* surface)
    {
        WGPUBrowserNative.SurfaceUnconfigure(WgpuCast.Cast(surface));
    }

    public unsafe void SurfaceReference(Surface* surface)
    {
        WGPUBrowserNative.SurfaceReference(WgpuCast.Cast(surface));
    }

    public unsafe void SurfaceRelease(Surface* surface)
    {
        WGPUBrowserNative.SurfaceRelease(WgpuCast.Cast(surface));
    }

    public unsafe TextureView* TextureCreateView(Texture* texture, TextureViewDescriptor* descriptor)
    {
        WGPUTextureViewDescriptor* nativeDescriptor = null;
        if (descriptor != null)
        {
            var nativeDescriptorValue = (*descriptor).Cast();
            nativeDescriptor = &nativeDescriptorValue;
        }

        return WgpuCast.Cast(WGPUBrowserNative.TextureCreateView(WgpuCast.Cast(texture), nativeDescriptor));
    }

    public unsafe TextureView* TextureCreateView(Texture* texture, in TextureViewDescriptor descriptor)
    {
        var d = descriptor;
        var nativeDescriptor = d.Cast();
        return WgpuCast.Cast(WGPUBrowserNative.TextureCreateView(WgpuCast.Cast(texture), in nativeDescriptor));
    }

    public unsafe void TextureDestroy(Texture* texture)
    {
        WGPUBrowserNative.TextureDestroy(WgpuCast.Cast(texture));
    }

    public unsafe uint TextureGetDepthOrArrayLayers(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetDepthOrArrayLayers(WgpuCast.Cast(texture));
    }

    public unsafe TextureDimension TextureGetDimension(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetDimension(WgpuCast.Cast(texture)).Cast();
    }

    public unsafe TextureFormat TextureGetFormat(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetFormat(WgpuCast.Cast(texture)).Cast();
    }

    public unsafe uint TextureGetHeight(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetHeight(WgpuCast.Cast(texture));
    }

    public unsafe uint TextureGetMipLevelCount(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetMipLevelCount(WgpuCast.Cast(texture));
    }

    public unsafe uint TextureGetSampleCount(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetSampleCount(WgpuCast.Cast(texture));
    }

    public unsafe TextureUsage TextureGetUsage(Texture* texture)
    {
        return (TextureUsage)WGPUBrowserNative.TextureGetUsage(WgpuCast.Cast(texture));
    }

    public unsafe uint TextureGetWidth(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetWidth(WgpuCast.Cast(texture));
    }

    public unsafe void TextureSetLabel(Texture* texture, byte* label)
    {
        WGPUBrowserNative.TextureSetLabel(WgpuCast.Cast(texture), label);
    }

    public unsafe void TextureSetLabel(Texture* texture, in byte label)
    {
        WGPUBrowserNative.TextureSetLabel(WgpuCast.Cast(texture), in label);
    }

    public unsafe void TextureSetLabel(Texture* texture, string label)
    {
        WGPUBrowserNative.TextureSetLabel(WgpuCast.Cast(texture), label);
    }

    public unsafe void TextureReference(Texture* texture)
    {
        WGPUBrowserNative.TextureReference(WgpuCast.Cast(texture));
    }

    public unsafe void TextureRelease(Texture* texture)
    {
        WGPUBrowserNative.TextureRelease(WgpuCast.Cast(texture));
    }

    public unsafe void TextureViewSetLabel(TextureView* textureView, byte* label)
    {
        WGPUBrowserNative.TextureViewSetLabel(WgpuCast.Cast(textureView), label);
    }

    public unsafe void TextureViewSetLabel(TextureView* textureView, in byte label)
    {
        WGPUBrowserNative.TextureViewSetLabel(WgpuCast.Cast(textureView), in label);
    }

    public unsafe void TextureViewSetLabel(TextureView* textureView, string label)
    {
        WGPUBrowserNative.TextureViewSetLabel(WgpuCast.Cast(textureView), label);
    }

    public unsafe void TextureViewReference(TextureView* textureView)
    {
        WGPUBrowserNative.TextureViewReference(WgpuCast.Cast(textureView));
    }

    public unsafe void TextureViewRelease(TextureView* textureView)
    {
        WGPUBrowserNative.TextureViewRelease(WgpuCast.Cast(textureView));
    }

    public unsafe TextureView* SwapChainGetCurrentTextureView(Dawn.SwapChain* swapChain)
    {
        return WgpuCast.Cast(WGPUBrowserNative.SwapChainGetCurrentTextureView((WGPUSwapChain*)swapChain));
    }

    public unsafe void SwapChainRelease(Dawn.SwapChain* swapChain)
    {
        WGPUBrowserNative.SwapChainRelease((WGPUSwapChain*)swapChain);
    }

    public unsafe Dawn.SwapChain* DeviceCreateSwapChain(Device* device, Surface* surface,
        Dawn.SwapChainDescriptor descriptor)
    {
        var nativeDescriptor = new WGPUSwapChainDescriptor
        {
            NextInChain = null,
            Label = null,
            Usage = (WGPUTextureUsage)descriptor.Usage,
            Format = descriptor.Format.Cast(),
            Width = descriptor.Width,
            Height = descriptor.Height,
            PresentMode = descriptor.PresentMode.Cast(),
        };
        return (Dawn.SwapChain*)WGPUBrowserNative.DeviceCreateSwapChain(WgpuCast.Cast(device), WgpuCast.Cast(surface),
            in nativeDescriptor);
    }

    public void Dispose()
    {
    }
}