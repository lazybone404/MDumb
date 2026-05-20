using Silk.NET.WebGPU;

namespace Dumb.Engine.Mesh;

public readonly record struct MeshAttribute
{
    public int Id { get; }
    public VertexFormat Format { get; }
    public uint Size { get; }
    public uint DefaultLocation { get; }

    // Built-in attribute IDs
    internal const int IdPosition = 0;
    internal const int IdNormal = 1;
    internal const int IdUV0 = 2;
    internal const int IdTangent = 3;
    internal const int IdColor = 4;
    internal const int IdBoneWeights = 5;
    internal const int IdBoneIndices = 6;

    public static readonly MeshAttribute Position = new(IdPosition, VertexFormat.Float32x3, 12);
    public static readonly MeshAttribute Normal = new(IdNormal, VertexFormat.Float32x3, 12);
    public static readonly MeshAttribute UV0 = new(IdUV0, VertexFormat.Float32x2, 8);
    public static readonly MeshAttribute Tangent = new(IdTangent, VertexFormat.Float32x4, 16);
    public static readonly MeshAttribute Color = new(IdColor, VertexFormat.Float32x4, 16);
    public static readonly MeshAttribute BoneWeights = new(IdBoneWeights, VertexFormat.Float32x4, 16);
    public static readonly MeshAttribute BoneIndices = new(IdBoneIndices, VertexFormat.Uint16x4, 8);

    private static int s_nextId = 1000;

    private MeshAttribute(int id, VertexFormat format, uint size)
    {
        Id = id;
        Format = format;
        Size = size;
        DefaultLocation = (uint)id;
    }

    public MeshAttribute(VertexFormat format, uint size)
    {
        Id = Interlocked.Increment(ref s_nextId) - 1;
        Format = format;
        Size = size;
        DefaultLocation = (uint)Id;
    }
}

public readonly record struct VertexElement
{
    public MeshAttribute Attribute { get; init; }
    public uint Location { get; init; }

    public VertexElement(MeshAttribute attribute, uint? location = null)
    {
        Attribute = attribute;
        Location = location ?? attribute.DefaultLocation;
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
    public static uint StreamStride(VertexElement[] elements)
    {
        uint stride = 0;
        foreach (var e in elements)
            stride += e.Attribute.Size;
        return stride;
    }

    public static bool TryFindAttribute(
        VertexStreamDescriptor[] streams,
        MeshAttribute attribute,
        out int streamIndex,
        out int streamStride,
        out int elementOffset)
    {
        for (var si = 0; si < streams.Length; si++)
        {
            var stride = (int)StreamStride(streams[si].Elements);
            var offset = 0;
            foreach (var elem in streams[si].Elements)
            {
                if (elem.Attribute == attribute)
                {
                    streamIndex = si;
                    streamStride = stride;
                    elementOffset = offset;
                    return true;
                }
                offset += (int)elem.Attribute.Size;
            }
        }
        streamIndex = -1;
        streamStride = 0;
        elementOffset = 0;
        return false;
    }

    public static (int streamIndex, VertexElement element)[] GetSortedElements(
        VertexStreamDescriptor[] streams)
    {
        var elements = new (int, VertexElement)[streams.Sum(s => s.Elements.Length)];
        var idx = 0;
        for (var si = 0; si < streams.Length; si++)
            foreach (var e in streams[si].Elements)
                elements[idx++] = (si, e);
        Array.Sort(elements, (a, b) => a.Item2.Attribute.Id.CompareTo(b.Item2.Attribute.Id));
        return elements;
    }
}
