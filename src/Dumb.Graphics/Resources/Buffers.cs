using System.Runtime.InteropServices;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public unsafe class BufferManager : GpuResourceManager<BufferData>
{
    public BufferManager(GraphicsContext ctx)
        : base(ctx, ctx._buffers)
    {
    }

    public Entity Create(ulong size, BufferUsage usage)
    {
        BufferDescriptor descriptor = new()
        {
            Usage = usage,
            Size = size,
            MappedAtCreation = false,
            Label = null
        };
        return Create(descriptor);
    }

    public Entity Create(BufferDescriptor descriptor)
    {
        var native = Ctx.Device.CreateBuffer(Ctx.NativeDevice, &descriptor);
        return CreateResource(new BufferData
        {
            NativePtr = native,
            Size = descriptor.Size,
            Usage = descriptor.Usage,
            RefCount = 1
        });
    }

    public void Write(Entity buffer, ulong offset, ReadOnlySpan<byte> data)
    {
        ref var buf = ref buffer.Get<BufferData>();
        fixed (byte* ptr = data)
            Ctx.Command.QueueWriteBuffer(Ctx.NativeQueue, buf.NativePtr, offset, ptr, (nuint)data.Length);
    }

    public void Write<T>(Entity buffer, in T value, ulong offset = 0)
        where T : unmanaged
    {
        fixed (T* ptr = &value)
            Write(buffer, offset, new ReadOnlySpan<byte>(ptr, sizeof(T)));
    }

    public void Write<T>(Entity buffer, ReadOnlySpan<T> values, ulong offset = 0)
        where T : unmanaged
    {
        Write(buffer, offset, MemoryMarshal.AsBytes(values));
    }

    public Entity Uniform<T>()
        where T : unmanaged
    {
        return Create((ulong)sizeof(T), BufferUsage.Uniform | BufferUsage.CopyDst);
    }

    public Entity Uniform<T>(in T value)
        where T : unmanaged
    {
        var buffer = Uniform<T>();
        Write(buffer, value);
        return buffer;
    }

    public Entity Storage<T>(
        int elementCount,
        BufferUsage extraUsage = BufferUsage.None)
        where T : unmanaged
    {
        return Create((ulong)(sizeof(T) * elementCount), BufferUsage.Storage | BufferUsage.CopyDst | extraUsage);
    }

    public Entity Vertex<T>(ReadOnlySpan<T> values)
        where T : unmanaged
    {
        var buffer = Create((ulong)(sizeof(T) * values.Length), BufferUsage.Vertex | BufferUsage.CopyDst);
        Write(buffer, values);
        return buffer;
    }

    public Entity Index<T>(ReadOnlySpan<T> values)
        where T : unmanaged
    {
        var buffer = Create((ulong)(sizeof(T) * values.Length), BufferUsage.Index | BufferUsage.CopyDst);
        Write(buffer, values);
        return buffer;
    }

    protected override ref int GetRefCountRef(ref BufferData data) => ref data.RefCount;

    protected override nint GetNativePtr(ref BufferData data) => data.NativePtr;

    protected override void ReleaseNative(nint nativePtr)
    {
        Ctx.Device.ReleaseBuffer(nativePtr);
    }
}
