using System.Numerics;

namespace Dumb.Engine.Mesh;

public static partial class MeshPrimitives
{
    public static MeshData CreateCylinder(float radius, float height, int sectors)
    {
        sectors = Math.Max(3, sectors);
        var halfHeight = height * 0.5f;

        var vertices = new List<MeshVertex>();
        var indices = new List<uint>();

        // Side vertices
        for (var i = 0; i <= sectors; i++)
        {
            var angle = i * 2 * MathF.PI / sectors;
            var x = radius * MathF.Cos(angle);
            var z = radius * MathF.Sin(angle);
            var normal = new Vector3(MathF.Cos(angle), 0, MathF.Sin(angle));
            var uv = new Vector2((float)i / sectors, 0);

            vertices.Add(new MeshVertex(new Vector3(x, -halfHeight, z), normal, uv));
            vertices.Add(new MeshVertex(new Vector3(x, halfHeight, z), normal, new Vector2(uv.X, 1)));
        }

        // Side indices
        for (uint i = 0; i < sectors; i++)
        {
            var a = i * 2;
            var b = a + 1;
            var c = a + 2;
            var d = a + 3;
            indices.Add(a); indices.Add(b); indices.Add(c);
            indices.Add(b); indices.Add(d); indices.Add(c);
        }

        // Top cap
        var topCenter = new Vector3(0, halfHeight, 0);
        var topStart = (uint)vertices.Count;
        vertices.Add(new MeshVertex(topCenter, Vector3.UnitY, new Vector2(0.5f, 0.5f)));
        for (var i = 0; i <= sectors; i++)
        {
            var angle = i * 2 * MathF.PI / sectors;
            var x = radius * MathF.Cos(angle);
            var z = radius * MathF.Sin(angle);
            var pos = new Vector3(x, halfHeight, z);
            vertices.Add(new MeshVertex(pos, Vector3.UnitY, new Vector2(MathF.Cos(angle) * 0.5f + 0.5f, MathF.Sin(angle) * 0.5f + 0.5f)));
        }
        for (uint i = 0; i < sectors; i++)
        {
            indices.Add(topStart);
            indices.Add(topStart + i + 1);
            indices.Add(topStart + i + 2);
        }

        // Bottom cap
        var bottomCenter = new Vector3(0, -halfHeight, 0);
        var bottomStart = (uint)vertices.Count;
        vertices.Add(new MeshVertex(bottomCenter, -Vector3.UnitY, new Vector2(0.5f, 0.5f)));
        for (var i = 0; i <= sectors; i++)
        {
            var angle = i * 2 * MathF.PI / sectors;
            var x = radius * MathF.Cos(angle);
            var z = radius * MathF.Sin(angle);
            var pos = new Vector3(x, -halfHeight, z);
            vertices.Add(new MeshVertex(pos, -Vector3.UnitY, new Vector2(MathF.Cos(angle) * 0.5f + 0.5f, MathF.Sin(angle) * 0.5f + 0.5f)));
        }
        for (uint i = 0; i < sectors; i++)
        {
            indices.Add(bottomStart);
            indices.Add(bottomStart + i + 2);
            indices.Add(bottomStart + i + 1);
        }

        return MeshData.FromVertices(vertices, indices);
    }
}
