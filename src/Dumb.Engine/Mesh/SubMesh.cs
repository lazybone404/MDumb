using Silk.NET.WebGPU;

namespace Dumb.Engine.Mesh;

public struct SubMesh
{
    public uint IndexStart;
    public uint IndexCount;
    public uint VertexStart;
    public uint VertexCount;
    public PrimitiveTopology Topology;
}
