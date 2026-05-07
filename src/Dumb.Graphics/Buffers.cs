using System.Threading;
using Dumb.Engine.Graph;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public static unsafe class Buffers
{
    public static Handle<BufferData> Create(GraphicsContext ctx, BufferDescriptor descriptor)
    {
        nint native = ctx.Device.CreateBuffer(ctx.NativeDevice, &descriptor);
        return ctx._buffers.Create(new BufferData
        {
            NativePtr = native,
            Size = descriptor.Size,
            Usage = descriptor.Usage,
            RefCount = 1
        });
    }

    public static void Write(GraphicsContext ctx, Handle<BufferData> handle, ulong offset, ReadOnlySpan<byte> data)
    {
        ref var buf = ref ctx._buffers.Get(handle);
        fixed (byte* ptr = data)
            ctx.Command.QueueWriteBuffer(ctx.NativeQueue, buf.NativePtr, offset, ptr, (nuint)data.Length);
    }

    internal static void Retain(GraphicsContext ctx, Handle<BufferData> handle)
    {
        ref var buf = ref ctx._buffers.Get(handle);
        Interlocked.Increment(ref buf.RefCount);
    }

    internal static void Release(GraphicsContext ctx, Handle<BufferData> handle)
    {
        if (!ctx._buffers.TryGet(handle, out var r)) return;
        ref var buf = ref r.Value;
        if (Interlocked.Decrement(ref buf.RefCount) == 0)
        {
            ctx.Device.ReleaseBuffer(buf.NativePtr);
            ctx._buffers.Destroy(handle);
        }
    }
}
