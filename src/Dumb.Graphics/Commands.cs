using Dumb.Engine.Graph;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public static unsafe class Commands
{
    public static GpuEncoder CreateEncoder(GraphicsContext ctx, byte* label = null)
    {
        CommandEncoderDescriptor desc = new() { Label = label };
        nint native = ctx.Command.CreateCommandEncoder(ctx.NativeDevice, &desc);
        return new GpuEncoder(ctx, native);
    }

    public static void Submit(GraphicsContext ctx, nint commandBuffer)
    {
        var cb = commandBuffer;
        ctx.Command.QueueSubmit(ctx.NativeQueue, 1, &cb);
    }

    public static void SubmitAndRelease(GraphicsContext ctx, nint commandBuffer)
    {
        Submit(ctx, commandBuffer);
        ctx.Command.CommandBufferRelease(commandBuffer);
    }

    public static void Submit(GraphicsContext ctx, ReadOnlySpan<nint> commandBuffers)
    {
        fixed (nint* ptr = commandBuffers)
            ctx.Command.QueueSubmit(ctx.NativeQueue, (nuint)commandBuffers.Length, ptr);
    }

    public static void ReleaseCommandBuffer(GraphicsContext ctx, nint commandBuffer)
    {
        ctx.Command.CommandBufferRelease(commandBuffer);
    }
}

public unsafe ref struct GpuEncoder
{
    readonly GraphicsContext _ctx;
    nint _encoder;

    internal GpuEncoder(GraphicsContext ctx, nint encoder)
    {
        _ctx = ctx;
        _encoder = encoder;
    }

    public RenderPass BeginRenderPass(RenderPassDescriptor* descriptor)
    {
        ThrowIfFinished();
        nint pass = _ctx.Command.CommandEncoderBeginRenderPass(_encoder, descriptor);
        return new RenderPass(_ctx, pass);
    }

    public ComputePass BeginComputePass(ComputePassDescriptor* descriptor)
    {
        ThrowIfFinished();
        nint pass = _ctx.Command.CommandEncoderBeginComputePass(_encoder, descriptor);
        return new ComputePass(_ctx, pass);
    }

    public void CopyBufferToBuffer(
        Handle<BufferData> source, ulong sourceOffset,
        Handle<BufferData> destination, ulong destinationOffset, ulong size)
    {
        ThrowIfFinished();
        ref var src = ref _ctx._buffers.Get(source);
        ref var dst = ref _ctx._buffers.Get(destination);
        _ctx.Command.CopyBufferToBuffer(_encoder, src.NativePtr, sourceOffset, dst.NativePtr, destinationOffset, size);
    }

    public nint Finish()
    {
        ThrowIfFinished();
        var commandBuffer = _ctx.Command.CommandEncoderFinish(_encoder, null);
        _ctx.Command.CommandEncoderRelease(_encoder);
        _encoder = 0;
        return commandBuffer;
    }

    public void Dispose()
    {
        if (_encoder == 0)
            return;

        _ctx.Command.CommandEncoderRelease(_encoder);
        _encoder = 0;
    }

    readonly void ThrowIfFinished()
    {
        if (_encoder == 0)
            throw new ObjectDisposedException(nameof(GpuEncoder));
    }
}

