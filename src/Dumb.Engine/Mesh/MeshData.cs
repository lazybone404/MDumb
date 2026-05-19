using System.Numerics;
using Silk.NET.WebGPU;

namespace Dumb.Engine.Mesh;

public class MeshData(MeshDescriptor descriptor, byte[][] streams, Indices indices)
{
    public byte[][] Streams { get; } = streams;
    public Indices Indices { get; } = indices;
    public IndexFormat IndexFormat => Indices.Format;
    public SubMesh[] SubMeshes { get; set; } = [];
    public MeshDescriptor Descriptor { get; } = descriptor;

    public int VertexCount
    {
        get
        {
            if (Streams.Length == 0) return 0;
            var stride = (int)MeshDescriptor.StreamStride(Descriptor.Streams[0].Elements);
            return stride > 0 ? Streams[0].Length / stride : 0;
        }
    }

    public int IndexCount => Indices.Count;

    public MeshAabb ComputeAabb()
    {
        if (Streams.Length == 0 || Descriptor.Streams.Length == 0)
            return new MeshAabb(Vector3.Zero, Vector3.Zero);

        if (!MeshDescriptor.TryFindAttribute(Descriptor.Streams, MeshAttribute.Position,
                out var si, out var stride, out var offset))
            return new MeshAabb(Vector3.Zero, Vector3.Zero);

        var stream = Streams[si];
        var count = stream.Length / stride;
        if (count == 0)
            return new MeshAabb(Vector3.Zero, Vector3.Zero);

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        for (var i = 0; i < count; i++)
        {
            var off = i * stride + offset;
            var p = VertexStreamUtils.ReadFloat3(stream, off);
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        return new MeshAabb(min, max);
    }

    public bool TryValidate(out string? error)
    {
        for (var s = 0; s < Streams.Length; s++)
        {
            var stride = (int)MeshDescriptor.StreamStride(Descriptor.Streams[s].Elements);
            if (stride == 0)
            {
                error = $"Stream {s} has zero stride";
                return false;
            }
            if (Streams[s].Length % stride != 0)
            {
                error = $"Stream {s} length {Streams[s].Length} is not a multiple of stride {stride}";
                return false;
            }
        }

        if (Streams.Length > 0)
        {
            var baseCount = Streams[0].Length / (int)MeshDescriptor.StreamStride(Descriptor.Streams[0].Elements);
            for (var s = 1; s < Streams.Length; s++)
            {
                if (Descriptor.Streams[s].StepMode == VertexStepMode.Instance)
                    continue;
                var stride = (int)MeshDescriptor.StreamStride(Descriptor.Streams[s].Elements);
                var count = stride > 0 ? Streams[s].Length / stride : 0;
                if (count != baseCount)
                {
                    error = $"Stream {s} vertex count {count} != stream 0 count {baseCount}";
                    return false;
                }
            }
        }

        if (!Indices.IsEmpty)
        {
            var span = Indices.GetSpan();
            var indexStride = Indices.Format == IndexFormat.Uint32 ? 4 : 2;
            if (span.Length % indexStride != 0)
            {
                error = $"Index data length {span.Length} is not a multiple of index stride {indexStride}";
                return false;
            }
        }

        error = null;
        return true;
    }

    public static MeshData FromVertices(IReadOnlyList<MeshVertex> vertices, IReadOnlyList<uint> indexData)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(indexData);

        VertexElement[] elements =
        [
            MeshAttribute.Position, MeshAttribute.Normal, MeshAttribute.UV0,
            MeshAttribute.Tangent, MeshAttribute.Color
        ];
        var stride = (int)MeshDescriptor.StreamStride(elements);
        var stream = new byte[vertices.Count * stride];

        for (var i = 0; i < vertices.Count; i++)
        {
            var v = vertices[i];
            var off = i * stride;
            VertexStreamUtils.WriteFloat3(stream, off, v.Position);
            VertexStreamUtils.WriteFloat3(stream, off + 12, v.Normal);
            VertexStreamUtils.WriteFloat2(stream, off + 24, v.UV);
            VertexStreamUtils.WriteFloat4(stream, off + 32, v.Tangent);
            VertexStreamUtils.WriteFloat4(stream, off + 48, new Vector4(v.Color, 1.0f));
        }

        var indices = indexData.Count > 0
            ? new Indices(IndexFormat.Uint32, indexData.Count)
            : new Indices();
        if (indexData.Count > 0)
            indices.Extend(indexData);

        var desc = new MeshDescriptor(
            [new VertexStreamDescriptor(elements)],
            indices.Format);

        return new MeshData(desc, [stream], indices);
    }

