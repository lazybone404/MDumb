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
            var stride = (int)MeshDescriptor.StreamStride(Descriptor.Streams[0].Attributes);
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
        var attrs = new[] { MeshAttribute.Position, MeshAttribute.Normal, MeshAttribute.Color };
        var stride = (int)MeshDescriptor.StreamStride(attrs);
        var stream = new byte[vertices.Count * stride];

        for (var i = 0; i < vertices.Count; i++)
        {
            var v = vertices[i];
            var off = i * stride;
            WriteFloat3(stream, off, v.Position);
            WriteFloat3(stream, off + 12, v.Normal);
            WriteFloat4(stream, off + 24, v.Color);
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
            [new VertexStreamDescriptor(attrs)],
            IndexFormat.Uint32);

        return new MeshData(desc, [stream], idxBytes);
    }

    public static MeshData CreateQuad()
    {
        var attrs = new[] { MeshAttribute.Position, MeshAttribute.Normal, MeshAttribute.UV0 };
        var stride = (int)MeshDescriptor.StreamStride(attrs);
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

        var desc = new MeshDescriptor([new VertexStreamDescriptor(attrs)]);

        return new MeshData(desc, [stream], idx);
    }

    private static void WriteFloat3(byte[] buf, int offset, Vector3 v)
    {
        BitConverter.GetBytes(v.X).CopyTo(buf, offset);
        BitConverter.GetBytes(v.Y).CopyTo(buf, offset + 4);
        BitConverter.GetBytes(v.Z).CopyTo(buf, offset + 8);
    }

    private static void WriteFloat4(byte[] buf, int offset, Vector3 v)
    {
        BitConverter.GetBytes(v.X).CopyTo(buf, offset);
        BitConverter.GetBytes(v.Y).CopyTo(buf, offset + 4);
        BitConverter.GetBytes(v.Z).CopyTo(buf, offset + 8);
        BitConverter.GetBytes(1.0f).CopyTo(buf, offset + 12);
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
