using System.Numerics;
using Dumb.Engine.Mesh;

namespace Dumb.Engine.Tests.Mesh;

public sealed class MeshBuilderTests
{
    [Fact]
    public void AddVertex_ReturnsCorrectIndex()
    {
        var builder = new MeshBuilder();

        var i0 = builder.AddVertex(new MeshVertex(Vector3.Zero, Vector3.UnitY, Vector3.One));
        var i1 = builder.AddVertex(new MeshVertex(Vector3.UnitX, Vector3.UnitY, Vector3.One));
        var i2 = builder.AddVertex(new MeshVertex(Vector3.UnitZ, Vector3.UnitY, Vector3.One));

        Assert.Equal(0u, i0);
        Assert.Equal(1u, i1);
        Assert.Equal(2u, i2);
        Assert.Equal(3, builder.VertexCount);
    }

    [Fact]
    public void AddTriangle_AddsThreeIndices()
    {
        var builder = new MeshBuilder();

        builder.AddVertex(new MeshVertex(Vector3.Zero, Vector3.UnitY, Vector3.One));
        builder.AddVertex(new MeshVertex(Vector3.UnitX, Vector3.UnitY, Vector3.One));
        builder.AddVertex(new MeshVertex(Vector3.UnitZ, Vector3.UnitY, Vector3.One));

        builder.AddTriangle(0, 1, 2);

        Assert.Equal(3, builder.IndexCount);
    }

    [Fact]
    public void AddQuad_AddsSixIndices()
    {
        var builder = new MeshBuilder();

        builder.AddVertex(new MeshVertex(new Vector3(-1, -1, 0), Vector3.UnitZ, Vector3.One));
        builder.AddVertex(new MeshVertex(new Vector3(1, -1, 0), Vector3.UnitZ, Vector3.One));
        builder.AddVertex(new MeshVertex(new Vector3(1, 1, 0), Vector3.UnitZ, Vector3.One));
        builder.AddVertex(new MeshVertex(new Vector3(-1, 1, 0), Vector3.UnitZ, Vector3.One));

        builder.AddQuad(0, 1, 2, 3);

        Assert.Equal(6, builder.IndexCount); // 2 triangles = 6 indices
    }

