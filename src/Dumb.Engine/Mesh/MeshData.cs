using System.Numerics;
using Silk.NET.WebGPU;

namespace Dumb.Engine.Mesh;

public sealed class MeshData(MeshDescriptor descriptor, byte[][] streams, byte[]? indices)
{
    public byte[][] Streams { get; } = streams;
    public byte[]? Indices { get; } = indices;
    public IndexFormat IndexFormat { get; } = descriptor.IndexFormat;
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

    public int IndexCount
    {
        get
        {
            if (Indices == null) return 0;
            var isize = IndexFormat == IndexFormat.Uint16 ? 2 : 4;
            return Indices.Length / isize;
        }
    }

    public static MeshData FromVertices(IReadOnlyList<MeshVertex> vertices, IReadOnlyList<uint> indices)
    {
        VertexElement[] elements = [MeshAttribute.Position, MeshAttribute.Normal, MeshAttribute.Color];
        var stride = (int)MeshDescriptor.StreamStride(elements);
        var stream = new byte[vertices.Count * stride];

        for (var i = 0; i < vertices.Count; i++)
        {
            var v = vertices[i];
            var off = i * stride;
            WriteFloat3(stream, off, v.Position);
            WriteFloat3(stream, off + 12, v.Normal);
            WriteFloat4(stream, off + 24, new Vector4(v.Color, 1.0f));
        }

        byte[]? idxBytes = null;
        if (indices.Count > 0)
        {
            idxBytes = new byte[indices.Count * 4];
            System.Buffer.BlockCopy(
                indices is uint[] arr ? arr : [.. indices],
                0, idxBytes, 0, idxBytes.Length);
        }

        var desc = new MeshDescriptor(
            [new VertexStreamDescriptor(elements)],
            IndexFormat.Uint32);

        return new MeshData(desc, [stream], idxBytes);
    }

    public static MeshData CreateQuad()
    {
        VertexElement[] elements = [MeshAttribute.Position, MeshAttribute.Normal, MeshAttribute.UV0];
        var stride = (int)MeshDescriptor.StreamStride(elements);
        var stream = new byte[4 * stride];

        WriteVertex(stream, 0, stride,
            new Vector3(-0.5f, -0.5f, 0), Vector3.UnitZ, new Vector2(0, 0));
        WriteVertex(stream, 1, stride,
            new Vector3(0.5f, -0.5f, 0), Vector3.UnitZ, new Vector2(1, 0));
        WriteVertex(stream, 2, stride,
            new Vector3(0.5f, 0.5f, 0), Vector3.UnitZ, new Vector2(1, 1));
        WriteVertex(stream, 3, stride,
            new Vector3(-0.5f, 0.5f, 0), Vector3.UnitZ, new Vector2(0, 1));

        var idx = new byte[6 * 4];
        uint[] idxArr = [0, 1, 2, 0, 2, 3];
        System.Buffer.BlockCopy(idxArr, 0, idx, 0, idx.Length);

        var desc = new MeshDescriptor([new VertexStreamDescriptor(elements)]);

        return new MeshData(desc, [stream], idx);
    }

    public static MeshData FromSkinnedVertices(IReadOnlyList<SkinnedMeshVertex> vertices, IReadOnlyList<uint> indices)
    {
        VertexElement[] stream0Elements = [MeshAttribute.Position, MeshAttribute.Normal, MeshAttribute.UV0, MeshAttribute.Tangent];
        VertexElement[] stream1Elements = [MeshAttribute.BoneWeights, MeshAttribute.BoneIndices];

        var stride0 = (int)MeshDescriptor.StreamStride(stream0Elements);
        var stride1 = (int)MeshDescriptor.StreamStride(stream1Elements);

        var stream0 = new byte[vertices.Count * stride0];
        var stream1 = new byte[vertices.Count * stride1];

        for (var i = 0; i < vertices.Count; i++)
        {
            var v = vertices[i];
            var off0 = i * stride0;
            WriteFloat3(stream0, off0, v.Position);
            WriteFloat3(stream0, off0 + 12, v.Normal);
            WriteFloat2(stream0, off0 + 24, v.UV);
            WriteFloat4(stream0, off0 + 32, v.Tangent);

            var off1 = i * stride1;
            WriteFloat4(stream1, off1, v.BoneWeights);
            WriteUint4(stream1, off1 + 16, v.BoneIndex0, v.BoneIndex1, v.BoneIndex2, v.BoneIndex3);
        }

        byte[]? idxBytes = null;
        if (indices.Count > 0)
        {
            idxBytes = new byte[indices.Count * 4];
            System.Buffer.BlockCopy(
                indices is uint[] arr ? arr : [.. indices],
                0, idxBytes, 0, idxBytes.Length);
        }

        var desc = new MeshDescriptor(
            [new VertexStreamDescriptor(stream0Elements), new VertexStreamDescriptor(stream1Elements)],
            IndexFormat.Uint32);

        return new MeshData(desc, [stream0, stream1], idxBytes);
    }

    private static void WriteFloat3(byte[] buf, int offset, Vector3 v)
    {
        BitConverter.GetBytes(v.X).CopyTo(buf, offset);
        BitConverter.GetBytes(v.Y).CopyTo(buf, offset + 4);
        BitConverter.GetBytes(v.Z).CopyTo(buf, offset + 8);
    }

    private static void WriteFloat2(byte[] buf, int offset, Vector2 v)
    {
        BitConverter.GetBytes(v.X).CopyTo(buf, offset);
        BitConverter.GetBytes(v.Y).CopyTo(buf, offset + 4);
    }

    private static void WriteFloat4(byte[] buf, int offset, Vector4 v)
    {
        BitConverter.GetBytes(v.X).CopyTo(buf, offset);
        BitConverter.GetBytes(v.Y).CopyTo(buf, offset + 4);
        BitConverter.GetBytes(v.Z).CopyTo(buf, offset + 8);
        BitConverter.GetBytes(v.W).CopyTo(buf, offset + 12);
    }

    private static void WriteUint4(byte[] buf, int offset, uint i0, uint i1, uint i2, uint i3)
    {
        BitConverter.GetBytes(i0).CopyTo(buf, offset);
        BitConverter.GetBytes(i1).CopyTo(buf, offset + 4);
        BitConverter.GetBytes(i2).CopyTo(buf, offset + 8);
        BitConverter.GetBytes(i3).CopyTo(buf, offset + 12);
    }

    private static void WriteVertex(byte[] stream, int vi, int stride,
        Vector3 pos, Vector3 nrm, Vector2 uv)
    {
        var off = vi * stride;
        WriteFloat3(stream, off, pos);
        WriteFloat3(stream, off + 12, nrm);
        BitConverter.GetBytes(uv.X).CopyTo(stream, off + 24);
        BitConverter.GetBytes(uv.Y).CopyTo(stream, off + 28);
    }
}
