using Silk.NET.WebGPU;

namespace Dumb.Engine.Mesh;

public readonly record struct SubMesh(
    uint IndexStart,
    uint IndexCount,
    uint VertexStart,
    uint VertexCount,
    PrimitiveTopology Topology);
