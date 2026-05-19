using System.Numerics;

namespace Dumb.Engine.Mesh;

public static class MeshModify
{
    public static void Merge(this MeshData mesh, MeshData other)
    {
        var vertexOffset = mesh.VertexCount;

        // Merge vertex streams
        for (var s = 0; s < mesh.Streams.Length; s++)
        {
            var selfStride = (int)MeshDescriptor.StreamStride(mesh.Descriptor.Streams[s].Elements);
            var otherStride = s < other.Streams.Length
                ? (int)MeshDescriptor.StreamStride(other.Descriptor.Streams[s].Elements)
                : 0;

            if (selfStride != otherStride)
                throw new MeshMergeException(
                    $"Stream {s} stride mismatch: {selfStride} vs {otherStride}");

            var selfStream = mesh.Streams[s];
            var otherStream = other.Streams[s];
            var merged = new byte[selfStream.Length + otherStream.Length];
            Array.Copy(selfStream, merged, selfStream.Length);
            Array.Copy(otherStream, 0, merged, selfStream.Length, otherStream.Length);
            mesh.Streams[s] = merged;
        }

        // Merge indices
        if (mesh.Indices.Count > 0 || other.Indices.Count > 0)
        {
            var otherCount = other.Indices.Count;
            for (var i = 0; i < otherCount; i++)
                mesh.Indices.Push(other.Indices[i] + (uint)vertexOffset);
        }

    }

    public static void InvertWinding(this MeshData mesh)
    {
        var count = mesh.Indices.Count;
        if (count == 0)
            throw new MeshWindingException("Mesh has no indices");
        if (count % 3 != 0)
            throw new MeshWindingException($"Index count {count} is not a multiple of 3");

        for (var i = 0; i < count; i += 3)
        {
            var tmp = mesh.Indices[i + 1];
            mesh.Indices.SetAt(i + 1, mesh.Indices[i + 2]);
            mesh.Indices.SetAt(i + 2, tmp);
        }
    }

    public static void DuplicateVertices(this MeshData mesh)
    {
        if (mesh.Indices.Count == 0)
            return;

        var indexCount = mesh.Indices.Count;

        // For each stream, expand vertices according to indices
        for (var s = 0; s < mesh.Streams.Length; s++)
        {
            var stride = (int)MeshDescriptor.StreamStride(mesh.Descriptor.Streams[s].Elements);
            var oldStream = mesh.Streams[s];
            var newStream = new byte[indexCount * stride];

            for (var i = 0; i < indexCount; i++)
            {
                var srcIdx = (int)mesh.Indices[i];
                var srcOff = srcIdx * stride;
                var dstOff = i * stride;
                Array.Copy(oldStream, srcOff, newStream, dstOff, stride);
            }

            mesh.Streams[s] = newStream;
        }

        // Clear indices — mesh is now un-indexed
        mesh.Indices.Clear();
    }

    public static MeshData WithMerged(this MeshData mesh, MeshData other)
    {
        Merge(mesh, other);
        return mesh;
    }

    public static MeshData WithInvertedWinding(this MeshData mesh)
    {
        InvertWinding(mesh);
        return mesh;
    }

    public static MeshData WithDuplicatedVertices(this MeshData mesh)
    {
        DuplicateVertices(mesh);
        return mesh;
    }

    public static MeshData WithNormalizedJointWeights(this MeshData mesh)
    {
        NormalizeJointWeights(mesh);
        return mesh;
    }

    public static void NormalizeJointWeights(this MeshData mesh)
    {
        if (!MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.BoneWeights,
                out var si, out var stride, out var offset))
            return;
        var stream = mesh.Streams[si];
        var count = mesh.VertexCount;
        for (var i = 0; i < count; i++)
        {
            var off = i * stride + offset;
            var w = VertexStreamUtils.ReadFloat4(stream, off);
            var wx = Math.Max(0, w.X);
            var wy = Math.Max(0, w.Y);
            var wz = Math.Max(0, w.Z);
            var ww = Math.Max(0, w.W);
            var sum = wx + wy + wz + ww;
            if (sum < 1e-10f)
                wx = 1f;
            else
            {
                var recip = 1f / sum;
                wx *= recip;
                wy *= recip;
                wz *= recip;
                ww *= recip;
            }
            VertexStreamUtils.WriteFloat4(stream, off, new Vector4(wx, wy, wz, ww));
        }
    }
}
