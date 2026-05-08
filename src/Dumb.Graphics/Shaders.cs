using System.Threading;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public static unsafe class Shaders
{
    public static Entity Wgsl(GraphicsContext ctx, string source)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(source + '\0');
        fixed (byte* sourcePtr = bytes)
        {
            ShaderModuleWGSLDescriptor wgsl = new()
            {
                Chain = new ChainedStruct
                {
                    Next = null,
                    SType = SType.ShaderModuleWgslDescriptor
                },
                Code = sourcePtr
            };

            ShaderModuleDescriptor descriptor = new()
            {
                NextInChain = (ChainedStruct*)&wgsl,
                HintCount = 0,
                Hints = null,
                Label = null
            };
            return Create(ctx, descriptor);
        }
    }

    public static Entity Create(GraphicsContext ctx, ShaderModuleDescriptor descriptor)
    {
        nint native = ctx.Device.CreateShaderModule(ctx.NativeDevice, &descriptor);
        return ctx._shaders.Create(HList.From(new ShaderData
        {
            NativePtr = native,
            RefCount = 1
        }));
    }

    internal static void Release(GraphicsContext ctx, Entity shader)
    {
        ref var s = ref shader.Get<ShaderData>();
        if (Interlocked.Decrement(ref s.RefCount) == 0)
        {
            ctx.Device.ReleaseShaderModule(s.NativePtr);
            shader.Destroy();
        }
    }
}
