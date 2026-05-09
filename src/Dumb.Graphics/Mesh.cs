using System.Threading;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public static class Mesh
{
    public static Entity Create(GraphicsContext ctx, Engine.Mesh.MeshData data)
    {
        var vertexBuffers = new Entity[data.Streams.Length];
        for (var i = 0; i < data.Streams.Length; i++)
        {
            var stream = data.Streams[i] ?? throw new InvalidOperationException("Mesh stream is null");
            var buffer = Buffers.Create(ctx, (ulong)stream.Length,
                BufferUsage.Vertex | BufferUsage.CopyDst);
            Buffers.Write(ctx, buffer, 0, stream);
            vertexBuffers[i] = buffer;
        }

        Entity indexBuffer = null!;
        if (data.Indices is { Length: > 0 })
        {
            indexBuffer = Buffers.Create(ctx, (ulong)data.Indices.Length,
                BufferUsage.Index | BufferUsage.CopyDst);
            Buffers.Write(ctx, indexBuffer, 0, data.Indices);
        }

        var subMeshes = data.SubMeshes;
        if (subMeshes.Length == 0)
        {
            subMeshes =
            [
                new Engine.Mesh.SubMesh
                {
                    IndexStart = 0,
                    IndexCount = (uint)data.IndexCount,
                    VertexStart = 0,
                    VertexCount = (uint)data.VertexCount,
                    Topology = PrimitiveTopology.TriangleList
                }
            ];
        }

        return ctx._meshes.Create(HList.From(new MeshResourceData
        {
            VertexBuffers = vertexBuffers,
            VertexCounts = vertexBuffers.Select(_ => data.VertexCount).ToArray(),
            IndexBuffer = indexBuffer,
            IndexFormat = data.IndexFormat,
            IndexCount = (uint)data.IndexCount,
            SubMeshes = subMeshes,
            RefCount = 1
        }));
    }

    internal static void Retain(GraphicsContext ctx, Entity mesh)
    {
        ref var data = ref mesh.Get<MeshResourceData>();
        Interlocked.Increment(ref data.RefCount);
    }

    internal static void Release(GraphicsContext ctx, Entity mesh)
    {
        ref var data = ref mesh.Get<MeshResourceData>();
        if (Interlocked.Decrement(ref data.RefCount) == 0)
        {
            foreach (var vb in data.VertexBuffers)
            {
                if (vb.Host != null)
                    Buffers.Release(ctx, vb);
            }
            if (data.IndexBuffer.Host != null)
                Buffers.Release(ctx, data.IndexBuffer);
            mesh.Destroy();
        }
    }

    public static void Draw(RenderPass pass, Entity mesh, uint subMeshIndex = 0)
    {
        ref var data = ref mesh.Get<MeshResourceData>();
        var sm = data.SubMeshes[subMeshIndex];

        for (var i = 0; i < data.VertexBuffers.Length; i++)
            pass.SetVertexBuffer((uint)i, data.VertexBuffers[i]);

        pass.SetIndexBuffer(data.IndexBuffer, data.IndexFormat);
        pass.DrawIndexed(sm.IndexCount, 1, sm.IndexStart, (int)sm.VertexStart);
    }

    public static VertexAttributeLayout[] ToVertexAttributeLayouts(Engine.Mesh.MeshAttribute[] attributes)
    {
        var layouts = new VertexAttributeLayout[attributes.Length];
        ulong offset = 0;
        for (var i = 0; i < attributes.Length; i++)
        {
            var format = Engine.Mesh.MeshDescriptor.GetVertexFormat(attributes[i]);
            var size = Engine.Mesh.MeshDescriptor.AttributeSize(attributes[i]);
            layouts[i] = new VertexAttributeLayout((uint)i, format, offset);
            offset += size;
        }
        return layouts;
    }
}
