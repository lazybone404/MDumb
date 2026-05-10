using System.Runtime.CompilerServices;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public sealed class CameraSyncSystem : ExtractSystemBase
{
    private readonly GraphicsContext _ctx;
    private readonly Dictionary<int, Entity> _buffers = []; // entity ID → GPU buffer

    public CameraSyncSystem(GraphicsContext ctx)
        : base(extractMatcher: Matchers.Of<Engine.Cameras.Camera>())
    {
        _ctx = ctx;
    }

    public Entity GetUniformBuffer(Entity cameraEntity)
    {
        return _buffers.TryGetValue(cameraEntity.Id.Value, out var buffer)
            ? buffer
            : throw new KeyNotFoundException("Camera entity has no uniform buffer.");
    }

    public bool TryGetUniformBuffer(Entity cameraEntity, out Entity buffer)
    {
        return _buffers.TryGetValue(cameraEntity.Id.Value, out buffer);
    }

    public override void Execute(World world, IEntityQuery query, IEntityQuery extract)
    {
        var seenIds = new HashSet<int>();

        extract.ForSlice((Entity entity, ref Engine.Cameras.Camera camera) =>
        {
            var id = entity.Id.Value;
            seenIds.Add(id);

            if (!_buffers.TryGetValue(id, out var buffer))
            {
                buffer = Buffers.Create(_ctx, (ulong)Unsafe.SizeOf<CameraUniforms>(),
                    BufferUsage.Uniform | BufferUsage.CopyDst);
                _buffers[id] = buffer;
            }

            var uniforms = CameraUniforms.From(camera);
            Buffers.Write(_ctx, buffer, uniforms);
        });

        // Release buffers for entities that no longer have a Camera component
        var removedIds = new List<int>();
        foreach (var (id, buffer) in _buffers)
        {
            if (!seenIds.Contains(id))
            {
                Buffers.Release(_ctx, buffer);
                removedIds.Add(id);
            }
        }
        foreach (var id in removedIds)
            _buffers.Remove(id);
    }
}
