using System.Numerics;

namespace Dumb.Engine.Mesh;

public sealed class MeshBuilder
{
    private readonly List<MeshVertex> _vertices = [];
    private readonly List<uint> _indices = [];

    public int VertexCount => _vertices.Count;
    public int IndexCount => _indices.Count;

    public uint AddVertex(MeshVertex vertex)
    {
        var index = (uint)_vertices.Count;
        _vertices.Add(vertex);
        return index;
    }

    public void AddTriangle(uint i0, uint i1, uint i2)
    {
        _indices.Add(i0);
        _indices.Add(i1);
        _indices.Add(i2);
    }

    public void AddQuad(uint i0, uint i1, uint i2, uint i3)
    {
        _indices.Add(i0);
        _indices.Add(i1);
        _indices.Add(i2);
        _indices.Add(i0);
        _indices.Add(i2);
        _indices.Add(i3);
    }

    public void AddFace(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal, Vector3 color)
    {
        var i0 = AddVertex(new MeshVertex(v0, normal, color));
        var i1 = AddVertex(new MeshVertex(v1, normal, color));
        var i2 = AddVertex(new MeshVertex(v2, normal, color));
        var i3 = AddVertex(new MeshVertex(v3, normal, color));
        AddQuad(i0, i1, i2, i3);
    }

    public MeshData Build()
    {
        return MeshData.FromVertices(_vertices, _indices);
    }

    public void Clear()
    {
        _vertices.Clear();
        _indices.Clear();
    }
}
