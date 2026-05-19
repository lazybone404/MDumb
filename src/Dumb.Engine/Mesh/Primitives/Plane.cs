using System.Numerics;

namespace Dumb.Engine.Mesh;

public static partial class MeshPrimitives
{
    public static MeshData CreatePlane(float size, int subdivisions = 0)
    {
        var halfSize = size * 0.5f;
        var zCount = subdivisions + 2;
        var xCount = subdivisions + 2;

        var vertices = new List<MeshVertex>();
        var indices = new List<uint>();

        for (var z = 0; z < zCount; z++)
        {
            for (var x = 0; x < xCount; x++)
            {
                var tx = (float)x / (xCount - 1);
                var tz = (float)z / (zCount - 1);
                var pos = new Vector3((-0.5f + tx) * size, 0, (-0.5f + tz) * size);
                vertices.Add(new MeshVertex(pos, Vector3.UnitY, new Vector2(tx, tz)));
            }
        }

        for (uint z = 0; z < zCount - 1; z++)
        {
            for (uint x = 0; x < xCount - 1; x++)
            {
                var quad = z * (uint)xCount + x;
                indices.Add(quad + (uint)xCount + 1);
                indices.Add(quad + 1);
                indices.Add(quad + (uint)xCount);
                indices.Add(quad);
                indices.Add(quad + (uint)xCount);
                indices.Add(quad + 1);
            }
        }

        return MeshData.FromVertices(vertices, indices);
    }
}
