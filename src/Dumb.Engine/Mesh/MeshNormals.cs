using System.Numerics;

namespace Dumb.Engine.Mesh;

public static class MeshNormals
{
    public static void ComputeNormals(this MeshData mesh)
    {
        if (mesh.Indices.Count == 0)
            ComputeFlatNormals(mesh);
        else
            ComputeSmoothNormals(mesh);
    }

    public static void ComputeFlatNormals(this MeshData mesh)
    {
        var positions = GetPositions(mesh);
        if (positions.Length < 3 || positions.Length % 3 != 0)
            return;

        var normals = new Vector3[positions.Length];
        for (var i = 0; i < positions.Length; i += 3)
        {
            var n = TriangleNormal(positions[i], positions[i + 1], positions[i + 2]);
            normals[i] = n;
            normals[i + 1] = n;
            normals[i + 2] = n;
        }

        WriteNormals(mesh, normals);
    }

    public static void ComputeSmoothNormals(this MeshData mesh)
    {
        ComputeCustomSmoothNormals(mesh, (i0, i1, i2, positions, normals) =>
        {
            var pa = positions[i0];
            var pb = positions[i1];
            var pc = positions[i2];

            var ab = pb - pa;
            var ba = -ab;
            var bc = pc - pb;
            var cb = -bc;
            var ca = pa - pc;
            var ac = -ca;

            var weightA = ab.LengthSquared() * ac.LengthSquared() > 1e-12f
                ? AngleBetween(ab, ac) : 0f;
            var weightB = ba.LengthSquared() * bc.LengthSquared() > 1e-12f
                ? AngleBetween(ba, bc) : 0f;
            var weightC = ca.LengthSquared() * cb.LengthSquared() > 1e-12f
                ? AngleBetween(ca, cb) : 0f;

            var n = TriangleNormal(pa, pb, pc);
            normals[i0] += n * weightA;
            normals[i1] += n * weightB;
            normals[i2] += n * weightC;
        });
    }

    public static void ComputeAreaWeightedNormals(this MeshData mesh)
    {
        ComputeCustomSmoothNormals(mesh, (i0, i1, i2, positions, normals) =>
        {
            var pa = positions[i0];
            var pb = positions[i1];
            var pc = positions[i2];

            var n = Vector3.Cross(pb - pa, pc - pa); // 2x triangle area
            normals[i0] += n;
            normals[i1] += n;
            normals[i2] += n;
        });
    }

    private delegate void PerTriangleFunc(
        int i0, int i1, int i2, Vector3[] positions, Vector3[] normals);

    private static void ComputeCustomSmoothNormals(this MeshData mesh, PerTriangleFunc func)
    {
        if (mesh.Indices.Count == 0)
            return;

        var positions = GetPositions(mesh);
        var normals = new Vector3[positions.Length];

        var indexCount = mesh.Indices.Count;
        for (var i = 0; i + 2 < indexCount; i += 3)
        {
            var i0 = (int)mesh.Indices[i];
            var i1 = (int)mesh.Indices[i + 1];
            var i2 = (int)mesh.Indices[i + 2];
            func(i0, i1, i2, positions, normals);
        }

        for (var i = 0; i < normals.Length; i++)
            normals[i] = normals[i].LengthSquared() > 1e-12f
                ? Vector3.Normalize(normals[i]) : Vector3.UnitZ;

        WriteNormals(mesh, normals);
    }

    private static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
        => Vector3.Normalize(Vector3.Cross(b - a, c - a));

    private static float AngleBetween(Vector3 a, Vector3 b)
        => MathF.Acos(Math.Clamp(
            Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b)), -1f, 1f));

    private static Vector3[] GetPositions(MeshData mesh)
    {
        if (!MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Position,
                out var si, out var stride, out var offset))
            return [];
        var stream = mesh.Streams[si];
        var count = mesh.VertexCount;
        var result = new Vector3[count];
        for (var i = 0; i < count; i++)
            result[i] = VertexStreamUtils.ReadFloat3(stream, i * stride + offset);
        return result;
    }

    private static void WriteNormals(MeshData mesh, Vector3[] normals)
    {
        if (!MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Normal,
                out var si, out var stride, out var offset))
            return;
        var stream = mesh.Streams[si];
        var count = Math.Min(mesh.VertexCount, normals.Length);
        for (var i = 0; i < count; i++)
            VertexStreamUtils.WriteFloat3(stream, i * stride + offset, normals[i]);
    }
}