    public static MeshData CreateQuad()
    {
        VertexElement[] elements =
        [
            MeshAttribute.Position, MeshAttribute.Normal, MeshAttribute.UV0,
            MeshAttribute.Tangent
        ];
        var stride = (int)MeshDescriptor.StreamStride(elements);
        var stream = new byte[4 * stride];

        var tangent = new Vector4(1, 0, 0, 1);
        WriteVertex(stream, 0, stride, new Vector3(-0.5f, -0.5f, 0), Vector3.UnitZ, new Vector2(0, 0), tangent);
        WriteVertex(stream, 1, stride, new Vector3(0.5f, -0.5f, 0), Vector3.UnitZ, new Vector2(1, 0), tangent);
        WriteVertex(stream, 2, stride, new Vector3(0.5f, 0.5f, 0), Vector3.UnitZ, new Vector2(1, 1), tangent);
        WriteVertex(stream, 3, stride, new Vector3(-0.5f, 0.5f, 0), Vector3.UnitZ, new Vector2(0, 1), tangent);

        var indices = new Indices(IndexFormat.Uint16, 6);
        indices.Extend([0u, 1, 2, 0, 2, 3]);

        var desc = new MeshDescriptor([new VertexStreamDescriptor(elements)]);

        return new MeshData(desc, [stream], indices);
    }

    public static MeshData FromSkinnedVertices(IReadOnlyList<SkinnedMeshVertex> vertices, IReadOnlyList<uint> indexData)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(indexData);

        VertexElement[] stream0Elements =
        [
            MeshAttribute.Position, MeshAttribute.Normal, MeshAttribute.UV0, MeshAttribute.Tangent
        ];
        VertexElement[] stream1Elements =
        [
            MeshAttribute.BoneWeights, MeshAttribute.BoneIndices
        ];

        var stride0 = (int)MeshDescriptor.StreamStride(stream0Elements);
        var stride1 = (int)MeshDescriptor.StreamStride(stream1Elements);

        var stream0 = new byte[vertices.Count * stride0];
        var stream1 = new byte[vertices.Count * stride1];

        for (var i = 0; i < vertices.Count; i++)
        {
            var v = vertices[i];
            var off0 = i * stride0;
            VertexStreamUtils.WriteFloat3(stream0, off0, v.Position);
            VertexStreamUtils.WriteFloat3(stream0, off0 + 12, v.Normal);
            VertexStreamUtils.WriteFloat2(stream0, off0 + 24, v.UV);
            VertexStreamUtils.WriteFloat4(stream0, off0 + 32, v.Tangent);

            var off1 = i * stride1;
            VertexStreamUtils.WriteFloat4(stream1, off1, v.BoneWeights);
            VertexStreamUtils.WriteUshort4(stream1, off1 + 16, v.BoneIndex0, v.BoneIndex1, v.BoneIndex2, v.BoneIndex3);
        }

        var indices = indexData.Count > 0
            ? new Indices(IndexFormat.Uint32, indexData.Count)
            : new Indices();
        if (indexData.Count > 0)
            indices.Extend(indexData);

        var desc = new MeshDescriptor(
            [new VertexStreamDescriptor(stream0Elements), new VertexStreamDescriptor(stream1Elements)],
            indices.Format);

        return new MeshData(desc, [stream0, stream1], indices);
    }

    private static void WriteVertex(byte[] stream, int vi, int stride,
        Vector3 pos, Vector3 nrm, Vector2 uv, Vector4 tangent)
    {
        var off = vi * stride;
        VertexStreamUtils.WriteFloat3(stream, off, pos);
        VertexStreamUtils.WriteFloat3(stream, off + 12, nrm);
        VertexStreamUtils.WriteFloat2(stream, off + 24, uv);
        VertexStreamUtils.WriteFloat4(stream, off + 32, tangent);
    }
}
