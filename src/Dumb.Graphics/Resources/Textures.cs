using System.Threading;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public readonly record struct RenderTexture(
    Entity Texture,
    Entity View,
    uint Width,
    uint Height,
    TextureFormat Format);

public unsafe class TextureManager : GpuResourceManager<TextureData>
{
    public TextureManager(GraphicsContext ctx)
        : base(ctx, ctx._textures)
    {
    }

    public RenderTexture RenderTarget(
        uint width,
        uint height,
        TextureFormat format,
        TextureUsage usage = TextureUsage.RenderAttachment | TextureUsage.TextureBinding)
    {
        var texture = Create2D(width, height, format, usage);
        var view = CreateView2D(texture, format);
        return new RenderTexture(texture, view, width, height, format);
    }

    public Entity Create2D(
        uint width,
        uint height,
        TextureFormat format,
        TextureUsage usage,
        uint mipLevelCount = 1,
        uint sampleCount = 1)
    {
        TextureDescriptor descriptor = new()
        {
            Usage = usage,
            Dimension = TextureDimension.Dimension2D,
            Size = new Extent3D(width, height, 1),
            Format = format,
            MipLevelCount = mipLevelCount,
            SampleCount = sampleCount,
            ViewFormatCount = 0,
            ViewFormats = null,
            Label = null
        };
        return Create(descriptor);
    }

    public Entity Create(TextureDescriptor descriptor)
    {
        var native = Ctx.Device.CreateTexture(Ctx.NativeDevice, &descriptor);
        return CreateResource(new TextureData
        {
            NativePtr = native,
            Size = descriptor.Size,
            Format = descriptor.Format,
            Usage = descriptor.Usage,
            MipLevelCount = descriptor.MipLevelCount,
            SampleCount = descriptor.SampleCount,
            RefCount = 1
        });
    }

    // Release(Entity) 继承自 GpuResourceManager<TextureData>，无需重写

    public Entity CreateView2D(
        Entity texture,
        TextureFormat format,
        TextureAspect aspect = TextureAspect.All)
    {
        TextureViewDescriptor descriptor = new()
        {
            Format = format,
            Dimension = TextureViewDimension.Dimension2D,
            BaseMipLevel = 0,
            MipLevelCount = 1,
            BaseArrayLayer = 0,
            ArrayLayerCount = 1,
            Aspect = aspect,
            Label = null
        };
        return CreateView(texture, descriptor);
    }

    public Entity CreateViewForNativeTexture(
        nint texture,
        TextureFormat format,
        TextureAspect aspect = TextureAspect.All)
    {
        TextureViewDescriptor descriptor = new()
        {
            Format = format,
            Dimension = TextureViewDimension.Dimension2D,
            BaseMipLevel = 0,
            MipLevelCount = 1,
            BaseArrayLayer = 0,
            ArrayLayerCount = 1,
            Aspect = aspect,
            Label = null
        };

        var native = Ctx.Device.CreateTextureView(texture, &descriptor);
        return Ctx._textureViews.Create(HList.From(new TextureViewData
        {
            NativePtr = native,
            Texture = null!,
            RefCount = 1
        }));
    }

    public Entity CreateDepthView(Entity texture, TextureFormat format = TextureFormat.Depth32float)
    {
        TextureViewDescriptor descriptor = new()
        {
            Format = format,
            Dimension = TextureViewDimension.Dimension2D,
            BaseMipLevel = 0,
            MipLevelCount = 1,
            BaseArrayLayer = 0,
            ArrayLayerCount = 1,
            Aspect = TextureAspect.DepthOnly,
            Label = null
        };
        return CreateView(texture, descriptor);
    }

    public Entity WrapNativeTextureView(nint textureView)
    {
        return Ctx._textureViews.Create(HList.From(new TextureViewData
        {
            NativePtr = textureView,
            Texture = null!,
            RefCount = 1
        }));
    }

    public Entity CreateView(Entity texture, TextureViewDescriptor descriptor)
    {
        ref var tex = ref texture.Get<TextureData>();
        var native = Ctx.Device.CreateTextureView(tex.NativePtr, &descriptor);
        return Ctx._textureViews.Create(HList.From(new TextureViewData
        {
            NativePtr = native,
            Texture = texture,
            RefCount = 1
        }));
    }

    public void ReleaseView(Entity view)
    {
        ref var v = ref view.Get<TextureViewData>();
        if (Interlocked.Decrement(ref v.RefCount) == 0)
        {
            Ctx.Device.ReleaseTextureView(v.NativePtr);
            view.Destroy();
        }
    }

    protected override ref int GetRefCountRef(ref TextureData data) => ref data.RefCount;

    protected override nint GetNativePtr(ref TextureData data) => data.NativePtr;

    protected override void ReleaseNative(nint nativePtr)
    {
        Ctx.Device.ReleaseTexture(nativePtr);
    }
}
