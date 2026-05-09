using System.Numerics;
using System.Runtime.InteropServices;

namespace Dumb.Engine.Mesh;

[StructLayout(LayoutKind.Sequential)]
public struct MeshVertex(Vector3 position, Vector3 normal, Vector3 color)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector3 Color = color;
}
