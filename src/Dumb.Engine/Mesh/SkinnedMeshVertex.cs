using System.Numerics;
using System.Runtime.InteropServices;

namespace Dumb.Engine.Mesh;

[StructLayout(LayoutKind.Sequential)]
public struct SkinnedMeshVertex(
    Vector3 position, Vector3 normal, Vector2 uv, Vector4 tangent,
    Vector4 boneWeights, uint boneIndex0, uint boneIndex1, uint boneIndex2, uint boneIndex3)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector2 UV = uv;
    public Vector4 Tangent = tangent;
    public Vector4 BoneWeights = boneWeights;
    public uint BoneIndex0 = boneIndex0;
    public uint BoneIndex1 = boneIndex1;
    public uint BoneIndex2 = boneIndex2;
    public uint BoneIndex3 = boneIndex3;
}
