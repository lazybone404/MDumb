using Silk.NET.WebGPU;

namespace Dumb.Engine.Mesh;

public enum MeshAttribute
{
    Position,
    Normal,
    Tangent,
    UV0,
    UV1,
    Color,
    BoneWeights,
    BoneIndices
}

public readonly struct VertexStreamDescriptor(
    MeshAttribute[] attributes,
    VertexStepMode stepMode = VertexStepMode.Vertex)
{
    public readonly MeshAttribute[] Attributes = attributes;
    public readonly VertexStepMode StepMode = stepMode;
}

public readonly struct MeshDescriptor(VertexStreamDescriptor[] streams, IndexFormat indexFormat = IndexFormat.Uint32)
{
    public readonly VertexStreamDescriptor[] Streams = streams;
    public readonly IndexFormat IndexFormat = indexFormat;

    public static VertexFormat GetVertexFormat(MeshAttribute a) => a switch
    {
        MeshAttribute.Position => VertexFormat.Float32x3,
        MeshAttribute.Normal => VertexFormat.Float32x3,
        MeshAttribute.Tangent => VertexFormat.Float32x4,
        MeshAttribute.UV0 => VertexFormat.Float32x2,
        MeshAttribute.UV1 => VertexFormat.Float32x2,
        MeshAttribute.Color => VertexFormat.Float32x4,
        MeshAttribute.BoneWeights => VertexFormat.Float32x4,
        MeshAttribute.BoneIndices => VertexFormat.Uint32x4,
        _ => VertexFormat.Float32x3
    };

    public static uint AttributeSize(MeshAttribute a) => a switch
    {
        MeshAttribute.Position => 12,
        MeshAttribute.Normal => 12,
        MeshAttribute.Tangent => 16,
        MeshAttribute.UV0 => 8,
        MeshAttribute.UV1 => 8,
        MeshAttribute.Color => 16,
        MeshAttribute.BoneWeights => 16,
        MeshAttribute.BoneIndices => 16,
        _ => 0
    };

    public static uint StreamStride(MeshAttribute[] attributes)
    {
        uint stride = 0;
        foreach (var a in attributes)
            stride += AttributeSize(a);
        return stride;
    }

}
