using Sia;
using Silk.NET.WebGPU;
using Dumb.Graphics.Interfaces;

namespace Dumb.Graphics;

/// <summary>
/// 管理 WebGPU Surface 生命周期：创建、配置、帧获取、呈现、销毁。
/// 从 GraphicsContext 中提取以提升可测试性。
/// </summary>
public unsafe class SurfaceManager
{
    private readonly GraphicsContext _ctx;
    private readonly IDeviceBackend _device;
    private readonly ISwapChainBackend _swapChain;

    public SurfaceManager(GraphicsContext ctx, IDeviceBackend device, ISwapChainBackend swapChain)
    {
        _ctx = ctx;
        _device = device;
        _swapChain = swapChain;
    }

    public GraphicsSurface CreateFromNative(nint handle)
    {
        return new GraphicsSurface { Handle = handle, Context = _ctx };
    }

    public static void SetCompatibleSurface(ref RequestAdapterOptions options, in GraphicsSurface surface)
    {
        options.CompatibleSurface = (Surface*)surface.Handle;
    }

    public TextureFormat GetPreferredFormat(GraphicsSurface surface)
    {
        return _swapChain.GetPreferredFormat(surface.Handle, _ctx.NativeAdapter);
    }

    public void Configure(in GraphicsSurface surface,
        TextureUsage usage = TextureUsage.RenderAttachment,
        PresentMode presentMode = PresentMode.Fifo)
    {
        _swapChain.Configure(surface, _ctx.NativeDevice, usage, presentMode);
    }

    public SurfaceFrame BeginFrame(in GraphicsSurface surface)
    {
        var viewHandle = _swapChain.GetCurrentTextureView(surface);
        Entity view = null!;
        if (viewHandle != 0)
            view = _ctx.Textures.WrapNativeTextureView(viewHandle);
        return new SurfaceFrame(_ctx, surface.Handle, view);
    }

    public void Present(nint surface)
    {
        _swapChain.Present(surface);
    }

    public void Unconfigure(nint surface)
    {
        _swapChain.Unconfigure(surface);
    }

    public void Destroy(ref GraphicsSurface surface)
    {
        if (!surface.IsValid) return;
        _swapChain.Unconfigure(surface.Handle);
        _ctx.NativeApi.SurfaceRelease((Surface*)surface.Handle);
        surface = default;
    }
}
