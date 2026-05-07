using System.Threading;
using Dumb.Engine.Graph;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public static unsafe class Textures
{
    public static Handle<TextureData> Create(GraphicsContext ctx, TextureDescriptor descriptor)
    {
        nint native = ctx.Device.CreateTexture(ctx.NativeDevice, &descriptor);
        return ctx._textures.Create(new TextureData
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

    public static Handle<TextureViewData> CreateView(GraphicsContext ctx, Handle<TextureData> texture, TextureViewDescriptor descriptor)
    {
        ref var tex = ref ctx._textures.Get(texture);
        nint native = ctx.Device.CreateTextureView(tex.NativePtr, &descriptor);
        return ctx._textureViews.Create(new TextureViewData
        {
            NativePtr = native,
            TextureHandle = texture.Value,
            RefCount = 1
        });
    }

    internal static void Release(GraphicsContext ctx, Handle<TextureData> handle)
    {
        if (!ctx._textures.TryGet(handle, out var r)) return;
        ref var tex = ref r.Value;
        if (Interlocked.Decrement(ref tex.RefCount) == 0)
        {
            ctx.Device.ReleaseTexture(tex.NativePtr);
            ctx._textures.Destroy(handle);
        }
    }

    internal static void ReleaseView(GraphicsContext ctx, Handle<TextureViewData> handle)
    {
        if (!ctx._textureViews.TryGet(handle, out var r)) return;
        ref var view = ref r.Value;
        if (Interlocked.Decrement(ref view.RefCount) == 0)
        {
            ctx.Device.ReleaseTextureView(view.NativePtr);
            ctx._textureViews.Destroy(handle);
        }
    }
}
