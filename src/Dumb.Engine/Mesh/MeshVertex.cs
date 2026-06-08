using System.Numerics;
using System.Runtime.InteropServices;

namespace Dumb.Engine.Mesh;

[StructLayout(LayoutKind.Sequential)]
public struct MeshVertex(
    Vector3 position, Vector3 normal, Vector2 uv, Vector4 tangent, Vector3 color)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector2 UV = uv;
    public Vector4 Tangent = tangent;
    public Vector3 Color = color;

    public MeshVertex(Vector3 position, Vector3 normal, Vector3 color)
        : this(position, normal, Vector2.Zero, new Vector4(1, 0, 0, 1), color)
    {
    }

    public MeshVertex(Vector3 position, Vector3 normal, Vector2 uv)
        : this(position, normal, uv, new Vector4(1, 0, 0, 1), Vector3.One)
    {
    }
}
