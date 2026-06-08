using System.Text;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public unsafe class ShaderManager : GpuResourceManager<ShaderData>
{
    public ShaderManager(GraphicsContext ctx)
        : base(ctx, ctx._shaders)
    {
    }

    public Entity Wgsl(string source)
    {
        var bytes = Encoding.UTF8.GetBytes(source + '\0');
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
            return Create(descriptor);
        }
    }

    public Entity Create(ShaderModuleDescriptor descriptor)
    {
        var native = Ctx.Device.CreateShaderModule(Ctx.NativeDevice, &descriptor);
        return CreateResource(new ShaderData
        {
            NativePtr = native,
            RefCount = 1
        });
    }

    protected override ref int GetRefCountRef(ref ShaderData data) => ref data.RefCount;

    protected override nint GetNativePtr(ref ShaderData data) => data.NativePtr;

    protected override void ReleaseNative(nint nativePtr)
    {
        Ctx.Device.ReleaseShaderModule(nativePtr);
    }
}
