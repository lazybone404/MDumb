using Silk.NET.WebGPU;

namespace Dumb.Engine.Mesh;

public enum MeshAttribute
{
    Position,
    Normal,
    UV0,
    UV1,
    Tangent,
    Color,
    BoneWeights,
    BoneIndices
}

public readonly record struct VertexElement
{
    public MeshAttribute Attribute { get; init; }
    public uint Location { get; init; }

    public VertexElement(MeshAttribute attribute, uint? location = null)
    {
        Attribute = attribute;
        Location = location ?? (uint)attribute;
    }

    public static implicit operator VertexElement(MeshAttribute attribute) => new(attribute);
}

public readonly record struct VertexStreamDescriptor(
    VertexElement[] Elements,
    VertexStepMode StepMode = VertexStepMode.Vertex);

public readonly record struct MeshDescriptor(
    VertexStreamDescriptor[] Streams,
    IndexFormat IndexFormat = IndexFormat.Uint32)
{
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
        _ => throw new ArgumentOutOfRangeException(nameof(a), a, "Unknown MeshAttribute")
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
        _ => throw new ArgumentOutOfRangeException(nameof(a), a, "Unknown MeshAttribute")
    };

    public static uint StreamStride(VertexElement[] elements)
    {
        uint stride = 0;
        foreach (var e in elements)
            stride += AttributeSize(e.Attribute);
        return stride;
    }
}
