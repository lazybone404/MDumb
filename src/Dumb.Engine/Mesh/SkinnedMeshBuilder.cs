namespace Dumb.Engine.Mesh;

public sealed class SkinnedMeshBuilder : MeshBuilder<SkinnedMeshVertex>
{
    public SkinnedMeshBuilder() : base(MeshData.FromSkinnedVertices) { }
}
