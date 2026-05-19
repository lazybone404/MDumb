using System.Numerics;

namespace Dumb.Engine.Mesh;

public static partial class MeshPrimitives
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
}
