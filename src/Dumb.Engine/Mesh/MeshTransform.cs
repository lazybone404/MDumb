using System.Numerics;

namespace Dumb.Engine.Mesh;

public static class MeshTransform
{
    public static void TransformBy(this MeshData mesh, Vector3 translation, Quaternion rotation, Vector3 scale)
    {
        var scaleRecip = new Vector3(1f / scale.X, 1f / scale.Y, 1f / scale.Z);
        var hasNonUniformScale = Math.Abs(scale.X - scale.Y) > 1e-7f
                              || Math.Abs(scale.Y - scale.Z) > 1e-7f;
        var hasRotation = !IsNearIdentity(rotation);

        foreach (var (streamIndex, streamDesc) in mesh.Descriptor.Streams.Index())
        {
            var stride = (int)MeshDescriptor.StreamStride(streamDesc.Elements);
            var stream = mesh.Streams[streamIndex];
            var vertexCount = mesh.VertexCount;

            var attrOffset = 0;
            foreach (var elem in streamDesc.Elements)
            {
                var attrId = elem.Attribute.Id;
                if (attrId == MeshAttribute.IdPosition)
                {
                    for (var i = 0; i < vertexCount; i++)
                    {
                        var off = i * stride + attrOffset;
                        var p = VertexStreamUtils.ReadFloat3(stream, off);
                        p = Vector3.Transform(p, rotation) * scale + translation;
                        VertexStreamUtils.WriteFloat3(stream, off, p);
                    }
                }
                else if (attrId == MeshAttribute.IdNormal && (hasRotation || hasNonUniformScale))
                {
                    for (var i = 0; i < vertexCount; i++)
                    {
                        var off = i * stride + attrOffset;
                        var n = VertexStreamUtils.ReadFloat3(stream, off);
                        if (hasRotation)
                            n = Vector3.Transform(n, rotation);
                        if (hasNonUniformScale)
                            n = ScaleNormal(n, scaleRecip);
                        VertexStreamUtils.WriteFloat3(stream, off, n);
                    }
                }
                else if (attrId == MeshAttribute.IdTangent && (hasRotation || hasNonUniformScale))
                {
                    for (var i = 0; i < vertexCount; i++)
                    {
                        var off = i * stride + attrOffset;
                        var t = VertexStreamUtils.ReadFloat4(stream, off);
                        var handedness = t.W;
                        var tangent3 = new Vector3(t.X, t.Y, t.Z);
                        if (hasRotation)
                            tangent3 = Vector3.Transform(tangent3, rotation);
                        if (hasNonUniformScale)
                            tangent3 *= scale;
                        tangent3 = Vector3.Normalize(tangent3);
                        VertexStreamUtils.WriteFloat4(stream, off, new Vector4(tangent3, handedness));
                    }
                }
                attrOffset += (int)elem.Attribute.Size;
            }
        }

        mesh.InvalidateCache();
    }

