using Silk.NET.WebGPU;

namespace Dumb.Graphics;

// All resource structs store nint NativePtr, int RefCount, and ulong graph edges.
// ulong = Handle<T>.Value; reconstruct with new Handle<T>(ulongValue).
// RefCount managed via Interlocked.Increment/Decrement.
// All structs are unmanaged so they can live in Storage<T>.

public unsafe struct BufferData
{
    public nint NativePtr;
    public ulong Size;
    public BufferUsage Usage;
    public int RefCount;
}

public unsafe struct TextureData
{
    public nint NativePtr;
    public Extent3D Size;
    public TextureFormat Format;
    public TextureUsage Usage;
    public uint MipLevelCount;
    public uint SampleCount;
    public int RefCount;
}

public unsafe struct TextureViewData
{
    public nint NativePtr;
    public ulong TextureHandle; // Handle<TextureData>.Value
    public int RefCount;
}

public unsafe struct SamplerData
{
    public nint NativePtr;
    public int RefCount;
}

public unsafe struct ShaderData
{
    public nint NativePtr;
    public int RefCount;
}

public unsafe struct BindGroupLayoutData
{
    public nint NativePtr;
    public int RefCount;
}

public unsafe struct BindGroupData
{
    public nint NativePtr;
    public ulong LayoutHandle; // Handle<BindGroupLayoutData>.Value
    public int RefCount;
}

public unsafe struct PipelineLayoutData
{
    public nint NativePtr;
    public uint BindGroupLayoutCount;
    public ulong* BindGroupLayoutHandles; // malloc'd array of Handle<BindGroupLayoutData>.Value
    public int RefCount;
}

public unsafe struct RenderPipelineData
{
    public nint NativePtr;
    public ulong VertexShaderHandle;   // Handle<ShaderData>.Value
    public ulong FragmentShaderHandle; // Handle<ShaderData>.Value
    public ulong LayoutHandle;         // Handle<PipelineLayoutData>.Value
    public int RefCount;
}

public unsafe struct ComputePipelineData
{
    public nint NativePtr;
    public ulong ComputeShaderHandle; // Handle<ShaderData>.Value
    public ulong LayoutHandle;        // Handle<PipelineLayoutData>.Value
    public int RefCount;
}
