using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public static class Skinning
{
    public const int MaxBones = 72;

    public const string Wgsl = """
        struct SkinnedVertex {
            position: vec3f,
            normal: vec3f,
        }

        fn skin_vertex(
            position: vec3f,
            normal: vec3f,
            weights: vec4f,
            indices: vec4u,
            bone_matrices: array<mat4x4f, 72>
        ) -> SkinnedVertex {
            var result: SkinnedVertex;
            result.position = vec3f(0.0);
            result.normal = vec3f(0.0);
            for (var i = 0u; i < 4u; i++) {
                let w = weights[i];
                if w > 0.0 {
                    let m = bone_matrices[indices[i]];
                    result.position += (m * vec4f(position, 1.0)).xyz * w;
                    result.normal += mat3x3f(m) * normal * w;
                }
            }
            result.normal = normalize(result.normal);
            return result;
        }
        """;

    public static Entity CreateBoneBuffer(GraphicsContext ctx, ReadOnlySpan<Matrix4x4> bindPose)
    {
        var size = (ulong)(bindPose.Length * sizeof(float) * 16);
        var buffer = Buffers.Create(ctx, size,
            BufferUsage.Storage | BufferUsage.CopyDst);

        Buffers.Write(ctx, buffer, 0, MemoryMarshal.AsBytes(bindPose));
        return buffer;
    }

    public static void UpdateBoneBuffer(GraphicsContext ctx, Entity buffer, ReadOnlySpan<Matrix4x4> matrices)
    {
        Buffers.Write(ctx, buffer, 0, MemoryMarshal.AsBytes(matrices));
    }
}
