namespace Dumb.Emscripten.WGPUGenerator;

public sealed class WgpuGenerationOptions(string ns = "Dumb.Emscripten.WGPU")
{
    public string Namespace { get; } = ns;
}
