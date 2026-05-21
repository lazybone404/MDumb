using Silk.NET.WebGPU;

namespace Dumb.Graphics.Native;

public sealed unsafe class NativeSwapChainBackend : ISwapChainBackend
{
    private readonly WebGPU _wgpu;
    private Texture* _currentTexture;

    public NativeSwapChainBackend(WebGPU wgpu) => _wgpu = wgpu;

    public TextureFormat GetPreferredFormat(nint surface, nint adapter)
    {
        return _wgpu.SurfaceGetPreferredFormat((Surface*)surface, (Adapter*)adapter);
    }

    public void Configure(in GraphicsSurface surface, nint device,
        TextureUsage usage, PresentMode presentMode)
    {
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
    }

    public nint GetCurrentTextureView(in GraphicsSurface surface)
    {
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
