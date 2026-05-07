using Silk.NET.WebGPU;

namespace Dumb.Graphics;

unsafe interface ICommandBackend
{
    nint CreateCommandEncoder(nint device, CommandEncoderDescriptor* descriptor);

    // Render pass
    nint CommandEncoderBeginRenderPass(nint encoder, RenderPassDescriptor* descriptor);
    void RenderPassEncoderSetPipeline(nint pass, nint pipeline);
    void RenderPassEncoderSetBindGroup(nint pass, uint groupIndex, nint group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    void RenderPassEncoderSetVertexBuffer(nint pass, uint slot, nint buffer, ulong offset, ulong size);
    void RenderPassEncoderSetIndexBuffer(nint pass, nint buffer, IndexFormat format, ulong offset, ulong size);
    void RenderPassEncoderSetViewport(nint pass, float x, float y, float w, float h, float minDepth, float maxDepth);
    void RenderPassEncoderSetScissorRect(nint pass, uint x, uint y, uint w, uint h);
    void RenderPassEncoderDraw(nint pass, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
    void RenderPassEncoderDrawIndexed(nint pass, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance);
    void RenderPassEncoderEnd(nint pass);

    // Compute pass
    nint CommandEncoderBeginComputePass(nint encoder, ComputePassDescriptor* descriptor);
    void ComputePassEncoderSetPipeline(nint pass, nint pipeline);
    void ComputePassEncoderSetBindGroup(nint pass, uint groupIndex, nint group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    void ComputePassEncoderDispatchWorkgroups(nint pass, uint x, uint y, uint z);
    void ComputePassEncoderEnd(nint pass);

    // Encoder finalization & queue
    nint CommandEncoderFinish(nint encoder, CommandBufferDescriptor* descriptor);
    void CommandEncoderRelease(nint encoder);
    void CommandBufferRelease(nint commandBuffer);
    void RenderPassEncoderRelease(nint pass);
    void ComputePassEncoderRelease(nint pass);
    void CopyBufferToBuffer(nint encoder, nint source, ulong sourceOffset, nint destination, ulong destinationOffset, ulong size);
    void QueueSubmit(nint queue, nuint commandCount, nint* commands);
    void QueueWriteBuffer(nint queue, nint buffer, ulong bufferOffset, void* data, nuint size);
    void QueueWriteTexture(nint queue, ImageCopyTexture* destination, void* data, nuint dataSize,
        TextureDataLayout* dataLayout, Extent3D* writeSize);
}
