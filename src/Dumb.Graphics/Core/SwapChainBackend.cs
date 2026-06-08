using Silk.NET.WebGPU;
#if BROWSER
using Dawn = Silk.NET.WebGPU.Extensions.Dawn;
#endif
using Dumb.Graphics.Interfaces;

namespace Dumb.Graphics;

/// <summary>
/// 统一 SwapChain 后端 — Browser 使用 Dawn SwapChain API，Native 使用标准 Surface API。
/// 两者 API 完全不同，因此方法体通过 #if BROWSER 分别实现。
/// </summary>
public sealed unsafe class SwapChainBackend : ISwapChainBackend
{
#if BROWSER
    private readonly Dumb.Emscripten.WGPUBrowser _wgpu;
    private Dawn.SwapChain* _swapChain;

    public SwapChainBackend(Dumb.Emscripten.WGPUBrowser wgpu) => _wgpu = wgpu;
#else
    private readonly WebGPU _wgpu;
    private Texture* _currentTexture;

    public SwapChainBackend(WebGPU wgpu) => _wgpu = wgpu;
#endif

    public TextureFormat GetPreferredFormat(nint surface, nint adapter)
    {
        return _wgpu.SurfaceGetPreferredFormat((Surface*)surface, (Adapter*)adapter);
    }

    public void Configure(in GraphicsSurface surface, nint device,
        TextureUsage usage, PresentMode presentMode)
    {
#if BROWSER
        // Browser: Dawn swap chain API
        if (_swapChain != null)
            _wgpu.SwapChainRelease(_swapChain);

        var desc = new Dawn.SwapChainDescriptor
        {
            Usage = usage,
            Format = surface.Format,
            Width = surface.Width,
            Height = surface.Height,
            PresentMode = presentMode
        };
        _swapChain = _wgpu.DeviceCreateSwapChain((Device*)device, (Surface*)surface.Handle, desc);
#else
        // Native: Standard WebGPU SurfaceConfiguration
        var config = new SurfaceConfiguration
        {
            Device = (Device*)device,
            Format = surface.Format,
            Usage = usage,
            Width = surface.Width,
            Height = surface.Height,
            PresentMode = presentMode,
            AlphaMode = CompositeAlphaMode.Opaque,
            ViewFormatCount = 0,
            ViewFormats = null,
            NextInChain = null
        };
        _wgpu.SurfaceConfigure((Surface*)surface.Handle, &config);
#endif
    }

    public nint GetCurrentTextureView(in GraphicsSurface surface)
    {
#if BROWSER
        var view = _wgpu.SwapChainGetCurrentTextureView(_swapChain);
        return (nint)view;
#else
        if (_currentTexture != null)
        {
            _wgpu.TextureRelease(_currentTexture);
            _currentTexture = null;
        }

        SurfaceTexture surfaceTexture = default;
        _wgpu.SurfaceGetCurrentTexture((Surface*)surface.Handle, &surfaceTexture);

        if (surfaceTexture.Status != SurfaceGetCurrentTextureStatus.Success || surfaceTexture.Texture == null)
            return 0;

        _currentTexture = surfaceTexture.Texture;

        TextureViewDescriptor viewDesc = new()
        {
            Format = surface.Format,
            Dimension = TextureViewDimension.Dimension2D,
            BaseMipLevel = 0,
            MipLevelCount = 1,
            BaseArrayLayer = 0,
            ArrayLayerCount = 1,
            Aspect = TextureAspect.All,
            Label = null
        };

        var view = _wgpu.TextureCreateView(_currentTexture, &viewDesc);
        return (nint)view;
#endif
    }

    public void Present(nint surface)
    {
#if BROWSER
        // Browser: presentation handled by the browser
#else
        _wgpu.SurfacePresent((Surface*)surface);
        if (_currentTexture != null)
        {
            _wgpu.TextureRelease(_currentTexture);
            _currentTexture = null;
        }
#endif
    }

    public void Unconfigure(nint surface)
    {
#if BROWSER
        if (_swapChain == null) return;
        _wgpu.SwapChainRelease(_swapChain);
        _swapChain = null;
#else
        if (_currentTexture != null)
        {
            _wgpu.TextureRelease(_currentTexture);
            _currentTexture = null;
        }
        _wgpu.SurfaceUnconfigure((Surface*)surface);
#endif
    }
}
