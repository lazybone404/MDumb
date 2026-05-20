using Silk.NET.WebGPU;
using Dawn = Silk.NET.WebGPU.Extensions.Dawn;

namespace Dumb.Graphics.Browser;

internal sealed unsafe class BrowserSwapChainBackend : ISwapChainBackend
{
    private readonly Dumb.Emscripten.WGPUBrowser _wgpu;
    private Dawn.SwapChain* _swapChain;

    public BrowserSwapChainBackend(Dumb.Emscripten.WGPUBrowser wgpu) => _wgpu = wgpu;

    public TextureFormat GetPreferredFormat(nint surface, nint adapter)
    {
        return _wgpu.SurfaceGetPreferredFormat((Surface*)surface, (Adapter*)adapter);
    }

    public void Configure(nint surface, nint device, uint width, uint height,
        TextureFormat format, TextureUsage usage, PresentMode presentMode)
    {
        if (_swapChain != null)
            _wgpu.SwapChainRelease(_swapChain);

        var desc = new Dawn.SwapChainDescriptor
        {
            Usage = usage,
            Format = format,
            Width = width,
            Height = height,
            PresentMode = presentMode
        };
        _swapChain = _wgpu.DeviceCreateSwapChain((Device*)device, (Surface*)surface, desc);
    }

    public nint GetCurrentTextureView(nint surface, TextureFormat format)
    {
        var view = _wgpu.SwapChainGetCurrentTextureView(_swapChain);
        return (nint)view;
    }

    public void Present(nint surface) { }

    public void Unconfigure(nint surface)
    {
        if (_swapChain == null) return;
        _wgpu.SwapChainRelease(_swapChain);
        _swapChain = null;
    }
}
