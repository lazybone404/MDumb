using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public sealed class TransformSyncSystem : ExtractSystemBase
{
    private readonly GraphicsContext _ctx;
    private readonly Dictionary<int, int> _entityToSlot = [];  // entity ID → slot index
    private Entity _modelBuffer;
    private int _slotCount;
    private int _capacity;

    public const int SlotByteSize = 256;
    private const int InitialSlots = 64;   // pre-allocate to avoid rebinding

    public TransformSyncSystem(GraphicsContext ctx)
        : base(Matchers.Any, extractMatcher: Matchers.Of<Engine.Transform.GlobalTransform>())
    {
        _ctx = ctx;
    }

    public Entity ModelBuffer => _modelBuffer;

    public uint GetOffset(Entity entity)
    {
        if (_entityToSlot.TryGetValue(entity.Id.Value, out var slot))
            return (uint)(slot * SlotByteSize);
        return 0;
    }

    public bool TryGetOffset(Entity entity, out uint offset)
    {
        if (_entityToSlot.TryGetValue(entity.Id.Value, out var slot))
        {
            offset = (uint)(slot * SlotByteSize);
            return true;
        }
        offset = 0;
        return false;
    }

    public override void Execute(World world, IEntityQuery query, IEntityQuery extract)
    {
        var neededSlots = 0;
        var seenIds = new HashSet<int>();

        // Assign slots to new entities
        extract.ForSlice((Entity entity, ref Engine.Transform.GlobalTransform gt) =>
        {
            var id = entity.Id.Value;
            seenIds.Add(id);
            if (!_entityToSlot.TryGetValue(id, out var slot))
            {
                slot = _entityToSlot.Count;
                _entityToSlot[id] = slot;
            }
            if (slot + 1 > neededSlots)
                neededSlots = slot + 1;
        });

        // Allocate buffer once, grow only if needed (avoids bind group invalidation)
        if (_modelBuffer?.Host == null)
        {
            _capacity = Math.Max(neededSlots, InitialSlots);
            _modelBuffer = Buffers.Create(_ctx,
                (ulong)(_capacity * SlotByteSize),
                BufferUsage.Uniform | BufferUsage.CopyDst);
        }
        else if (neededSlots > _capacity)
        {
            Buffers.Release(_ctx, _modelBuffer!);
            _capacity = neededSlots;
            _modelBuffer = Buffers.Create(_ctx,
                (ulong)(_capacity * SlotByteSize),
                BufferUsage.Uniform | BufferUsage.CopyDst);
        }

        _slotCount = neededSlots;

        // Write model matrices into aligned slots
        var modelData = new byte[_capacity * SlotByteSize];
        extract.ForSlice((Entity entity, ref Engine.Transform.GlobalTransform gt) =>
        {
            var slot = _entityToSlot[entity.Id.Value];
            var mat = gt.Value.ToMatrix4x4();
            MemoryMarshal.Write(modelData.AsSpan(slot * SlotByteSize), mat);
        });
        Buffers.Write(_ctx, _modelBuffer!, 0, modelData);

        // Clean up removed entities
        var removed = _entityToSlot.Keys
            .Where(k => !seenIds.Contains(k))
            .ToList();
        foreach (var id in removed)
            _entityToSlot.Remove(id);
    }
}
