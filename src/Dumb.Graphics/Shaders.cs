using System.Threading;
using Dumb.Engine.Graph;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public static unsafe class Shaders
{
    public static Handle<ShaderData> Create(GraphicsContext ctx, ShaderModuleDescriptor descriptor)
    {
        nint native = ctx.Device.CreateShaderModule(ctx.NativeDevice, &descriptor);
        return ctx._shaders.Create(new ShaderData
        {
            NativePtr = native,
            RefCount = 1
        });
    }

    internal static void Release(GraphicsContext ctx, Handle<ShaderData> handle)
    {
        if (!ctx._shaders.TryGet(handle, out var r)) return;
        ref var shader = ref r.Value;
        if (Interlocked.Decrement(ref shader.RefCount) == 0)
        {
            ctx.Device.ReleaseShaderModule(shader.NativePtr);
            ctx._shaders.Destroy(handle);
        }
    }
}
