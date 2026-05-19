using System.Numerics;

namespace Dumb.Engine.Mesh;

public static partial class MeshPrimitives
{
    public static MeshData CreateCapsule(float radius, float length, int sectors, int stacks)
    {
        var halfLength = length * 0.5f;
        var vertices = new List<MeshVertex>();
        var indices = new List<uint>();

        // Cylinder body
        for (var i = 0; i <= sectors; i++)
        {
            var angle = i * 2 * MathF.PI / sectors;
            var x = radius * MathF.Cos(angle);
            var z = radius * MathF.Sin(angle);
            var normal = new Vector3(MathF.Cos(angle), 0, MathF.Sin(angle));
            var uv = new Vector2((float)i / sectors, 0);

            vertices.Add(new MeshVertex(new Vector3(x, -halfLength, z), normal, uv));
            vertices.Add(new MeshVertex(new Vector3(x, halfLength, z), normal, new Vector2(uv.X, 1)));
        }

        // Cylinder indices
        for (uint i = 0; i < sectors; i++)
        {
            var a = i * 2; var b = a + 1; var c = a + 2; var d = a + 3;
            indices.Add(a); indices.Add(b); indices.Add(c);
            indices.Add(b); indices.Add(d); indices.Add(c);
        }

        // Top hemisphere
        var topBase = (uint)vertices.Count;
        for (var i = 0; i <= stacks / 2; i++)
        {
            var phi = i * MathF.PI / 2 / (stacks / 2);
            var y = halfLength + radius * MathF.Sin(phi);
            var r = radius * MathF.Cos(phi);

            for (var j = 0; j <= sectors; j++)
            {
                var theta = j * 2 * MathF.PI / sectors;
                var pos = new Vector3(r * MathF.Cos(theta), y, r * MathF.Sin(theta));
                var normal = Vector3.Normalize(pos - new Vector3(0, halfLength, 0));
                vertices.Add(new MeshVertex(pos, normal, new Vector2((float)j / sectors, (float)i / (stacks / 2))));
            }
        }
        for (uint i = 0; i < stacks / 2; i++)
        for (uint j = 0; j < sectors; j++)
        {
            var a = topBase + i * (uint)(sectors + 1) + j;
            var b = a + (uint)(sectors + 1);
            indices.Add(a); indices.Add(b); indices.Add(a + 1);
            indices.Add(a + 1); indices.Add(b); indices.Add(b + 1);
        }

        // Bottom hemisphere
        var bottomBase = (uint)vertices.Count;
        for (var i = 0; i <= stacks / 2; i++)
        {
            var phi = -MathF.PI / 2 + i * MathF.PI / 2 / (stacks / 2);
            var y = -halfLength + radius * MathF.Sin(phi);
            var r = radius * MathF.Cos(phi);

            for (var j = 0; j <= sectors; j++)
            {
                var theta = j * 2 * MathF.PI / sectors;
                var pos = new Vector3(r * MathF.Cos(theta), y, r * MathF.Sin(theta));
                var normal = Vector3.Normalize(pos - new Vector3(0, -halfLength, 0));
                vertices.Add(new MeshVertex(pos, normal, new Vector2((float)j / sectors, (float)i / (stacks / 2))));
            }
        }
        for (uint i = 0; i < stacks / 2; i++)
        for (uint j = 0; j < sectors; j++)
        {
            var a = bottomBase + i * (uint)(sectors + 1) + j;
            var b = a + (uint)(sectors + 1);
            indices.Add(a); indices.Add(b); indices.Add(a + 1);
            indices.Add(a + 1); indices.Add(b); indices.Add(b + 1);
        }

        return MeshData.FromVertices(vertices, indices);
    }
}
