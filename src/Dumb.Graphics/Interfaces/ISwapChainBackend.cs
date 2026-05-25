using Silk.NET.WebGPU;

namespace Dumb.Graphics.Interfaces;

public unsafe interface ISwapChainBackend
{
    TextureFormat GetPreferredFormat(nint surface, nint adapter);

    void Configure(in GraphicsSurface surface, nint device,
        TextureUsage usage, PresentMode presentMode);

    nint GetCurrentTextureView(in GraphicsSurface surface);

    void Present(nint surface);

    void Unconfigure(nint surface);
}
