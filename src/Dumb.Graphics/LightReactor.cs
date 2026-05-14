using System.Numerics;
using System.Runtime.InteropServices;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GPULight
{
    public Vector3 Color;
    public float Intensity;
    public Vector3 Direction;
    public float Range;
    public Vector3 Position;
    public uint Type;
    public float InnerConeAngle;
    public float OuterConeAngle;
    public float _pad0;
    public float _pad1;

    public const uint MaxLights = 64;
    public const int Size = 80;

    public static GPULight From(in Engine.Lighting.Light light, Vector3 position)
    {
        return new GPULight
        {
            Color = light.Color,
            Intensity = light.Intensity,
            Direction = light.Direction,
            Range = light.Range,
            Position = position,
            Type = (uint)light.Type,
            InnerConeAngle = light.InnerConeAngle,
            OuterConeAngle = light.OuterConeAngle
        };
    }
}

public sealed class LightSyncSystem : ExtractSystemBase
{
    private readonly GraphicsContext _ctx;
    private readonly Dictionary<int, int> _entityToSlot = [];
    private Entity _lightBuffer;
    private int _lightCount;

    public Entity LightBuffer => _lightBuffer;
    public int LightCount => _lightCount;

    public LightSyncSystem(GraphicsContext ctx)
        : base(Matchers.Any, extractMatcher: Matchers.Of<Engine.Lighting.Light>())
    {
        _ctx = ctx;
    }

    public override void Execute(World world, IEntityQuery query, IEntityQuery extract)
    {
        var seenIds = new HashSet<int>();
        _lightCount = 0;

        var lightData = new byte[GPULight.MaxLights * GPULight.Size];

        extract.ForSlice((Entity entity, ref Engine.Lighting.Light light) =>
        {
            var id = entity.Id.Value;
            seenIds.Add(id);

            if (!_entityToSlot.TryGetValue(id, out var slot))
            {
                slot = _entityToSlot.Count;
                _entityToSlot[id] = slot;
            }
            if (slot >= GPULight.MaxLights)
                return;

            // Resolve world position from transform if entity has one
            var position = entity.Contains<Engine.Transform.LocalTransform>()
                ? entity.Get<Engine.Transform.LocalTransform>().Position
                : Vector3.Zero;

            var gpuLight = GPULight.From(light, position);
            MemoryMarshal.Write(lightData.AsSpan(slot * GPULight.Size), gpuLight);
            _lightCount = Math.Max(_lightCount, slot + 1);
        });

        // Resize buffer if needed
        if (_lightBuffer?.Host == null)
        {
            _lightBuffer = Buffers.Create(_ctx,
                (ulong)(GPULight.MaxLights * GPULight.Size),
                BufferUsage.Uniform | BufferUsage.CopyDst);
        }

        Buffers.Write(_ctx, _lightBuffer!, 0, lightData);

        // Clean up removed entities
        var removed = _entityToSlot.Keys
            .Where(k => !seenIds.Contains(k))
            .ToList();
        foreach (var id in removed)
            _entityToSlot.Remove(id);
    }
}