public unsafe ref struct RenderPass
{
    readonly GraphicsContext _ctx;
    nint _pass;

    internal RenderPass(GraphicsContext ctx, nint pass)
    {
        _ctx = ctx;
        _pass = pass;
    }

    public void SetPipeline(Handle<RenderPipelineData> pipeline)
    {
        ThrowIfEnded();
        ref var data = ref _ctx._renderPipelines.Get(pipeline);
        _ctx.Command.RenderPassEncoderSetPipeline(_pass, data.NativePtr);
    }

    public void SetBindGroup(uint groupIndex, Handle<BindGroupData> group, ReadOnlySpan<uint> dynamicOffsets = default)
    {
        ThrowIfEnded();
        ref var data = ref _ctx._bindGroups.Get(group);
        fixed (uint* offsets = dynamicOffsets)
            _ctx.Command.RenderPassEncoderSetBindGroup(_pass, groupIndex, data.NativePtr, (nuint)dynamicOffsets.Length, offsets);
    }

    public void SetVertexBuffer(uint slot, Handle<BufferData> buffer, ulong offset = 0, ulong size = unchecked((ulong)-1))
    {
        ThrowIfEnded();
        ref var data = ref _ctx._buffers.Get(buffer);
        if (size == unchecked((ulong)-1)) size = data.Size - offset;
        _ctx.Command.RenderPassEncoderSetVertexBuffer(_pass, slot, data.NativePtr, offset, size);
    }

    public void SetIndexBuffer(Handle<BufferData> buffer, IndexFormat format, ulong offset = 0, ulong size = unchecked((ulong)-1))
    {
        ThrowIfEnded();
        ref var data = ref _ctx._buffers.Get(buffer);
        if (size == unchecked((ulong)-1)) size = data.Size - offset;
        _ctx.Command.RenderPassEncoderSetIndexBuffer(_pass, data.NativePtr, format, offset, size);
    }

    public void SetViewport(float x, float y, float w, float h, float minDepth = 0, float maxDepth = 1)
    {
        ThrowIfEnded();
        _ctx.Command.RenderPassEncoderSetViewport(_pass, x, y, w, h, minDepth, maxDepth);
    }

    public void SetScissorRect(uint x, uint y, uint w, uint h)
    {
        ThrowIfEnded();
        _ctx.Command.RenderPassEncoderSetScissorRect(_pass, x, y, w, h);
    }

    public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
    {
        ThrowIfEnded();
        _ctx.Command.RenderPassEncoderDraw(_pass, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int baseVertex = 0, uint firstInstance = 0)
    {
        ThrowIfEnded();
        _ctx.Command.RenderPassEncoderDrawIndexed(_pass, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }

    public void End()
    {
        ThrowIfEnded();
        _ctx.Command.RenderPassEncoderEnd(_pass);
        _ctx.Command.RenderPassEncoderRelease(_pass);
        _pass = 0;
    }

    public void Dispose()
    {
        if (_pass != 0)
            End();
    }

    readonly void ThrowIfEnded()
    {
        if (_pass == 0)
            throw new ObjectDisposedException(nameof(RenderPass));
    }
}

public unsafe ref struct ComputePass
{
    readonly GraphicsContext _ctx;
    nint _pass;

    internal ComputePass(GraphicsContext ctx, nint pass)
    {
        _ctx = ctx;
        _pass = pass;
    }

    public void SetPipeline(Handle<ComputePipelineData> pipeline)
    {
        ThrowIfEnded();
        ref var data = ref _ctx._computePipelines.Get(pipeline);
        _ctx.Command.ComputePassEncoderSetPipeline(_pass, data.NativePtr);
    }

    public void SetBindGroup(uint groupIndex, Handle<BindGroupData> group, ReadOnlySpan<uint> dynamicOffsets = default)
    {
        ThrowIfEnded();
        ref var data = ref _ctx._bindGroups.Get(group);
        fixed (uint* offsets = dynamicOffsets)
            _ctx.Command.ComputePassEncoderSetBindGroup(_pass, groupIndex, data.NativePtr, (nuint)dynamicOffsets.Length, offsets);
    }

    public void Dispatch(uint x, uint y = 1, uint z = 1)
    {
        ThrowIfEnded();
        _ctx.Command.ComputePassEncoderDispatchWorkgroups(_pass, x, y, z);
    }

    public void End()
    {
        ThrowIfEnded();
        _ctx.Command.ComputePassEncoderEnd(_pass);
        _ctx.Command.ComputePassEncoderRelease(_pass);
        _pass = 0;
    }

    public void Dispose()
    {
        if (_pass != 0)
            End();
    }

    readonly void ThrowIfEnded()
    {
        if (_pass == 0)
            throw new ObjectDisposedException(nameof(ComputePass));
    }
}
