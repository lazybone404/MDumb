using System.Numerics;

namespace Dumb.Engine.Mesh;

public static class MeshPrimitives
{

    public static MeshData CreateCuboid(float width, float height, float depth)
    {
        var hw = width * 0.5f; var hh = height * 0.5f; var hd = depth * 0.5f;

        var positions = new Vector3[]
        {
            new(-hw, -hh,  hd), new( hw, -hh,  hd), new( hw,  hh,  hd), new(-hw,  hh,  hd), // front
            new(-hw,  hh, -hd), new( hw,  hh, -hd), new( hw, -hh, -hd), new(-hw, -hh, -hd), // back
            new( hw, -hh, -hd), new( hw,  hh, -hd), new( hw,  hh,  hd), new( hw, -hh,  hd), // right
            new(-hw, -hh,  hd), new(-hw,  hh,  hd), new(-hw,  hh, -hd), new(-hw, -hh, -hd), // left
            new( hw,  hh, -hd), new(-hw,  hh, -hd), new(-hw,  hh,  hd), new( hw,  hh,  hd), // top
            new( hw, -hh,  hd), new(-hw, -hh,  hd), new(-hw, -hh, -hd), new( hw, -hh, -hd), // bottom
        };

        var normals = new Vector3[]
        {
            Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ,
            -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ,
            Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector3.UnitX,
            -Vector3.UnitX, -Vector3.UnitX, -Vector3.UnitX, -Vector3.UnitX,
            Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector3.UnitY,
            -Vector3.UnitY, -Vector3.UnitY, -Vector3.UnitY, -Vector3.UnitY,
        };

        var uvs = new Vector2[]
        {
            new(0,0), new(1,0), new(1,1), new(0,1),
            new(1,0), new(0,0), new(0,1), new(1,1),
            new(0,0), new(1,0), new(1,1), new(0,1),
            new(1,0), new(0,0), new(0,1), new(1,1),
            new(1,0), new(0,0), new(0,1), new(1,1),
            new(0,0), new(1,0), new(1,1), new(0,1),
        };

        return BuildMesh(positions, normals, uvs, new uint[]
        {
            0,1,2, 2,3,0, 4,5,6, 6,7,4, 8,9,10, 10,11,8,
            12,13,14, 14,15,12, 16,17,18, 18,19,16, 20,21,22, 22,23,20,
        });
    }

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
