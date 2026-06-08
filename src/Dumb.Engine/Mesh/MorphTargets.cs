using System.Numerics;

namespace Dumb.Engine.Mesh;

public record struct MorphAttributes(Vector3 Position, Vector3 Normal, Vector3 Tangent)
{
    public const int ComponentCount = 9;
}

public sealed class MorphWeights(float[] weights)
{
    public float[] Weights { get; } = weights.Length > MaxMorphWeights
        ? throw new ArgumentException($"Too many morph targets: {weights.Length} (max {MaxMorphWeights})")
        : weights;

    public const int MaxMorphWeights = 64;
}

public sealed class MeshMorphWeights(float[] weights)
{
    public float[] Weights { get; private set; } = weights.Length > MorphWeights.MaxMorphWeights
        ? throw new ArgumentException($"Too many morph targets: {weights.Length} (max {MorphWeights.MaxMorphWeights})")
        : weights;

    public void Extend(float[] weights)
    {
        if (Weights.Length + weights.Length > MorphWeights.MaxMorphWeights)
            throw new ArgumentException(
                $"Extend would result in {Weights.Length + weights.Length} morph weights (max {MorphWeights.MaxMorphWeights})",
                nameof(weights));
        var combined = new float[Weights.Length + weights.Length];
        Array.Copy(Weights, combined, Weights.Length);
        Array.Copy(weights, 0, combined, Weights.Length, weights.Length);
        Weights = combined;
    }

    public void Clear()
    {
        Weights = [];
    }
}

public sealed class MorphTargetImage
{
    public float[] Data { get; }
    public int Width { get; }
    public int Height { get; }
    public int TargetCount { get; }

    public const int MaxTextureWidth = 2048;

    public MorphTargetImage(
        MorphAttributes[][] targets,
        int vertexCount)
    {
        TargetCount = targets.Length;
        if (TargetCount > MorphWeights.MaxMorphWeights)
            throw new ArgumentException($"Too many morph targets: {TargetCount}");

        var componentCount = (uint)(vertexCount * MorphAttributes.ComponentCount);
        var (width, height) = FindOptimalDimensions(componentCount, MaxTextureWidth);

        Width = (int)width;
        Height = (int)height;

        var paddedComponents = Width * Height;
        Data = new float[paddedComponents * TargetCount];

        for (var t = 0; t < TargetCount; t++)
        {
            var layerOff = t * paddedComponents;
            var attrs = targets[t];

            for (var v = 0; v < vertexCount && v < attrs.Length; v++)
            {
                var off = layerOff + v * MorphAttributes.ComponentCount;
                Data[off] = attrs[v].Position.X;
                Data[off + 1] = attrs[v].Position.Y;
                Data[off + 2] = attrs[v].Position.Z;
                Data[off + 3] = attrs[v].Normal.X;
                Data[off + 4] = attrs[v].Normal.Y;
                Data[off + 5] = attrs[v].Normal.Z;
                Data[off + 6] = attrs[v].Tangent.X;
                Data[off + 7] = attrs[v].Tangent.Y;
                Data[off + 8] = attrs[v].Tangent.Z;
            }
        }
    }

    private static (uint width, uint height) FindOptimalDimensions(uint minCells, uint maxEdge)
    {
        uint bestW = 0, bestH = 0, bestPad = uint.MaxValue;

        for (uint w = 1; w <= maxEdge; w++)
        {
            var h = (minCells + w - 1) / w;
            if (h > maxEdge) continue;

            var pad = w * h - minCells;
            if (pad < bestPad)
            {
                bestPad = pad;
                bestW = w;
                bestH = h;
            }
        }

        return bestW > 0 ? (bestW, bestH) : (maxEdge, maxEdge);
    }
}
