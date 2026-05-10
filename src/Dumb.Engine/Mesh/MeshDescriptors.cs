using Silk.NET.WebGPU;

namespace Dumb.Engine.Mesh;

public static class MeshDescriptors
{
    public static VertexStreamDescriptor BoneStream => new(
    [
        new VertexElement(MeshAttribute.BoneWeights),
        new VertexElement(MeshAttribute.BoneIndices)
    ]);

    public static VertexStreamDescriptor BoneStreamInstanced => new(
    [
        new VertexElement(MeshAttribute.BoneWeights),
        new VertexElement(MeshAttribute.BoneIndices)
    ], VertexStepMode.Instance);
}
