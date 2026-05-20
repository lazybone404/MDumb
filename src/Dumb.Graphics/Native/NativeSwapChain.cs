using Silk.NET.WebGPU;

namespace Dumb.Graphics.Native;

internal sealed unsafe class NativeSwapChainBackend : ISwapChainBackend
{
    private readonly WebGPU _wgpu;
    private Texture* _currentTexture;

    public NativeSwapChainBackend(WebGPU wgpu) => _wgpu = wgpu;

    public TextureFormat GetPreferredFormat(nint surface, nint adapter)
    {
        return _wgpu.SurfaceGetPreferredFormat((Surface*)surface, (Adapter*)adapter);
    }

    public void Configure(nint surface, nint device, uint width, uint height,
        TextureFormat format, TextureUsage usage, PresentMode presentMode)
    {
        var config = new SurfaceConfiguration
        {
            Device = (Device*)device,
            Format = format,
            Usage = usage,
            Width = width,
            Height = height,
            PresentMode = presentMode,
            AlphaMode = CompositeAlphaMode.Opaque,
            ViewFormatCount = 0,
            ViewFormats = null,
            NextInChain = null
        };
        _wgpu.SurfaceConfigure((Surface*)surface, &config);
    }

    public nint GetCurrentTextureView(nint surface, TextureFormat format)
    {
        if (_currentTexture != null)
        {
            _wgpu.TextureRelease(_currentTexture);
            _currentTexture = null;
        }

        SurfaceTexture surfaceTexture = default;
        _wgpu.SurfaceGetCurrentTexture((Surface*)surface, &surfaceTexture);

        if (surfaceTexture.Status != SurfaceGetCurrentTextureStatus.Success || surfaceTexture.Texture == null)
            return 0;

        _currentTexture = surfaceTexture.Texture;

        TextureViewDescriptor viewDesc = new()
        {
            Format = format,
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
    }

    public void Present(nint surface)
    {
        _wgpu.SurfacePresent((Surface*)surface);
        if (_currentTexture != null)
        {
            _wgpu.TextureRelease(_currentTexture);
            _currentTexture = null;
        }
    }

    public void Unconfigure(nint surface)
    {
        if (_currentTexture != null)
        {
            _wgpu.TextureRelease(_currentTexture);
            _currentTexture = null;
        }
        _wgpu.SurfaceUnconfigure((Surface*)surface);
    }
}