    public static void TranslateBy(this MeshData mesh, Vector3 translation)
    {
        if (translation == Vector3.Zero) return;
        if (!MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Position,
                out var si, out var stride, out var attrOffset))
            return;
        var stream = mesh.Streams[si];
        var vertexCount = mesh.VertexCount;
        for (var i = 0; i < vertexCount; i++)
        {
            var off = i * stride + attrOffset;
            var p = VertexStreamUtils.ReadFloat3(stream, off);
            p += translation;
            VertexStreamUtils.WriteFloat3(stream, off, p);
        }
        mesh.InvalidateCache();
    }

    public static void RotateBy(this MeshData mesh, Quaternion rotation)
    {
        if (IsNearIdentity(rotation)) return;

        // Position
        if (MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Position,
                out var psi, out var pStride, out var pOff))
        {
            var stream = mesh.Streams[psi];
            var count = mesh.VertexCount;
            for (var i = 0; i < count; i++)
            {
                var off = i * pStride + pOff;
                var p = VertexStreamUtils.ReadFloat3(stream, off);
                p = Vector3.Transform(p, rotation);
                VertexStreamUtils.WriteFloat3(stream, off, p);
            }
        }

        // Normal
        if (MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Normal,
                out var nsi, out var nStride, out var nOff))
        {
            var stream = mesh.Streams[nsi];
            var count = mesh.VertexCount;
            for (var i = 0; i < count; i++)
            {
                var off = i * nStride + nOff;
                var n = VertexStreamUtils.ReadFloat3(stream, off);
                n = Vector3.Normalize(Vector3.Transform(n, rotation));
                VertexStreamUtils.WriteFloat3(stream, off, n);
            }
        }

        // Tangent
        if (MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Tangent,
                out var tsi, out var tStride, out var tOff))
        {
            var stream = mesh.Streams[tsi];
            var count = mesh.VertexCount;
            for (var i = 0; i < count; i++)
            {
                var off = i * tStride + tOff;
                var t = VertexStreamUtils.ReadFloat4(stream, off);
                var handedness = t.W;
                var tangent3 = Vector3.Normalize(Vector3.Transform(new Vector3(t.X, t.Y, t.Z), rotation));
                VertexStreamUtils.WriteFloat4(stream, off, new Vector4(tangent3, handedness));
            }
        }

        mesh.InvalidateCache();
    }

    public static void ScaleBy(this MeshData mesh, Vector3 scale)
    {
        var scaleRecip = new Vector3(1f / scale.X, 1f / scale.Y, 1f / scale.Z);
        var isUniform = Math.Abs(scale.X - scale.Y) < 1e-7f && Math.Abs(scale.Y - scale.Z) < 1e-7f;

        // Position
        if (MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Position,
                out var psi, out var pStride, out var pOff))
        {
            var stream = mesh.Streams[psi];
            var count = mesh.VertexCount;
            for (var i = 0; i < count; i++)
            {
                var off = i * pStride + pOff;
                var p = VertexStreamUtils.ReadFloat3(stream, off);
                p *= scale;
                VertexStreamUtils.WriteFloat3(stream, off, p);
            }
        }

        if (!isUniform)
        {
            // Normal (non-uniform scale correction)
            if (MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Normal,
                    out var nsi, out var nStride, out var nOff))
            {
                var stream = mesh.Streams[nsi];
                var count = mesh.VertexCount;
                for (var i = 0; i < count; i++)
                {
                    var off = i * nStride + nOff;
                    var n = VertexStreamUtils.ReadFloat3(stream, off);
                    n = ScaleNormal(n, scaleRecip);
                    VertexStreamUtils.WriteFloat3(stream, off, n);
                }
            }

            // Tangent (non-uniform scale correction)
            if (MeshDescriptor.TryFindAttribute(mesh.Descriptor.Streams, MeshAttribute.Tangent,
                    out var tsi, out var tStride, out var tOff))
            {
                var stream = mesh.Streams[tsi];
                var count = mesh.VertexCount;
                for (var i = 0; i < count; i++)
                {
                    var off = i * tStride + tOff;
                    var t = VertexStreamUtils.ReadFloat4(stream, off);
                    var handedness = t.W;
                    var tangent3 = Vector3.Normalize(new Vector3(t.X, t.Y, t.Z) * scale);
                    VertexStreamUtils.WriteFloat4(stream, off, new Vector4(tangent3, handedness));
                }
            }
        }

        mesh.InvalidateCache();
    }

    private static Vector3 ScaleNormal(Vector3 normal, Vector3 scaleRecip)
    {
        var n = normal * scaleRecip;
        return n.LengthSquared() > 1e-12f ? Vector3.Normalize(n) : normal;
    }

    private static bool IsNearIdentity(Quaternion q)
        => Math.Abs(q.X) < 1e-7f && Math.Abs(q.Y) < 1e-7f
        && Math.Abs(q.Z) < 1e-7f && Math.Abs(1f - q.W) < 1e-7f;

}
