using Silk.NET.WebGPU;

namespace Dumb.Graphics;

internal unsafe interface ISwapChainBackend
{
    TextureFormat GetPreferredFormat(nint surface, nint adapter);

    void Configure(nint surface, nint device, uint width, uint height,
        TextureFormat format, TextureUsage usage, PresentMode presentMode);

    nint GetCurrentTextureView(nint surface, TextureFormat format);

    void Present(nint surface);

    void Unconfigure(nint surface);
}
