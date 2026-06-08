using System.Numerics;

namespace Dumb.Engine.Mesh;

public record struct MeshAabb(Vector3 Min, Vector3 Max)
{
    public readonly Vector3 Center => (Min + Max) * 0.5f;
    public readonly Vector3 HalfExtents => (Max - Min) * 0.5f;

    public static MeshAabb FromPositions(IReadOnlyList<Vector3> positions)
    {
        if (positions.Count == 0)
            return new MeshAabb(Vector3.Zero, Vector3.Zero);

        var min = positions[0];
        var max = positions[0];
        for (var i = 1; i < positions.Count; i++)
        {
            min = Vector3.Min(min, positions[i]);
            max = Vector3.Max(max, positions[i]);
        }
        return new MeshAabb(min, max);
    }

    public MeshAabb Merge(MeshAabb other) =>
        new(Vector3.Min(Min, other.Min), Vector3.Max(Max, other.Max));
}
