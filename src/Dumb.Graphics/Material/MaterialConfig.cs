using Silk.NET.WebGPU;

namespace Dumb.Graphics;

/// <summary>
/// 材质配置 — 将 IMaterial 上散落的多个 static abstract 成员
/// (BindGroupLayouts / ColorFormats / Blend / DepthStencil / VertexDescriptor) 合并为一个属性。
/// </summary>
public sealed record MaterialConfig
{
    public required Engine.Mesh.MeshDescriptor VertexDescriptor { get; init; }
    public required BindingLayout[][] BindGroupLayouts { get; init; }
    public TextureFormat[] ColorFormats { get; init; } = [TextureFormat.Rgba8Unorm];
    public BlendState? Blend { get; init; }
    public DepthStencilState? DepthStencil { get; init; }
}