    [Fact]
    public void Build_ProducesValidMeshData()
    {
        var builder = new MeshBuilder();

        var v0 = builder.AddVertex(new MeshVertex(
            new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0)));
        var v1 = builder.AddVertex(new MeshVertex(
            new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0)));
        var v2 = builder.AddVertex(new MeshVertex(
            new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0)));

        builder.AddTriangle(v0, v1, v2);

        var mesh = builder.Build();

        Assert.NotNull(mesh);
        Assert.Equal(3, mesh.VertexCount);
        Assert.Equal(3, mesh.IndexCount);
        Assert.True(mesh.TryValidate(out var error));
        Assert.Null(error);
    }

    [Fact]
    public void Clear_ResetsVertexAndIndexCounts()
    {
        var builder = new MeshBuilder();

        builder.AddVertex(new MeshVertex(Vector3.Zero, Vector3.UnitY, Vector3.One));
        builder.AddVertex(new MeshVertex(Vector3.UnitX, Vector3.UnitY, Vector3.One));
        builder.AddTriangle(0, 1, 0);

        Assert.Equal(2, builder.VertexCount);
        Assert.Equal(3, builder.IndexCount);

        builder.Clear();

        Assert.Equal(0, builder.VertexCount);
        Assert.Equal(0, builder.IndexCount);

        // 清除后可以重新构建
        var v0 = builder.AddVertex(new MeshVertex(Vector3.Zero, Vector3.UnitZ, Vector3.One));
        Assert.Equal(0u, v0);
        Assert.Equal(1, builder.VertexCount);
    }

    [Fact]
    public void AddFace_AddsFourVerticesAndSixIndices()
    {
        var builder = new MeshBuilder();

        builder.AddFace(
            new Vector3(-1, 0, -1), new Vector3(1, 0, -1),
            new Vector3(1, 0, 1), new Vector3(-1, 0, 1),
            Vector3.UnitY, new Vector3(0.5f, 0.5f, 0.5f));

        Assert.Equal(4, builder.VertexCount);
        Assert.Equal(6, builder.IndexCount);

        var mesh = builder.Build();
        Assert.True(mesh.TryValidate(out _));
    }

    [Fact]
    public void GenericMeshBuilder_WorksWithCustomVertexType()
    {
        var builder = new MeshBuilder<SkinnedMeshVertex>(
            MeshData.FromSkinnedVertices);

        builder.AddVertex(new SkinnedMeshVertex(
            Vector3.Zero, Vector3.UnitY, Vector2.Zero,
            new Vector4(1, 0, 0, 1),
            new Vector4(1, 0, 0, 0),
            0, 0, 0, 0));
        builder.AddVertex(new SkinnedMeshVertex(
            Vector3.UnitX, Vector3.UnitY, Vector2.One,
            new Vector4(1, 0, 0, 1),
            new Vector4(0, 1, 0, 0),
            0, 0, 0, 0));

        builder.AddTriangle(0, 0, 1); // degenerate but valid structurally

        Assert.Equal(2, builder.VertexCount);
        Assert.Equal(3, builder.IndexCount);
    }

    [Fact]
    public void MeshVertex_Constructors_SetCorrectDefaults()
    {
        var v1 = new MeshVertex(
            new Vector3(1, 2, 3), new Vector3(0, 0, -1), new Vector3(1, 0, 0));

        Assert.Equal(new Vector3(1, 2, 3), v1.Position);
        Assert.Equal(new Vector3(0, 0, -1), v1.Normal);
        Assert.Equal(new Vector3(1, 0, 0), v1.Color);
        // 默认 UV 应该是 (0, 0)
        Assert.Equal(Vector2.Zero, v1.UV);
        // 默认 Tangent 应该是 (1, 0, 0, 1)
        Assert.Equal(new Vector4(1, 0, 0, 1), v1.Tangent);

        var v2 = new MeshVertex(
            new Vector3(0, 5, 0), new Vector3(0, 1, 0), new Vector2(0.5f, 0.5f));

        Assert.Equal(new Vector3(0, 5, 0), v2.Position);
        Assert.Equal(new Vector3(0, 1, 0), v2.Normal);
        Assert.Equal(new Vector2(0.5f, 0.5f), v2.UV);
        Assert.Equal(Vector3.One, v2.Color);
    }

    [Fact]
    public void MeshData_ComputeAabb_ReturnsCorrectBounds()
    {
        var vertices = new MeshVertex[]
        {
            new(new Vector3(-1, -1, -1), Vector3.UnitY, Vector3.One),
            new(new Vector3(1, -1, -1), Vector3.UnitY, Vector3.One),
            new(new Vector3(0, 2, 0), Vector3.UnitY, Vector3.One),
        };
        var data = MeshData.FromVertices(vertices, [0u, 1, 2]);

        var aabb = data.ComputeAabb();

        Assert.Equal(new Vector3(-1, -1, -1), aabb.Min);
        Assert.Equal(new Vector3(1, 2, 0), aabb.Max);
    }

    [Fact]
    public void MeshData_ComputeAabb_EmptyData_ReturnsZeroBounds()
    {
        var data = new MeshData(
            new MeshDescriptor([]),
            [],
            new Indices());

        var aabb = data.ComputeAabb();

        Assert.Equal(Vector3.Zero, aabb.Min);
        Assert.Equal(Vector3.Zero, aabb.Max);
    }

    [Fact]
    public void MeshDescriptor_StreamStride_CalculatesCorrectly()
    {
        var elements = new VertexElement[]
        {
            new(MeshAttribute.Position, 0),  // Float32x3 = 12 bytes
            new(MeshAttribute.Normal, 1),    // Float32x3 = 12 bytes
            new(MeshAttribute.UV0, 2),       // Float32x2 = 8 bytes
        };

        var stride = MeshDescriptor.StreamStride(elements);

        Assert.Equal(32u, stride); // 12 + 12 + 8
    }

    [Fact]
    public void MeshDescriptor_TryFindAttribute_FindsExistingAttribute()
    {
        var streams = new[]
        {
            new VertexStreamDescriptor([
                new VertexElement(MeshAttribute.Position, 0),
                new VertexElement(MeshAttribute.Normal, 1)
            ]),
            new VertexStreamDescriptor([
                new VertexElement(MeshAttribute.UV0, 2),
                new VertexElement(MeshAttribute.Color, 3)
            ])
        };

        Assert.True(MeshDescriptor.TryFindAttribute(streams, MeshAttribute.Color,
            out var streamIndex, out var streamStride, out var elementOffset));
        Assert.Equal(1, streamIndex);
        Assert.Equal(24, streamStride); // UV0(8) + Color(16)
        Assert.Equal(8, elementOffset); // UV0 is first at offset 0, Color at offset 8
    }

    [Fact]
    public void MeshDescriptor_TryFindAttribute_NotFound_ReturnsFalse()
    {
        var streams = new[]
        {
            new VertexStreamDescriptor([
                new VertexElement(MeshAttribute.Position, 0)
            ])
        };

        Assert.False(MeshDescriptor.TryFindAttribute(streams, MeshAttribute.Tangent,
            out _, out _, out _));
    }

    [Fact]
    public void MeshDescriptor_GetSortedElements_SortsByAttributeId()
    {
        var streams = new[]
        {
            new VertexStreamDescriptor([
                new VertexElement(MeshAttribute.Color, 0),    // Id=4
                new VertexElement(MeshAttribute.Position, 1)  // Id=0
            ])
        };

        var sorted = MeshDescriptor.GetSortedElements(streams);

        Assert.Equal(2, sorted.Length);
        // 按 Id 排序：Position(id=0) < Color(id=4)
        Assert.Equal(MeshAttribute.Position, sorted[0].element.Attribute);
        Assert.Equal(MeshAttribute.Color, sorted[1].element.Attribute);
    }

    [Fact]
    public void SubMesh_DefaultTriangleList_Indexed()
    {
        var subMesh = new SubMesh(0, 36, 0, 24, Silk.NET.WebGPU.PrimitiveTopology.TriangleList);

        Assert.Equal(0u, subMesh.IndexStart);
        Assert.Equal(36u, subMesh.IndexCount);
        Assert.Equal(0u, subMesh.VertexStart);
        Assert.Equal(24u, subMesh.VertexCount);
    }
}
