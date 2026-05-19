using System.Numerics;

namespace Dumb.Engine.Mesh;

public static class MeshTangents
{
    public static MeshData WithTangents(this MeshData mesh)
    {
        GenerateTangents(mesh);
        return mesh;
    }

    public static void GenerateTangents(this MeshData mesh)
    {
        var positions = ReadFloat3Attribute(mesh, MeshAttribute.Position);
        var normals = ReadFloat3Attribute(mesh, MeshAttribute.Normal);
        var uvs = GetUVs(mesh);

        if (positions.Length == 0)
            throw new GenerateTangentsException("Mesh has no position attribute");
        if (normals.Length == 0)
            throw new GenerateTangentsException("Mesh has no normal attribute");
        if (uvs.Length == 0)
            throw new GenerateTangentsException("Mesh has no UV attribute");

        var vertexCount = Math.Min(positions.Length, Math.Min(normals.Length, uvs.Length));
        var tangents = new Vector3[vertexCount];
        var bitangents = new Vector3[vertexCount];

        // Accumulate tangent and bitangent per face
        var indexCount = mesh.Indices.Count;
        var hasIndices = indexCount > 0;
        var faceCount = hasIndices ? indexCount / 3 : vertexCount / 3;

        for (var f = 0; f < faceCount; f++)
        {
            int i0, i1, i2;
            if (hasIndices)
            {
                if (mesh.Indices.Format == Silk.NET.WebGPU.IndexFormat.Uint32)
                {
                    var idx = mesh.Indices.GetUInt32Span();
                    i0 = (int)idx[f * 3];
                    i1 = (int)idx[f * 3 + 1];
                    i2 = (int)idx[f * 3 + 2];
                }
                else
                {
                    var idx = mesh.Indices.GetUInt16Span();
                    i0 = idx[f * 3];
                    i1 = idx[f * 3 + 1];
                    i2 = idx[f * 3 + 2];
                }
            }
            else
            {
                i0 = f * 3;
                i1 = f * 3 + 1;
                i2 = f * 3 + 2;
            }

            AccumulateTriangle(i0, i1, i2);

        void AccumulateTriangle(int i0, int i1, int i2)
        {
            var p0 = positions[i0];
            var p1 = positions[i1];
            var p2 = positions[i2];

            var uv0 = uvs[i0];
            var uv1 = uvs[i1];
            var uv2 = uvs[i2];

            var e1 = p1 - p0;
            var e2 = p2 - p0;

            var duv1 = uv1 - uv0;
            var duv2 = uv2 - uv0;

            var fInv = duv1.X * duv2.Y - duv2.X * duv1.Y;
            if (Math.Abs(fInv) < 1e-12f) return;
            fInv = 1f / fInv;

            var tangent = new Vector3(
                fInv * (duv2.Y * e1.X - duv1.Y * e2.X),
                fInv * (duv2.Y * e1.Y - duv1.Y * e2.Y),
                fInv * (duv2.Y * e1.Z - duv1.Y * e2.Z));

            var bitangent = new Vector3(
                fInv * (-duv2.X * e1.X + duv1.X * e2.X),
                fInv * (-duv2.X * e1.Y + duv1.X * e2.Y),
                fInv * (-duv2.X * e1.Z + duv1.X * e2.Z));

            tangents[i0] += tangent;
            tangents[i1] += tangent;
            tangents[i2] += tangent;

            bitangents[i0] += bitangent;
            bitangents[i1] += bitangent;
            bitangents[i2] += bitangent;
        }
        }

        // Orthogonalize and compute handedness
        var result = new Vector4[vertexCount];
        for (var i = 0; i < vertexCount; i++)
        {
            var n = normals[i];
            var t = tangents[i];

            // Gram-Schmidt orthogonalize
            var dot = Vector3.Dot(n, t);
            t = t - n * dot;

            if (t.LengthSquared() < 1e-12f)
            {
                // Degenerate: pick an arbitrary perpendicular vector
                t = Math.Abs(n.X) < Math.Abs(n.Y)
                    ? Vector3.Normalize(Vector3.Cross(n, Vector3.UnitX))
                    : Vector3.Normalize(Vector3.Cross(n, Vector3.UnitY));
            }
            else
            {
                t = Vector3.Normalize(t);
            }

            // Handedness: dot(cross(n, t), bitangent) determines sign.
            // MikkTSpace assumes left-handed; flip to right-handed
            // (standard OpenGL/Vulkan/DirectX convention).
            var handedness = Vector3.Dot(Vector3.Cross(n, t), bitangents[i]) < 0 ? 1f : -1f;
            result[i] = new Vector4(t, handedness);
        }

        WriteTangents(mesh, result);
    }

    private static Vector2[] GetUVs(MeshData mesh)
    {
        if (!MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.UV0,
                out var si, out var stride, out var offset))
            return [];
        var stream = mesh.Streams[si];
        var count = mesh.VertexCount;
        var result = new Vector2[count];
        for (var i = 0; i < count; i++)
            result[i] = VertexStreamUtils.ReadFloat2(stream, i * stride + offset);
        return result;
    }

    private static Vector3[] ReadFloat3Attribute(MeshData mesh, MeshAttribute attr)
    {
        if (!MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, attr,
                out var si, out var stride, out var offset))
            return [];
        var stream = mesh.Streams[si];
        var count = mesh.VertexCount;
        var result = new Vector3[count];
        for (var i = 0; i < count; i++)
            result[i] = VertexStreamUtils.ReadFloat3(stream, i * stride + offset);
        return result;
    }

    private static void WriteTangents(MeshData mesh, Vector4[] tangents)
    {
        if (!MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Tangent,
                out var si, out var stride, out var offset))
            return;
        var stream = mesh.Streams[si];
        var count = Math.Min(mesh.VertexCount, tangents.Length);
        for (var i = 0; i < count; i++)
            VertexStreamUtils.WriteFloat4(stream, i * stride + offset, tangents[i]);
    }
}
