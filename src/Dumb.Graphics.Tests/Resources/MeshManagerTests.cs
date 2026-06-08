using Dumb.Engine.Mesh;
using Silk.NET.WebGPU;

namespace Dumb.Graphics.Tests.Resources;

public sealed class MeshManagerTests
{
    [Fact]
    public void ToVertexBufferLayouts_SingleStream_ReturnsCorrectLayout()
    {
        var streams = new[]
        {
            new VertexStreamDescriptor([
                new VertexElement(MeshAttribute.Position, 0),
                new VertexElement(MeshAttribute.Normal, 1),
                new VertexElement(MeshAttribute.UV0, 2)
            ])
        };

        var layouts = MeshManager.ToVertexBufferLayouts(streams);

        Assert.Single(layouts);
        Assert.Equal(3, layouts[0].Attributes.Length);

        // Position: offset 0, Float32x3
        Assert.Equal(0u, layouts[0].Attributes[0].ShaderLocation);
        Assert.Equal(VertexFormat.Float32x3, layouts[0].Attributes[0].Format);
        Assert.Equal(0ul, layouts[0].Attributes[0].Offset);

        // Normal: offset 12, Float32x3
        Assert.Equal(1u, layouts[0].Attributes[1].ShaderLocation);
        Assert.Equal(VertexFormat.Float32x3, layouts[0].Attributes[1].Format);
        Assert.Equal(12ul, layouts[0].Attributes[1].Offset);

        // UV0: offset 24, Float32x2
        Assert.Equal(2u, layouts[0].Attributes[2].ShaderLocation);
        Assert.Equal(VertexFormat.Float32x2, layouts[0].Attributes[2].Format);
        Assert.Equal(24ul, layouts[0].Attributes[2].Offset);

        // Stride = 12 + 12 + 8 = 32
        Assert.Equal(32ul, layouts[0].Stride);
    }

    [Fact]
    public void ToVertexBufferLayouts_MultipleStreams_ReturnsCorrectLayouts()
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

        var layouts = MeshManager.ToVertexBufferLayouts(streams);

        Assert.Equal(2, layouts.Length);

        // Stream 0: Position (12) + Normal (12) = stride 24
        Assert.Equal(2, layouts[0].Attributes.Length);
        Assert.Equal(24ul, layouts[0].Stride);

