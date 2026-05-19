using System.Numerics;

namespace Dumb.Engine.Mesh;

public static partial class MeshPrimitives
{
    private static int MidPoint(int i1, int i2, List<Vector3> verts,
        Dictionary<(int, int), int> cache)
    {
        var key = i1 < i2 ? (i1, i2) : (i2, i1);
        if (cache.TryGetValue(key, out var idx))
            return idx;

        var mid = (verts[i1] + verts[i2]) * 0.5f;
        idx = verts.Count;
        verts.Add(mid);
        cache[key] = idx;
        return idx;
    }

    private static MeshData BuildMesh(Vector3[] positions, Vector3[] normals, Vector2[] uvs, uint[] indices)
    {
        var vertices = new MeshVertex[positions.Length];
        for (var i = 0; i < positions.Length; i++)
            vertices[i] = new MeshVertex(positions[i], normals[i], uvs[i]);
        return MeshData.FromVertices(vertices, indices);
    }
}
