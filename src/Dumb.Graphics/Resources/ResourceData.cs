using Silk.NET.WebGPU;
using Sia;

namespace Dumb.Graphics;

/// <summary>
/// 标记接口 — 实现此接口的 struct 必须包含 public nint NativePtr 和 public int RefCount 字段。
/// 用于 GpuResourceManager&lt;TData&gt; 的泛型约束。
/// </summary>
public interface IGpuResource { }

public struct BufferData : IGpuResource
{
    public nint NativePtr;
    public ulong Size;
    public BufferUsage Usage;
    public int RefCount;
}

public struct TextureData : IGpuResource
{
    public nint NativePtr;
    public Extent3D Size;
    public TextureFormat Format;
    public TextureUsage Usage;
    public uint MipLevelCount;
    public uint SampleCount;
    public int RefCount;
}

public struct TextureViewData : IGpuResource
{
    public nint NativePtr;
    public Entity Texture;
    public int RefCount;
}

public struct SamplerData : IGpuResource
{
    public nint NativePtr;
    public int RefCount;
}

public struct ShaderData : IGpuResource
{
    public nint NativePtr;
    public int RefCount;
}

public struct BindGroupLayoutData : IGpuResource
{
    public nint NativePtr;
    public int RefCount;
}

public struct BindGroupData : IGpuResource
{
    public nint NativePtr;
    public Entity Layout;
    public int RefCount;
}

public struct PipelineLayoutData : IGpuResource
{
    public nint NativePtr;
    public uint BindGroupLayoutCount;
    public Entity[]? BindGroupLayouts;
    public int RefCount;
}

public struct RenderPipelineData : IGpuResource
{
    public nint NativePtr;
    public Entity VertexShader;
    public Entity FragmentShader;
    public Entity Layout;
    public int RefCount;
}

public struct ComputePipelineData : IGpuResource
{
    public nint NativePtr;
    public Entity ComputeShader;
    public Entity Layout;
    public int RefCount;
}

public struct MaterialResourceData
{
    public Entity Pipeline;
    public Entity PipelineLayout;
    public Entity?[] BindGroups;
    public int RefCount;
}

public struct MeshResourceData
{
    public Entity[] VertexBuffers;
    public int[] VertexCounts;
    public Entity IndexBuffer;
    public IndexFormat IndexFormat;
    public uint IndexCount;
    public Engine.Mesh.SubMesh[] SubMeshes;
    public int RefCount;
}
