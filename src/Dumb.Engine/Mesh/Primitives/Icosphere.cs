using System.Numerics;

namespace Dumb.Engine.Mesh;

public static partial class MeshPrimitives
{
    public static MeshData CreateIcosphere(float radius, int subdivisions)
    {
        subdivisions = Math.Clamp(subdivisions, 0, 6);

        // Start with icosahedron
        const float x = 0.525731112119133606f;
        const float z = 0.850650808352039932f;

        var verts = new List<Vector3>
        {
            new(-x, 0, z), new(x, 0, z), new(-x, 0, -z), new(x, 0, -z),
            new(0, z, x), new(0, z, -x), new(0, -z, x), new(0, -z, -x),
            new(z, x, 0), new(-z, x, 0), new(z, -x, 0), new(-z, -x, 0),
        };

        var faces = new List<(int, int, int)>
        {
            (0,4,1), (0,9,4), (9,5,4), (4,5,8), (4,8,1),
            (8,10,1), (8,3,10), (5,3,8), (5,2,3), (2,7,3),
            (7,10,3), (7,6,10), (7,11,6), (11,0,6), (0,1,6),
            (6,1,10), (9,0,11), (9,11,2), (9,2,5), (7,2,11),
        };

        // Subdivide
        var midCache = new Dictionary<(int, int), int>();
        for (var s = 0; s < subdivisions; s++)
        {
            midCache.Clear();
            var newFaces = new List<(int, int, int)>();
            foreach (var (a, b, c) in faces)
            {
                var ab = MidPoint(a, b, verts, midCache);
                var bc = MidPoint(b, c, verts, midCache);
                var ca = MidPoint(c, a, verts, midCache);
                newFaces.Add((a, ab, ca));
                newFaces.Add((b, bc, ab));
                newFaces.Add((c, ca, bc));
                newFaces.Add((ab, bc, ca));
            }
            faces = newFaces;
        }

        // Build vertices
        var positions = new Vector3[verts.Count];
        for (var i = 0; i < verts.Count; i++)
            positions[i] = Vector3.Normalize(verts[i]) * radius;

        var normals = new Vector3[verts.Count];
        for (var i = 0; i < verts.Count; i++)
            normals[i] = Vector3.Normalize(verts[i]);

        // UVs via spherical projection
        var uvs = new Vector2[verts.Count];
        for (var i = 0; i < verts.Count; i++)
        {
            var n = normals[i];
            uvs[i] = new Vector2(
                0.5f + MathF.Atan2(n.X, n.Z) / (2 * MathF.PI),
                0.5f - MathF.Asin(n.Y) / MathF.PI);
        }

        var indexList = new List<uint>();
        foreach (var (a, b, c) in faces)
        {
            indexList.Add((uint)a);
            indexList.Add((uint)b);
            indexList.Add((uint)c);
        }

        var vertices = new MeshVertex[verts.Count];
        for (var i = 0; i < verts.Count; i++)
            vertices[i] = new MeshVertex(positions[i], normals[i], uvs[i]);

        return MeshData.FromVertices(vertices, indexList);
    }
}
