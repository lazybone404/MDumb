using Silk.NET.WebGPU;
using Sia;

namespace Dumb.Graphics;

// GPU resource structs stored as single-component Sia entities.
// Entity references use Sia's ObjectPool<Entity>; destroyed entities return to pool.
// RefCount managed via Interlocked.Increment/Decrement.

public struct BufferData
{
    public nint NativePtr;
    public ulong Size;
    public BufferUsage Usage;
    public int RefCount;
}

public struct TextureData
{
    public nint NativePtr;
    public Extent3D Size;
    public TextureFormat Format;
    public TextureUsage Usage;
    public uint MipLevelCount;
    public uint SampleCount;
    public int RefCount;
}

public struct TextureViewData
{
    public nint NativePtr;
    public Entity Texture;
    public int RefCount;
}

public struct SamplerData
{
    public nint NativePtr;
    public int RefCount;
}

public struct ShaderData
{
    public nint NativePtr;
    public int RefCount;
}

public struct BindGroupLayoutData
{
    public nint NativePtr;
    public int RefCount;
}

public struct BindGroupData
{
    public nint NativePtr;
    public Entity Layout;
    public int RefCount;
}

public struct PipelineLayoutData
{
    public nint NativePtr;
    public uint BindGroupLayoutCount;
    public Entity[]? BindGroupLayouts;
    public int RefCount;
}

public struct RenderPipelineData
{
    public nint NativePtr;
    public Entity VertexShader;
    public Entity FragmentShader;
    public Entity Layout;
    public int RefCount;
}

public struct ComputePipelineData
{
    public nint NativePtr;
    public Entity ComputeShader;
    public Entity Layout;
    public int RefCount;
}
