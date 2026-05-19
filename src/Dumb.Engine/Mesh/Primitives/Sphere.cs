using System.Numerics;

namespace Dumb.Engine.Mesh;

public static partial class MeshPrimitives
{
    public static MeshData CreateSphere(float radius, int sectors, int stacks)
    {
        sectors = Math.Max(3, sectors);
        stacks = Math.Max(2, stacks);

        var vertices = new List<MeshVertex>();
        var indices = new List<uint>();

        for (var i = 0; i <= stacks; i++)
        {
            var stackAngle = MathF.PI / 2 - i * MathF.PI / stacks;
            var xy = radius * MathF.Cos(stackAngle);
            var z = radius * MathF.Sin(stackAngle);

            for (var j = 0; j <= sectors; j++)
            {
                var sectorAngle = j * 2 * MathF.PI / sectors;
                var x = xy * MathF.Cos(sectorAngle);
                var y = xy * MathF.Sin(sectorAngle);

                var pos = new Vector3(x, y, z);
                var normal = Vector3.Normalize(pos);
                var uv = new Vector2((float)j / sectors, (float)i / stacks);
                vertices.Add(new MeshVertex(pos, normal, uv));
            }
        }

        for (uint i = 0; i < stacks; i++)
        {
            var k1 = i * (uint)(sectors + 1);
            var k2 = k1 + (uint)(sectors + 1);

            for (uint j = 0; j < sectors; j++, k1++, k2++)
            {
                if (i != 0)
                {
                    indices.Add(k1);
                    indices.Add(k2);
                    indices.Add(k1 + 1);
                }
                if (i != stacks - 1)
                {
                    indices.Add(k1 + 1);
                    indices.Add(k2);
                    indices.Add(k2 + 1);
                }
            }
        }

        return MeshData.FromVertices(vertices, indices);
    }
}