        // Stream 1: UV0 (8) + Color (16) = stride 24
        Assert.Equal(2, layouts[1].Attributes.Length);
        Assert.Equal(24ul, layouts[1].Stride);
    }

    [Fact]
    public void ToVertexBufferLayouts_EmptyStreams_ReturnsEmptyArray()
    {
        var layouts = MeshManager.ToVertexBufferLayouts([]);

        Assert.Empty(layouts);
    }

    [Fact]
    public void ToVertexBufferLayouts_InstanceStepMode_Preserved()
    {
        var streams = new[]
        {
            new VertexStreamDescriptor([
                new VertexElement(MeshAttribute.Position, 0)
            ], StepMode: VertexStepMode.Instance)
        };

        var layouts = MeshManager.ToVertexBufferLayouts(streams);

        Assert.Single(layouts);
        Assert.Equal(VertexStepMode.Instance, layouts[0].StepMode);
    }

    [Fact]
    public void ToVertexBufferLayouts_DefaultStepMode_IsVertex()
    {
        var streams = new[]
        {
            new VertexStreamDescriptor([
                new VertexElement(MeshAttribute.Position, 0)
            ])
        };

        var layouts = MeshManager.ToVertexBufferLayouts(streams);

        Assert.Single(layouts);
        Assert.Equal(VertexStepMode.Vertex, layouts[0].StepMode);
    }

    [Fact]
    public void MeshData_Validate_ValidData_ReturnsTrue()
    {
        var elements = new VertexElement[]
        {
            new(MeshAttribute.Position, 0),
            new(MeshAttribute.Normal, 1)
        };
        var stride = (int)MeshDescriptor.StreamStride(elements); // 24
        var stream = new byte[24 * 3]; // 3 vertices

        var data = new MeshData(
            new MeshDescriptor([new VertexStreamDescriptor(elements)]),
            [stream],
            new Indices());

        Assert.True(data.TryValidate(out var error));
        Assert.Null(error);
    }

    [Fact]
    public void MeshData_Validate_StreamLengthNotMultipleOfStride_ReturnsFalse()
    {
        var elements = new VertexElement[]
        {
            new(MeshAttribute.Position, 0)
        };
        // stride = 12, but stream length = 13 (not a multiple)
        var stream = new byte[13];

        var data = new MeshData(
            new MeshDescriptor([new VertexStreamDescriptor(elements)]),
            [stream],
            new Indices());

        Assert.False(data.TryValidate(out var error));
        Assert.NotNull(error);
        Assert.Contains("not a multiple of stride", error);
    }

    [Fact]
    public void MeshData_Validate_MismatchedVertexCounts_ReturnsFalse()
    {
        var elements = new VertexElement[]
        {
            new(MeshAttribute.Position, 0)
        };
        var stride = 12;
        // Stream 0: 3 vertices, Stream 1: 2 vertices
        var stream0 = new byte[stride * 3];
        var stream1 = new byte[stride * 2];

        var data = new MeshData(
            new MeshDescriptor([
                new VertexStreamDescriptor([new VertexElement(MeshAttribute.Position, 0)]),
                new VertexStreamDescriptor([new VertexElement(MeshAttribute.Normal, 1)])
            ]),
            [stream0, stream1],
            new Indices());

        Assert.False(data.TryValidate(out var error));
        Assert.NotNull(error);
        Assert.Contains("vertex count", error);
    }

    [Fact]
    public void MeshData_Validate_ZeroStride_ReturnsFalse()
    {
        var data = new MeshData(
            new MeshDescriptor([new VertexStreamDescriptor([])]),
            [Array.Empty<byte>()],
            new Indices());

        Assert.False(data.TryValidate(out var error));
        Assert.NotNull(error);
        Assert.Contains("zero stride", error);
    }

    [Fact]
    public void MeshData_Validate_InstanceStream_SkipsCountCheck()
    {
        // Stream 0: Position (12 bytes) × 3 vertices = 36 bytes
        var posStride = 12;
        var stream0 = new byte[posStride * 3];

        // Stream 1: Color (16 bytes) × 2 instances (different count, skipped due to Instance mode)
        var colorStride = 16;
        var stream1 = new byte[colorStride * 2];

        var data = new MeshData(
            new MeshDescriptor([
                new VertexStreamDescriptor([new VertexElement(MeshAttribute.Position, 0)]),
                new VertexStreamDescriptor([new VertexElement(MeshAttribute.Color, 1)], VertexStepMode.Instance)
            ]),
            [stream0, stream1],
            new Indices());

        Assert.True(data.TryValidate(out var error));
        Assert.Null(error);
    }

    [Fact]
    public void MeshData_Validate_WithIndexData_Validates()
    {
        var elements = new VertexElement[]
        {
            new(MeshAttribute.Position, 0)
        };
        var stride = (int)MeshDescriptor.StreamStride(elements);
        var stream = new byte[stride * 3];

        var indices = new Indices(IndexFormat.Uint32, 3);
        indices.Extend(new uint[] { 0, 1, 2 });

        var data = new MeshData(
            new MeshDescriptor([new VertexStreamDescriptor(elements)]),
            [stream],
            indices);

        Assert.True(data.TryValidate(out var error));
        Assert.Null(error);
    }

    [Fact]
    public void MeshData_VertexCount_WithIndexData_Matches()
    {
        var elements = new VertexElement[]
        {
            new(MeshAttribute.Position, 0)
        };
        var data = MeshData.FromVertices(
            new MeshVertex[]
            {
                new(System.Numerics.Vector3.Zero, System.Numerics.Vector3.UnitY, System.Numerics.Vector3.One),
                new(System.Numerics.Vector3.One, System.Numerics.Vector3.UnitY, System.Numerics.Vector3.One),
            },
            [0u, 1u]);

        Assert.Equal(2, data.VertexCount);
        Assert.Equal(2, data.IndexCount);
        Assert.Equal(IndexFormat.Uint32, data.IndexFormat);
    }

    [Fact]
    public void MeshData_VertexCount_EmptyStreams_ReturnsZero()
    {
        var data = new MeshData(
            new MeshDescriptor([]),
            [],
            new Indices());

        Assert.Equal(0, data.VertexCount);
    }

    [Fact]
    public void MeshData_FromVertices_ProducesValidData()
    {
        var vertices = new MeshVertex[]
        {
            new(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector3(0, 1, 0), new System.Numerics.Vector3(1, 1, 1)),
            new(new System.Numerics.Vector3(1, 0, 0), new System.Numerics.Vector3(0, 1, 0), new System.Numerics.Vector3(1, 1, 1)),
            new(new System.Numerics.Vector3(0, 1, 0), new System.Numerics.Vector3(0, 1, 0), new System.Numerics.Vector3(1, 1, 1)),
        };
        var indices = new uint[] { 0, 1, 2 };

        var data = MeshData.FromVertices(vertices, indices);

        Assert.NotNull(data);
        Assert.Single(data.Streams);
        Assert.Equal(3, data.VertexCount);
        Assert.Equal(3, data.IndexCount);
        Assert.True(data.TryValidate(out _));
    }

    [Fact]
    public void MeshData_CreateQuad_IsValid()
    {
        var quad = MeshData.CreateQuad();

        Assert.NotNull(quad);
        Assert.Equal(4, quad.VertexCount);
        Assert.Equal(6, quad.IndexCount); // 2 triangles
        Assert.True(quad.TryValidate(out var error));
        Assert.Null(error);
    }
}
