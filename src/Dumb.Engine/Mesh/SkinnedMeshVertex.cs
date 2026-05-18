using System.Numerics;
using System.Runtime.InteropServices;

namespace Dumb.Engine.Mesh;

[StructLayout(LayoutKind.Sequential)]
public struct SkinnedMeshVertex(
    Vector3 position, Vector3 normal, Vector2 uv, Vector4 tangent,
    Vector4 boneWeights, ushort boneIndex0, ushort boneIndex1, ushort boneIndex2, ushort boneIndex3)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector2 UV = uv;
    public Vector4 Tangent = tangent;
    public Vector4 BoneWeights = boneWeights;
    public ushort BoneIndex0 = boneIndex0;
    public ushort BoneIndex1 = boneIndex1;
    public ushort BoneIndex2 = boneIndex2;
    public ushort BoneIndex3 = boneIndex3;

    public SkinnedMeshVertex(
        Vector3 position, Vector3 normal, Vector2 uv, Vector4 tangent,
        Vector4 boneWeights, uint i0, uint i1, uint i2, uint i3)
        : this(position, normal, uv, tangent, boneWeights,
            (ushort)i0, (ushort)i1, (ushort)i2, (ushort)i3)
    {
    }
}
