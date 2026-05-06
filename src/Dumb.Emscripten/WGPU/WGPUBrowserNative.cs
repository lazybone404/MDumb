namespace Dumb.Emscripten;

using System.Runtime.InteropServices;
using Dumb.Emscripten.WGPU;

public static partial class WGPUBrowserNative
{
    private const string LibraryName = "__Internal_emscripten";

    [DllImport(LibraryName, EntryPoint = "wgpuCreateInstance")]
    extern public static unsafe WGPUInstance* CreateInstance(WGPUInstanceDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuCreateInstance")]
    extern public static unsafe WGPUInstance* CreateInstance(in WGPUInstanceDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuGetProcAddress")]
    extern public static unsafe nint GetProcAddress(WGPUDevice* device, byte* procName);
    [DllImport(LibraryName, EntryPoint = "wgpuGetProcAddress")]
    extern public static unsafe nint GetProcAddress(WGPUDevice* device, in byte procName);
    [DllImport(LibraryName, EntryPoint = "wgpuGetProcAddress")]
    extern public static unsafe nint GetProcAddress(WGPUDevice* device, [MarshalAs(UnmanagedType.LPUTF8Str)] string procName);
    [DllImport(LibraryName, EntryPoint = "wgpuAdapterEnumerateFeatures")]
    extern public static unsafe nuint AdapterEnumerateFeatures(WGPUAdapter* adapter, WGPUFeatureName* features);
    [DllImport(LibraryName, EntryPoint = "wgpuAdapterEnumerateFeatures")]
    extern public static unsafe nuint AdapterEnumerateFeatures(WGPUAdapter* adapter, ref WGPUFeatureName features);
    [DllImport(LibraryName, EntryPoint = "wgpuAdapterGetLimits")]
    extern public static unsafe bool AdapterGetLimits(WGPUAdapter* adapter, WGPUSupportedLimits* limits);
    [DllImport(LibraryName, EntryPoint = "wgpuAdapterGetLimits")]
    extern public static unsafe bool AdapterGetLimits(WGPUAdapter* adapter, ref WGPUSupportedLimits limits);
    [DllImport(LibraryName, EntryPoint = "wgpuAdapterGetProperties")]
    extern public static unsafe void AdapterGetProperties(WGPUAdapter* adapter, WGPUAdapterProperties* properties);
    [DllImport(LibraryName, EntryPoint = "wgpuAdapterGetProperties")]
    extern public static unsafe void AdapterGetProperties(WGPUAdapter* adapter, ref WGPUAdapterProperties properties);
    [DllImport(LibraryName, EntryPoint = "wgpuAdapterHasFeature")]
    extern public static unsafe bool AdapterHasFeature(WGPUAdapter* adapter, WGPUFeatureName feature);

    [DllImport(LibraryName, EntryPoint = "wgpuAdapterRequestDevice")]
    extern public static unsafe void AdapterRequestDevice(WGPUAdapter* adapter, WGPUDeviceDescriptor* descriptor, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuAdapterRequestDevice")]
    extern public static unsafe void AdapterRequestDevice(WGPUAdapter* adapter, in WGPUDeviceDescriptor descriptor, nint callback, void* userdata);

    [DllImport(LibraryName, EntryPoint = "wgpuAdapterReference")]
    extern public static unsafe void AdapterReference(WGPUAdapter* adapter);
    [DllImport(LibraryName, EntryPoint = "wgpuAdapterRelease")]
    extern public static unsafe void AdapterRelease(WGPUAdapter* adapter);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupSetLabel")]
    extern public static unsafe void BindGroupSetLabel(WGPUBindGroup* bindGroup, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupSetLabel")]
    extern public static unsafe void BindGroupSetLabel(WGPUBindGroup* bindGroup, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupSetLabel")]
    extern public static unsafe void BindGroupSetLabel(WGPUBindGroup* bindGroup, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupReference")]
    extern public static unsafe void BindGroupReference(WGPUBindGroup* bindGroup);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupRelease")]
    extern public static unsafe void BindGroupRelease(WGPUBindGroup* bindGroup);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupLayoutSetLabel")]
    extern public static unsafe void BindGroupLayoutSetLabel(WGPUBindGroupLayout* bindGroupLayout, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupLayoutSetLabel")]
    extern public static unsafe void BindGroupLayoutSetLabel(WGPUBindGroupLayout* bindGroupLayout, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupLayoutSetLabel")]
    extern public static unsafe void BindGroupLayoutSetLabel(WGPUBindGroupLayout* bindGroupLayout, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupLayoutReference")]
    extern public static unsafe void BindGroupLayoutReference(WGPUBindGroupLayout* bindGroupLayout);
    [DllImport(LibraryName, EntryPoint = "wgpuBindGroupLayoutRelease")]
    extern public static unsafe void BindGroupLayoutRelease(WGPUBindGroupLayout* bindGroupLayout);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferDestroy")]
    extern public static unsafe void BufferDestroy(WGPUBuffer* buffer);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferGetConstMappedRange")]
    extern public static unsafe void* BufferGetConstMappedRange(WGPUBuffer* buffer, nuint offset, nuint size);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferGetMapState")]
    extern public static unsafe WGPUBufferMapState BufferGetMapState(WGPUBuffer* buffer);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferGetMappedRange")]
    extern public static unsafe void* BufferGetMappedRange(WGPUBuffer* buffer, nuint offset, nuint size);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferGetSize")]
    extern public static unsafe ulong BufferGetSize(WGPUBuffer* buffer);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferGetUsage")]
    extern public static unsafe WGPUBufferUsage BufferGetUsage(WGPUBuffer* buffer);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferMapAsync")]
    extern public static unsafe void BufferMapAsync(WGPUBuffer* buffer, WGPUMapMode mode, nuint offset, nuint size, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferSetLabel")]
    extern public static unsafe void BufferSetLabel(WGPUBuffer* buffer, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferSetLabel")]
    extern public static unsafe void BufferSetLabel(WGPUBuffer* buffer, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferSetLabel")]
    extern public static unsafe void BufferSetLabel(WGPUBuffer* buffer, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferUnmap")]
    extern public static unsafe void BufferUnmap(WGPUBuffer* buffer);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferReference")]
    extern public static unsafe void BufferReference(WGPUBuffer* buffer);
    [DllImport(LibraryName, EntryPoint = "wgpuBufferRelease")]
    extern public static unsafe void BufferRelease(WGPUBuffer* buffer);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandBufferSetLabel")]
    extern public static unsafe void CommandBufferSetLabel(WGPUCommandBuffer* commandBuffer, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandBufferSetLabel")]
    extern public static unsafe void CommandBufferSetLabel(WGPUCommandBuffer* commandBuffer, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandBufferSetLabel")]
    extern public static unsafe void CommandBufferSetLabel(WGPUCommandBuffer* commandBuffer, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandBufferReference")]
    extern public static unsafe void CommandBufferReference(WGPUCommandBuffer* commandBuffer);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandBufferRelease")]
    extern public static unsafe void CommandBufferRelease(WGPUCommandBuffer* commandBuffer);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderBeginComputePass")]
    extern public static unsafe WGPUComputePassEncoder* CommandEncoderBeginComputePass(WGPUCommandEncoder* commandEncoder, WGPUComputePassDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderBeginComputePass")]
    extern public static unsafe WGPUComputePassEncoder* CommandEncoderBeginComputePass(WGPUCommandEncoder* commandEncoder, in WGPUComputePassDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderBeginRenderPass")]
    extern public static unsafe WGPURenderPassEncoder* CommandEncoderBeginRenderPass(WGPUCommandEncoder* commandEncoder, WGPURenderPassDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderBeginRenderPass")]
    extern public static unsafe WGPURenderPassEncoder* CommandEncoderBeginRenderPass(WGPUCommandEncoder* commandEncoder, in WGPURenderPassDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderClearBuffer")]
    extern public static unsafe void CommandEncoderClearBuffer(WGPUCommandEncoder* commandEncoder, WGPUBuffer* buffer, ulong offset, ulong size);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyBufferToBuffer")]
    extern public static unsafe void CommandEncoderCopyBufferToBuffer(WGPUCommandEncoder* commandEncoder, WGPUBuffer* source, ulong sourceOffset, WGPUBuffer* destination, ulong destinationOffset, ulong size);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyBuffer* destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyBuffer* destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyBuffer destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyBuffer destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyBuffer* destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyBuffer* destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyBuffer destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyBuffer destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderFinish")]
    extern public static unsafe WGPUCommandBuffer* CommandEncoderFinish(WGPUCommandEncoder* commandEncoder, WGPUCommandBufferDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderFinish")]
    extern public static unsafe WGPUCommandBuffer* CommandEncoderFinish(WGPUCommandEncoder* commandEncoder, in WGPUCommandBufferDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderInsertDebugMarker")]
    extern public static unsafe void CommandEncoderInsertDebugMarker(WGPUCommandEncoder* commandEncoder, byte* markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderInsertDebugMarker")]
    extern public static unsafe void CommandEncoderInsertDebugMarker(WGPUCommandEncoder* commandEncoder, in byte markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderInsertDebugMarker")]
    extern public static unsafe void CommandEncoderInsertDebugMarker(WGPUCommandEncoder* commandEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderPopDebugGroup")]
    extern public static unsafe void CommandEncoderPopDebugGroup(WGPUCommandEncoder* commandEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderPushDebugGroup")]
    extern public static unsafe void CommandEncoderPushDebugGroup(WGPUCommandEncoder* commandEncoder, byte* groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderPushDebugGroup")]
    extern public static unsafe void CommandEncoderPushDebugGroup(WGPUCommandEncoder* commandEncoder, in byte groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderPushDebugGroup")]
    extern public static unsafe void CommandEncoderPushDebugGroup(WGPUCommandEncoder* commandEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderResolveQuerySet")]
    extern public static unsafe void CommandEncoderResolveQuerySet(WGPUCommandEncoder* commandEncoder, WGPUQuerySet* querySet, uint firstQuery, uint queryCount, WGPUBuffer* destination, ulong destinationOffset);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderSetLabel")]
    extern public static unsafe void CommandEncoderSetLabel(WGPUCommandEncoder* commandEncoder, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderSetLabel")]
    extern public static unsafe void CommandEncoderSetLabel(WGPUCommandEncoder* commandEncoder, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderSetLabel")]
    extern public static unsafe void CommandEncoderSetLabel(WGPUCommandEncoder* commandEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderWriteTimestamp")]
    extern public static unsafe void CommandEncoderWriteTimestamp(WGPUCommandEncoder* commandEncoder, WGPUQuerySet* querySet, uint queryIndex);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderReference")]
    extern public static unsafe void CommandEncoderReference(WGPUCommandEncoder* commandEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuCommandEncoderRelease")]
    extern public static unsafe void CommandEncoderRelease(WGPUCommandEncoder* commandEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderBeginPipelineStatisticsQuery")]
    extern public static unsafe void ComputePassEncoderBeginPipelineStatisticsQuery(WGPUComputePassEncoder* computePassEncoder, WGPUQuerySet* querySet, uint queryIndex);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderDispatchWorkgroups")]
    extern public static unsafe void ComputePassEncoderDispatchWorkgroups(WGPUComputePassEncoder* computePassEncoder, uint workgroupCountX, uint workgroupCountY, uint workgroupCountZ);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderDispatchWorkgroupsIndirect")]
    extern public static unsafe void ComputePassEncoderDispatchWorkgroupsIndirect(WGPUComputePassEncoder* computePassEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderEnd")]
    extern public static unsafe void ComputePassEncoderEnd(WGPUComputePassEncoder* computePassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderEndPipelineStatisticsQuery")]
    extern public static unsafe void ComputePassEncoderEndPipelineStatisticsQuery(WGPUComputePassEncoder* computePassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderInsertDebugMarker")]
    extern public static unsafe void ComputePassEncoderInsertDebugMarker(WGPUComputePassEncoder* computePassEncoder, byte* markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderInsertDebugMarker")]
    extern public static unsafe void ComputePassEncoderInsertDebugMarker(WGPUComputePassEncoder* computePassEncoder, in byte markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderInsertDebugMarker")]
    extern public static unsafe void ComputePassEncoderInsertDebugMarker(WGPUComputePassEncoder* computePassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderPopDebugGroup")]
    extern public static unsafe void ComputePassEncoderPopDebugGroup(WGPUComputePassEncoder* computePassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderPushDebugGroup")]
    extern public static unsafe void ComputePassEncoderPushDebugGroup(WGPUComputePassEncoder* computePassEncoder, byte* groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderPushDebugGroup")]
    extern public static unsafe void ComputePassEncoderPushDebugGroup(WGPUComputePassEncoder* computePassEncoder, in byte groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderPushDebugGroup")]
    extern public static unsafe void ComputePassEncoderPushDebugGroup(WGPUComputePassEncoder* computePassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderSetBindGroup")]
    extern public static unsafe void ComputePassEncoderSetBindGroup(WGPUComputePassEncoder* computePassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderSetBindGroup")]
    extern public static unsafe void ComputePassEncoderSetBindGroup(WGPUComputePassEncoder* computePassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderSetLabel")]
    extern public static unsafe void ComputePassEncoderSetLabel(WGPUComputePassEncoder* computePassEncoder, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderSetLabel")]
    extern public static unsafe void ComputePassEncoderSetLabel(WGPUComputePassEncoder* computePassEncoder, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderSetLabel")]
    extern public static unsafe void ComputePassEncoderSetLabel(WGPUComputePassEncoder* computePassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderSetPipeline")]
    extern public static unsafe void ComputePassEncoderSetPipeline(WGPUComputePassEncoder* computePassEncoder, WGPUComputePipeline* pipeline);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderReference")]
    extern public static unsafe void ComputePassEncoderReference(WGPUComputePassEncoder* computePassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePassEncoderRelease")]
    extern public static unsafe void ComputePassEncoderRelease(WGPUComputePassEncoder* computePassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePipelineGetBindGroupLayout")]
    extern public static unsafe WGPUBindGroupLayout* ComputePipelineGetBindGroupLayout(WGPUComputePipeline* computePipeline, uint groupIndex);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePipelineSetLabel")]
    extern public static unsafe void ComputePipelineSetLabel(WGPUComputePipeline* computePipeline, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePipelineSetLabel")]
    extern public static unsafe void ComputePipelineSetLabel(WGPUComputePipeline* computePipeline, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePipelineSetLabel")]
    extern public static unsafe void ComputePipelineSetLabel(WGPUComputePipeline* computePipeline, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePipelineReference")]
    extern public static unsafe void ComputePipelineReference(WGPUComputePipeline* computePipeline);
    [DllImport(LibraryName, EntryPoint = "wgpuComputePipelineRelease")]
    extern public static unsafe void ComputePipelineRelease(WGPUComputePipeline* computePipeline);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateBindGroup")]
    extern public static unsafe WGPUBindGroup* DeviceCreateBindGroup(WGPUDevice* device, WGPUBindGroupDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateBindGroup")]
    extern public static unsafe WGPUBindGroup* DeviceCreateBindGroup(WGPUDevice* device, in WGPUBindGroupDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateBindGroupLayout")]
    extern public static unsafe WGPUBindGroupLayout* DeviceCreateBindGroupLayout(WGPUDevice* device, WGPUBindGroupLayoutDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateBindGroupLayout")]
    extern public static unsafe WGPUBindGroupLayout* DeviceCreateBindGroupLayout(WGPUDevice* device, in WGPUBindGroupLayoutDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateBuffer")]
    extern public static unsafe WGPUBuffer* DeviceCreateBuffer(WGPUDevice* device, WGPUBufferDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateBuffer")]
    extern public static unsafe WGPUBuffer* DeviceCreateBuffer(WGPUDevice* device, in WGPUBufferDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateCommandEncoder")]
    extern public static unsafe WGPUCommandEncoder* DeviceCreateCommandEncoder(WGPUDevice* device, WGPUCommandEncoderDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateCommandEncoder")]
    extern public static unsafe WGPUCommandEncoder* DeviceCreateCommandEncoder(WGPUDevice* device, in WGPUCommandEncoderDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateComputePipeline")]
    extern public static unsafe WGPUComputePipeline* DeviceCreateComputePipeline(WGPUDevice* device, WGPUComputePipelineDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateComputePipeline")]
    extern public static unsafe WGPUComputePipeline* DeviceCreateComputePipeline(WGPUDevice* device, in WGPUComputePipelineDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateComputePipelineAsync")]
    extern public static unsafe void DeviceCreateComputePipelineAsync(WGPUDevice* device, WGPUComputePipelineDescriptor* descriptor, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateComputePipelineAsync")]
    extern public static unsafe void DeviceCreateComputePipelineAsync(WGPUDevice* device, in WGPUComputePipelineDescriptor descriptor, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreatePipelineLayout")]
    extern public static unsafe WGPUPipelineLayout* DeviceCreatePipelineLayout(WGPUDevice* device, WGPUPipelineLayoutDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreatePipelineLayout")]
    extern public static unsafe WGPUPipelineLayout* DeviceCreatePipelineLayout(WGPUDevice* device, in WGPUPipelineLayoutDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateQuerySet")]
    extern public static unsafe WGPUQuerySet* DeviceCreateQuerySet(WGPUDevice* device, WGPUQuerySetDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateQuerySet")]
    extern public static unsafe WGPUQuerySet* DeviceCreateQuerySet(WGPUDevice* device, in WGPUQuerySetDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateRenderBundleEncoder")]
    extern public static unsafe WGPURenderBundleEncoder* DeviceCreateRenderBundleEncoder(WGPUDevice* device, WGPURenderBundleEncoderDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateRenderBundleEncoder")]
    extern public static unsafe WGPURenderBundleEncoder* DeviceCreateRenderBundleEncoder(WGPUDevice* device, in WGPURenderBundleEncoderDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateRenderPipeline")]
    extern public static unsafe WGPURenderPipeline* DeviceCreateRenderPipeline(WGPUDevice* device, WGPURenderPipelineDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateRenderPipeline")]
    extern public static unsafe WGPURenderPipeline* DeviceCreateRenderPipeline(WGPUDevice* device, in WGPURenderPipelineDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateRenderPipelineAsync")]
    extern public static unsafe void DeviceCreateRenderPipelineAsync(WGPUDevice* device, WGPURenderPipelineDescriptor* descriptor, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateRenderPipelineAsync")]
    extern public static unsafe void DeviceCreateRenderPipelineAsync(WGPUDevice* device, in WGPURenderPipelineDescriptor descriptor, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateSampler")]
    extern public static unsafe WGPUSampler* DeviceCreateSampler(WGPUDevice* device, WGPUSamplerDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateSampler")]
    extern public static unsafe WGPUSampler* DeviceCreateSampler(WGPUDevice* device, in WGPUSamplerDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateShaderModule")]
    extern public static unsafe WGPUShaderModule* DeviceCreateShaderModule(WGPUDevice* device, WGPUShaderModuleDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateShaderModule")]
    extern public static unsafe WGPUShaderModule* DeviceCreateShaderModule(WGPUDevice* device, in WGPUShaderModuleDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateTexture")]
    extern public static unsafe WGPUTexture* DeviceCreateTexture(WGPUDevice* device, WGPUTextureDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateTexture")]
    extern public static unsafe WGPUTexture* DeviceCreateTexture(WGPUDevice* device, in WGPUTextureDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceDestroy")]
    extern public static unsafe void DeviceDestroy(WGPUDevice* device);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceEnumerateFeatures")]
    extern public static unsafe nuint DeviceEnumerateFeatures(WGPUDevice* device, WGPUFeatureName* features);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceEnumerateFeatures")]
    extern public static unsafe nuint DeviceEnumerateFeatures(WGPUDevice* device, ref WGPUFeatureName features);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceGetLimits")]
    extern public static unsafe bool DeviceGetLimits(WGPUDevice* device, WGPUSupportedLimits* limits);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceGetLimits")]
    extern public static unsafe bool DeviceGetLimits(WGPUDevice* device, ref WGPUSupportedLimits limits);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceGetQueue")]
    extern public static unsafe WGPUQueue* DeviceGetQueue(WGPUDevice* device);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceHasFeature")]
    extern public static unsafe bool DeviceHasFeature(WGPUDevice* device, WGPUFeatureName feature);
    [DllImport(LibraryName, EntryPoint = "wgpuDevicePopErrorScope")]
    extern public static unsafe void DevicePopErrorScope(WGPUDevice* device, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuDevicePushErrorScope")]
    extern public static unsafe void DevicePushErrorScope(WGPUDevice* device, WGPUErrorFilter filter);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceSetLabel")]
    extern public static unsafe void DeviceSetLabel(WGPUDevice* device, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceSetLabel")]
    extern public static unsafe void DeviceSetLabel(WGPUDevice* device, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceSetLabel")]
    extern public static unsafe void DeviceSetLabel(WGPUDevice* device, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceSetUncapturedErrorCallback")]
    extern public static unsafe void DeviceSetUncapturedErrorCallback(WGPUDevice* device, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceReference")]
    extern public static unsafe void DeviceReference(WGPUDevice* device);
    [DllImport(LibraryName, EntryPoint = "wgpuDeviceRelease")]
    extern public static unsafe void DeviceRelease(WGPUDevice* device);
    [DllImport(LibraryName, EntryPoint = "wgpuInstanceCreateSurface")]
    extern public static unsafe WGPUSurface* InstanceCreateSurface(WGPUInstance* instance, WGPUSurfaceDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuInstanceCreateSurface")]
    extern public static unsafe WGPUSurface* InstanceCreateSurface(WGPUInstance* instance, in WGPUSurfaceDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuInstanceProcessEvents")]
    extern public static unsafe void InstanceProcessEvents(WGPUInstance* instance);

    [DllImport(LibraryName, EntryPoint = "wgpuInstanceRequestAdapter")]
    extern public static unsafe void InstanceRequestAdapter(WGPUInstance* instance, WGPURequestAdapterOptions* options, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuInstanceRequestAdapter")]
    extern public static unsafe void InstanceRequestAdapter(WGPUInstance* instance, in WGPURequestAdapterOptions options, nint callback, void* userdata);

    [DllImport(LibraryName, EntryPoint = "wgpuInstanceReference")]
    extern public static unsafe void InstanceReference(WGPUInstance* instance);
    [DllImport(LibraryName, EntryPoint = "wgpuInstanceRelease")]
    extern public static unsafe void InstanceRelease(WGPUInstance* instance);
    [DllImport(LibraryName, EntryPoint = "wgpuPipelineLayoutSetLabel")]
    extern public static unsafe void PipelineLayoutSetLabel(WGPUPipelineLayout* pipelineLayout, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuPipelineLayoutSetLabel")]
    extern public static unsafe void PipelineLayoutSetLabel(WGPUPipelineLayout* pipelineLayout, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuPipelineLayoutSetLabel")]
    extern public static unsafe void PipelineLayoutSetLabel(WGPUPipelineLayout* pipelineLayout, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuPipelineLayoutReference")]
    extern public static unsafe void PipelineLayoutReference(WGPUPipelineLayout* pipelineLayout);
    [DllImport(LibraryName, EntryPoint = "wgpuPipelineLayoutRelease")]
    extern public static unsafe void PipelineLayoutRelease(WGPUPipelineLayout* pipelineLayout);
    [DllImport(LibraryName, EntryPoint = "wgpuQuerySetDestroy")]
    extern public static unsafe void QuerySetDestroy(WGPUQuerySet* querySet);
    [DllImport(LibraryName, EntryPoint = "wgpuQuerySetGetCount")]
    extern public static unsafe uint QuerySetGetCount(WGPUQuerySet* querySet);
    [DllImport(LibraryName, EntryPoint = "wgpuQuerySetGetType")]
    extern public static unsafe WGPUQueryType QuerySetGetType(WGPUQuerySet* querySet);
    [DllImport(LibraryName, EntryPoint = "wgpuQuerySetSetLabel")]
    extern public static unsafe void QuerySetSetLabel(WGPUQuerySet* querySet, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuQuerySetSetLabel")]
    extern public static unsafe void QuerySetSetLabel(WGPUQuerySet* querySet, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuQuerySetSetLabel")]
    extern public static unsafe void QuerySetSetLabel(WGPUQuerySet* querySet, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuQuerySetReference")]
    extern public static unsafe void QuerySetReference(WGPUQuerySet* querySet);
    [DllImport(LibraryName, EntryPoint = "wgpuQuerySetRelease")]
    extern public static unsafe void QuerySetRelease(WGPUQuerySet* querySet);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueOnSubmittedWorkDone")]
    extern public static unsafe void QueueOnSubmittedWorkDone(WGPUQueue* queue, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueSetLabel")]
    extern public static unsafe void QueueSetLabel(WGPUQueue* queue, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueSetLabel")]
    extern public static unsafe void QueueSetLabel(WGPUQueue* queue, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueSetLabel")]
    extern public static unsafe void QueueSetLabel(WGPUQueue* queue, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueSubmit")]
    extern public static unsafe void QueueSubmit(WGPUQueue* queue, nuint commandCount, WGPUCommandBuffer** commands);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueSubmit")]
    extern public static unsafe void QueueSubmit(WGPUQueue* queue, nuint commandCount, ref WGPUCommandBuffer* commands);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueWriteBuffer")]
    extern public static unsafe void QueueWriteBuffer(WGPUQueue* queue, WGPUBuffer* buffer, ulong bufferOffset, void* data, nuint size);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, in WGPUExtent3D writeSize);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, WGPUExtent3D* writeSize);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, in WGPUExtent3D writeSize);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, in WGPUExtent3D writeSize);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, WGPUExtent3D* writeSize);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, in WGPUExtent3D writeSize);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueReference")]
    extern public static unsafe void QueueReference(WGPUQueue* queue);
    [DllImport(LibraryName, EntryPoint = "wgpuQueueRelease")]
    extern public static unsafe void QueueRelease(WGPUQueue* queue);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleSetLabel")]
    extern public static unsafe void RenderBundleSetLabel(WGPURenderBundle* renderBundle, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleSetLabel")]
    extern public static unsafe void RenderBundleSetLabel(WGPURenderBundle* renderBundle, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleSetLabel")]
    extern public static unsafe void RenderBundleSetLabel(WGPURenderBundle* renderBundle, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleReference")]
    extern public static unsafe void RenderBundleReference(WGPURenderBundle* renderBundle);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleRelease")]
    extern public static unsafe void RenderBundleRelease(WGPURenderBundle* renderBundle);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderDraw")]
    extern public static unsafe void RenderBundleEncoderDraw(WGPURenderBundleEncoder* renderBundleEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderDrawIndexed")]
    extern public static unsafe void RenderBundleEncoderDrawIndexed(WGPURenderBundleEncoder* renderBundleEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderDrawIndexedIndirect")]
    extern public static unsafe void RenderBundleEncoderDrawIndexedIndirect(WGPURenderBundleEncoder* renderBundleEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderDrawIndirect")]
    extern public static unsafe void RenderBundleEncoderDrawIndirect(WGPURenderBundleEncoder* renderBundleEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderFinish")]
    extern public static unsafe WGPURenderBundle* RenderBundleEncoderFinish(WGPURenderBundleEncoder* renderBundleEncoder, WGPURenderBundleDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderFinish")]
    extern public static unsafe WGPURenderBundle* RenderBundleEncoderFinish(WGPURenderBundleEncoder* renderBundleEncoder, in WGPURenderBundleDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderInsertDebugMarker")]
    extern public static unsafe void RenderBundleEncoderInsertDebugMarker(WGPURenderBundleEncoder* renderBundleEncoder, byte* markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderInsertDebugMarker")]
    extern public static unsafe void RenderBundleEncoderInsertDebugMarker(WGPURenderBundleEncoder* renderBundleEncoder, in byte markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderInsertDebugMarker")]
    extern public static unsafe void RenderBundleEncoderInsertDebugMarker(WGPURenderBundleEncoder* renderBundleEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderPopDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPopDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderPushDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPushDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder, byte* groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderPushDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPushDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder, in byte groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderPushDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPushDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderSetBindGroup")]
    extern public static unsafe void RenderBundleEncoderSetBindGroup(WGPURenderBundleEncoder* renderBundleEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderSetBindGroup")]
    extern public static unsafe void RenderBundleEncoderSetBindGroup(WGPURenderBundleEncoder* renderBundleEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderSetIndexBuffer")]
    extern public static unsafe void RenderBundleEncoderSetIndexBuffer(WGPURenderBundleEncoder* renderBundleEncoder, WGPUBuffer* buffer, WGPUIndexFormat format, ulong offset, ulong size);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderSetLabel")]
    extern public static unsafe void RenderBundleEncoderSetLabel(WGPURenderBundleEncoder* renderBundleEncoder, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderSetLabel")]
    extern public static unsafe void RenderBundleEncoderSetLabel(WGPURenderBundleEncoder* renderBundleEncoder, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderSetLabel")]
    extern public static unsafe void RenderBundleEncoderSetLabel(WGPURenderBundleEncoder* renderBundleEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderSetPipeline")]
    extern public static unsafe void RenderBundleEncoderSetPipeline(WGPURenderBundleEncoder* renderBundleEncoder, WGPURenderPipeline* pipeline);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderSetVertexBuffer")]
    extern public static unsafe void RenderBundleEncoderSetVertexBuffer(WGPURenderBundleEncoder* renderBundleEncoder, uint slot, WGPUBuffer* buffer, ulong offset, ulong size);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderReference")]
    extern public static unsafe void RenderBundleEncoderReference(WGPURenderBundleEncoder* renderBundleEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderBundleEncoderRelease")]
    extern public static unsafe void RenderBundleEncoderRelease(WGPURenderBundleEncoder* renderBundleEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderBeginOcclusionQuery")]
    extern public static unsafe void RenderPassEncoderBeginOcclusionQuery(WGPURenderPassEncoder* renderPassEncoder, uint queryIndex);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderBeginPipelineStatisticsQuery")]
    extern public static unsafe void RenderPassEncoderBeginPipelineStatisticsQuery(WGPURenderPassEncoder* renderPassEncoder, WGPUQuerySet* querySet, uint queryIndex);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderDraw")]
    extern public static unsafe void RenderPassEncoderDraw(WGPURenderPassEncoder* renderPassEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderDrawIndexed")]
    extern public static unsafe void RenderPassEncoderDrawIndexed(WGPURenderPassEncoder* renderPassEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderDrawIndexedIndirect")]
    extern public static unsafe void RenderPassEncoderDrawIndexedIndirect(WGPURenderPassEncoder* renderPassEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderDrawIndirect")]
    extern public static unsafe void RenderPassEncoderDrawIndirect(WGPURenderPassEncoder* renderPassEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderEnd")]
    extern public static unsafe void RenderPassEncoderEnd(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderEndOcclusionQuery")]
    extern public static unsafe void RenderPassEncoderEndOcclusionQuery(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderEndPipelineStatisticsQuery")]
    extern public static unsafe void RenderPassEncoderEndPipelineStatisticsQuery(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderExecuteBundles")]
    extern public static unsafe void RenderPassEncoderExecuteBundles(WGPURenderPassEncoder* renderPassEncoder, nuint bundleCount, WGPURenderBundle** bundles);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderExecuteBundles")]
    extern public static unsafe void RenderPassEncoderExecuteBundles(WGPURenderPassEncoder* renderPassEncoder, nuint bundleCount, ref WGPURenderBundle* bundles);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderInsertDebugMarker")]
    extern public static unsafe void RenderPassEncoderInsertDebugMarker(WGPURenderPassEncoder* renderPassEncoder, byte* markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderInsertDebugMarker")]
    extern public static unsafe void RenderPassEncoderInsertDebugMarker(WGPURenderPassEncoder* renderPassEncoder, in byte markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderInsertDebugMarker")]
    extern public static unsafe void RenderPassEncoderInsertDebugMarker(WGPURenderPassEncoder* renderPassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderPopDebugGroup")]
    extern public static unsafe void RenderPassEncoderPopDebugGroup(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderPushDebugGroup")]
    extern public static unsafe void RenderPassEncoderPushDebugGroup(WGPURenderPassEncoder* renderPassEncoder, byte* groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderPushDebugGroup")]
    extern public static unsafe void RenderPassEncoderPushDebugGroup(WGPURenderPassEncoder* renderPassEncoder, in byte groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderPushDebugGroup")]
    extern public static unsafe void RenderPassEncoderPushDebugGroup(WGPURenderPassEncoder* renderPassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetBindGroup")]
    extern public static unsafe void RenderPassEncoderSetBindGroup(WGPURenderPassEncoder* renderPassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetBindGroup")]
    extern public static unsafe void RenderPassEncoderSetBindGroup(WGPURenderPassEncoder* renderPassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetBlendConstant")]
    extern public static unsafe void RenderPassEncoderSetBlendConstant(WGPURenderPassEncoder* renderPassEncoder, WGPUColor* color);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetBlendConstant")]
    extern public static unsafe void RenderPassEncoderSetBlendConstant(WGPURenderPassEncoder* renderPassEncoder, in WGPUColor color);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetIndexBuffer")]
    extern public static unsafe void RenderPassEncoderSetIndexBuffer(WGPURenderPassEncoder* renderPassEncoder, WGPUBuffer* buffer, WGPUIndexFormat format, ulong offset, ulong size);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetLabel")]
    extern public static unsafe void RenderPassEncoderSetLabel(WGPURenderPassEncoder* renderPassEncoder, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetLabel")]
    extern public static unsafe void RenderPassEncoderSetLabel(WGPURenderPassEncoder* renderPassEncoder, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetLabel")]
    extern public static unsafe void RenderPassEncoderSetLabel(WGPURenderPassEncoder* renderPassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetPipeline")]
    extern public static unsafe void RenderPassEncoderSetPipeline(WGPURenderPassEncoder* renderPassEncoder, WGPURenderPipeline* pipeline);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetScissorRect")]
    extern public static unsafe void RenderPassEncoderSetScissorRect(WGPURenderPassEncoder* renderPassEncoder, uint x, uint y, uint width, uint height);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetStencilReference")]
    extern public static unsafe void RenderPassEncoderSetStencilReference(WGPURenderPassEncoder* renderPassEncoder, uint reference);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetVertexBuffer")]
    extern public static unsafe void RenderPassEncoderSetVertexBuffer(WGPURenderPassEncoder* renderPassEncoder, uint slot, WGPUBuffer* buffer, ulong offset, ulong size);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderSetViewport")]
    extern public static unsafe void RenderPassEncoderSetViewport(WGPURenderPassEncoder* renderPassEncoder, float x, float y, float width, float height, float minDepth, float maxDepth);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderReference")]
    extern public static unsafe void RenderPassEncoderReference(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPassEncoderRelease")]
    extern public static unsafe void RenderPassEncoderRelease(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPipelineGetBindGroupLayout")]
    extern public static unsafe WGPUBindGroupLayout* RenderPipelineGetBindGroupLayout(WGPURenderPipeline* renderPipeline, uint groupIndex);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPipelineSetLabel")]
    extern public static unsafe void RenderPipelineSetLabel(WGPURenderPipeline* renderPipeline, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPipelineSetLabel")]
    extern public static unsafe void RenderPipelineSetLabel(WGPURenderPipeline* renderPipeline, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPipelineSetLabel")]
    extern public static unsafe void RenderPipelineSetLabel(WGPURenderPipeline* renderPipeline, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPipelineReference")]
    extern public static unsafe void RenderPipelineReference(WGPURenderPipeline* renderPipeline);
    [DllImport(LibraryName, EntryPoint = "wgpuRenderPipelineRelease")]
    extern public static unsafe void RenderPipelineRelease(WGPURenderPipeline* renderPipeline);
    [DllImport(LibraryName, EntryPoint = "wgpuSamplerSetLabel")]
    extern public static unsafe void SamplerSetLabel(WGPUSampler* sampler, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuSamplerSetLabel")]
    extern public static unsafe void SamplerSetLabel(WGPUSampler* sampler, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuSamplerSetLabel")]
    extern public static unsafe void SamplerSetLabel(WGPUSampler* sampler, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuSamplerReference")]
    extern public static unsafe void SamplerReference(WGPUSampler* sampler);
    [DllImport(LibraryName, EntryPoint = "wgpuSamplerRelease")]
    extern public static unsafe void SamplerRelease(WGPUSampler* sampler);
    [DllImport(LibraryName, EntryPoint = "wgpuShaderModuleGetCompilationInfo")]
    extern public static unsafe void ShaderModuleGetCompilationInfo(WGPUShaderModule* shaderModule, nint callback, void* userdata);
    [DllImport(LibraryName, EntryPoint = "wgpuShaderModuleSetLabel")]
    extern public static unsafe void ShaderModuleSetLabel(WGPUShaderModule* shaderModule, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuShaderModuleSetLabel")]
    extern public static unsafe void ShaderModuleSetLabel(WGPUShaderModule* shaderModule, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuShaderModuleSetLabel")]
    extern public static unsafe void ShaderModuleSetLabel(WGPUShaderModule* shaderModule, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuShaderModuleReference")]
    extern public static unsafe void ShaderModuleReference(WGPUShaderModule* shaderModule);
    [DllImport(LibraryName, EntryPoint = "wgpuShaderModuleRelease")]
    extern public static unsafe void ShaderModuleRelease(WGPUShaderModule* shaderModule);
    [DllImport(LibraryName, EntryPoint = "wgpuSurfaceGetPreferredFormat")]
    extern public static unsafe WGPUTextureFormat SurfaceGetPreferredFormat(WGPUSurface* surface, WGPUAdapter* adapter);
    [DllImport(LibraryName, EntryPoint = "wgpuSurfacePresent")]
    extern public static unsafe void SurfacePresent(WGPUSurface* surface);
    [DllImport(LibraryName, EntryPoint = "wgpuSurfaceUnconfigure")]
    extern public static unsafe void SurfaceUnconfigure(WGPUSurface* surface);
    [DllImport(LibraryName, EntryPoint = "wgpuSurfaceReference")]
    extern public static unsafe void SurfaceReference(WGPUSurface* surface);
    [DllImport(LibraryName, EntryPoint = "wgpuSurfaceRelease")]
    extern public static unsafe void SurfaceRelease(WGPUSurface* surface);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureCreateView")]
    extern public static unsafe WGPUTextureView* TextureCreateView(WGPUTexture* texture, WGPUTextureViewDescriptor* descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureCreateView")]
    extern public static unsafe WGPUTextureView* TextureCreateView(WGPUTexture* texture, in WGPUTextureViewDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureDestroy")]
    extern public static unsafe void TextureDestroy(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureGetDepthOrArrayLayers")]
    extern public static unsafe uint TextureGetDepthOrArrayLayers(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureGetDimension")]
    extern public static unsafe WGPUTextureDimension TextureGetDimension(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureGetFormat")]
    extern public static unsafe WGPUTextureFormat TextureGetFormat(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureGetHeight")]
    extern public static unsafe uint TextureGetHeight(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureGetMipLevelCount")]
    extern public static unsafe uint TextureGetMipLevelCount(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureGetSampleCount")]
    extern public static unsafe uint TextureGetSampleCount(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureGetUsage")]
    extern public static unsafe WGPUTextureUsage TextureGetUsage(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureGetWidth")]
    extern public static unsafe uint TextureGetWidth(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureSetLabel")]
    extern public static unsafe void TextureSetLabel(WGPUTexture* texture, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureSetLabel")]
    extern public static unsafe void TextureSetLabel(WGPUTexture* texture, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureSetLabel")]
    extern public static unsafe void TextureSetLabel(WGPUTexture* texture, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureReference")]
    extern public static unsafe void TextureReference(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureRelease")]
    extern public static unsafe void TextureRelease(WGPUTexture* texture);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureViewSetLabel")]
    extern public static unsafe void TextureViewSetLabel(WGPUTextureView* textureView, byte* label);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureViewSetLabel")]
    extern public static unsafe void TextureViewSetLabel(WGPUTextureView* textureView, in byte label);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureViewSetLabel")]
    extern public static unsafe void TextureViewSetLabel(WGPUTextureView* textureView, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureViewReference")]
    extern public static unsafe void TextureViewReference(WGPUTextureView* textureView);
    [DllImport(LibraryName, EntryPoint = "wgpuTextureViewRelease")]
    extern public static unsafe void TextureViewRelease(WGPUTextureView* textureView);

    [DllImport(LibraryName, EntryPoint = "wgpuDeviceCreateSwapChain")]
    extern public static unsafe WGPUSwapChain* DeviceCreateSwapChain(WGPUDevice* device, WGPUSurface* surface, in WGPUSwapChainDescriptor descriptor);
    [DllImport(LibraryName, EntryPoint = "wgpuSwapChainGetCurrentTextureView")]
    extern public static unsafe WGPUTextureView* SwapChainGetCurrentTextureView(WGPUSwapChain* swapChain);
    [DllImport(LibraryName, EntryPoint = "wgpuSwapChainRelease")]
    extern public static unsafe void SwapChainRelease(WGPUSwapChain* swapChain);
}
