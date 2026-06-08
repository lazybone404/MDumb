using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public unsafe class SamplerManager : GpuResourceManager<SamplerData>
{
    public SamplerManager(GraphicsContext ctx)
        : base(ctx, ctx._samplers)
    {
    }

    public Entity LinearClamp() =>
        Create(AddressMode.ClampToEdge, AddressMode.ClampToEdge, AddressMode.ClampToEdge,
            FilterMode.Linear, FilterMode.Linear, MipmapFilterMode.Linear);

    public Entity Create(
        AddressMode addressModeU,
        AddressMode addressModeV,
        AddressMode addressModeW,
        FilterMode mag,
        FilterMode min,
        MipmapFilterMode mip)
    {
        SamplerDescriptor descriptor = new()
        {
            AddressModeU = addressModeU,
            AddressModeV = addressModeV,
            AddressModeW = addressModeW,
            MagFilter = mag,
            MinFilter = min,
            MipmapFilter = mip,
            LodMinClamp = 0,
            LodMaxClamp = 1,
            Compare = CompareFunction.Undefined,
            MaxAnisotropy = 1,
            Label = null
        };
        return Create(descriptor);
    }

    public Entity Create(SamplerDescriptor descriptor)
    {
        var native = Ctx.Device.CreateSampler(Ctx.NativeDevice, &descriptor);
        return CreateResource(new SamplerData
        {
            NativePtr = native,
            RefCount = 1
        });
    }

    protected override ref int GetRefCountRef(ref SamplerData data) => ref data.RefCount;

    protected override nint GetNativePtr(ref SamplerData data) => data.NativePtr;

    protected override void ReleaseNative(nint nativePtr)
    {
        Ctx.Device.ReleaseSampler(nativePtr);
    }
}
