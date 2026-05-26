using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics.Material;

[StructLayout(LayoutKind.Sequential, Size = 80)]
public struct PBRMaterialParameters
{
    public Vector3 Albedo;              // offset 0,  WGSL: albedo: vec3f
    public float _pad0;                 // offset 12, WGSL: _pad0: f32
    public float Metallic;              // offset 16, WGSL: metallic: f32
    public float Roughness;             // offset 20, WGSL: roughness: f32
    public float AmbientOcclusion;      // offset 24, WGSL: ambient_occlusion: f32
    public float _wgpuAlign0;           // offset 28, WGSL implicit padding (vec3f align 16)
    public Vector3 Emissive;            // offset 32, WGSL: emissive: vec3f
    public float _pad1;                 // offset 44, WGSL: _pad1: f32
    public float _pad;                  // offset 48, WGSL: _pad: f32
    public float _wgpuAlign1;           // offset 52, WGSL implicit padding (vec4f align 16)
    public float _wgpuAlign2;           // offset 56
    public float _wgpuAlign3;           // offset 60
    public float _padEnd0;              // offset 64, WGSL: _padEnd: vec4f
    public float _padEnd1;              // offset 68
    public float _padEnd2;              // offset 72
    public float _padEnd3;              // offset 76

    public static readonly PBRMaterialParameters Default = new()
    {
        Albedo = new Vector3(0.8f, 0.8f, 0.8f),
        Metallic = 0.0f,
        Roughness = 0.5f,
        AmbientOcclusion = 1.0f,
        Emissive = Vector3.Zero
    };
}

public struct PBRMaterial : IMaterial
{
    public PBRMaterialParameters Parameters;
    public Entity? AlbedoTexture;
    public Entity? NormalTexture;
    public Entity? MetallicRoughnessTexture;
    public Entity? AOTexture;
    public Entity? EmissiveTexture;
    public Entity? Sampler;

    private Entity? _cachedShader;

    public static string Name => "PBR";

    public static Engine.Mesh.MeshDescriptor VertexDescriptor => new(
        [new Engine.Mesh.VertexStreamDescriptor([
            new Engine.Mesh.VertexElement(Engine.Mesh.MeshAttribute.Position, location: 0),
            new Engine.Mesh.VertexElement(Engine.Mesh.MeshAttribute.Normal, location: 1),
            new Engine.Mesh.VertexElement(Engine.Mesh.MeshAttribute.UV0, location: 2),
            new Engine.Mesh.VertexElement(Engine.Mesh.MeshAttribute.Tangent, location: 3),
            new Engine.Mesh.VertexElement(Engine.Mesh.MeshAttribute.Color, location: 4)
        ])],
        IndexFormat.Uint32);

    private static readonly BindingLayout[][] s_bindGroupLayouts =
    [
        // Group 0: Frame (camera + model — provided by renderer)
        [
            BindingLayout.UniformBuffer(0, ShaderStage.Vertex, 336),
            BindingLayout.UniformBuffer(2, ShaderStage.Vertex, 64, hasDynamicOffset: true)
        ],
        // Group 1: Material
        [
            BindingLayout.UniformBuffer(0, ShaderStage.Fragment, (ulong)Unsafe.SizeOf<PBRMaterialParameters>()),
            BindingLayout.Texture(1, ShaderStage.Fragment),
            BindingLayout.Texture(2, ShaderStage.Fragment),
            BindingLayout.Texture(3, ShaderStage.Fragment),
            BindingLayout.Texture(4, ShaderStage.Fragment),
            BindingLayout.Texture(5, ShaderStage.Fragment),
            BindingLayout.Sampler(6, ShaderStage.Fragment)
        ]
    ];

    public static BindingLayout[][] BindGroupLayouts => s_bindGroupLayouts;

    public static BlendState? Blend => null;

    public static TextureFormat[] ColorFormats =>
    [
        TextureFormat.Rgba8Unorm,
        TextureFormat.Rgba16float,
        TextureFormat.Rgba8Unorm
    ];

    public static DepthStencilState? DepthStencil => new()
    {
        Format = TextureFormat.Depth32float,
        DepthWriteEnabled = true,
        DepthCompare = CompareFunction.Less,
        StencilFront = new StencilFaceState { Compare = CompareFunction.Always, FailOp = StencilOperation.Keep, DepthFailOp = StencilOperation.Keep, PassOp = StencilOperation.Keep },
        StencilBack = new StencilFaceState { Compare = CompareFunction.Always, FailOp = StencilOperation.Keep, DepthFailOp = StencilOperation.Keep, PassOp = StencilOperation.Keep }
    };

    public Entity GetShader(GraphicsContext ctx)
    {
        if (_cachedShader is { Host: not null } s)
            return s;

        _cachedShader = Shaders.Wgsl(ctx, GBufferVertexShader + GBufferFragmentShader);
        return _cachedShader!;
    }

    public Entity?[] CreateBindGroups(GraphicsContext ctx, Entity pipelineLayout)
    {
        var materialUniform = Buffers.Uniform(ctx, Parameters);

        var albedoTex = AlbedoTexture ?? ctx._textures.First();
        var normalTex = NormalTexture ?? ctx._textures.First();
        var mrTex = MetallicRoughnessTexture ?? ctx._textures.First();
        var aoTex = AOTexture ?? ctx._textures.First();
        var emissiveTex = EmissiveTexture ?? ctx._textures.First();
        var sampler = Sampler ?? Samplers.LinearClamp(ctx);

        // Group 1: Material bindings
        ref var plData = ref pipelineLayout.Get<PipelineLayoutData>();
        var bgl1 = plData.BindGroupLayouts?[1]
            ?? throw new InvalidOperationException("Material requires bind group layout at index 1.");

        var group1 = Pipelines.BindGroup(ctx, bgl1,
        [
            Binding.Uniform<PBRMaterialParameters>(0, materialUniform),
            Binding.Texture(1, albedoTex),
            Binding.Texture(2, normalTex),
            Binding.Texture(3, mrTex),
            Binding.Texture(4, aoTex),
            Binding.Texture(5, emissiveTex),
            Binding.Sampler(6, sampler)
        ]);

        return [null, group1];
    }

    // ── WGSL Shaders ────────────────────────────────────────────

    public const string GBufferVertexShader = @"
struct CameraUniforms {
    view_projection: mat4x4f,
    view: mat4x4f,
    projection: mat4x4f,
    camera_position: vec3f,
    _pad0: f32,
    view_inverse: mat4x4f,
    projection_inverse: mat4x4f,
}

struct VSInput {
    @location(0) position: vec3f,
    @location(1) normal: vec3f,
    @location(2) uv: vec2f,
    @location(3) tangent: vec4f,
    @location(4) color: vec4f,
}

struct VSOutput {
    @builtin(position) clip_position: vec4f,
    @location(0) world_position: vec3f,
    @location(1) world_normal: vec3f,
    @location(2) uv: vec2f,
    @location(3) color: vec3f,
}

@group(0) @binding(0) var<uniform> camera: CameraUniforms;
@group(0) @binding(2) var<uniform> model: mat4x4f;

@vertex
fn vs_main(in: VSInput) -> VSOutput {
    var out: VSOutput;
    let world_pos = (model * vec4f(in.position, 1.0)).xyz;
    let world_norm = mat3x3f(model[0].xyz, model[1].xyz, model[2].xyz) * in.normal;
    out.world_position = world_pos;
    out.world_normal = normalize(world_norm);
    out.uv = in.uv;
    out.color = in.color.rgb;
    out.clip_position = camera.view_projection * vec4f(world_pos, 1.0);
    return out;
}
";

    public const string GBufferFragmentShader = @"
struct MaterialUniforms {
    albedo: vec3f,
    _pad0: f32,
    metallic: f32,
    roughness: f32,
    ambient_occlusion: f32,
    emissive: vec3f,
    _pad1: f32,
    _pad: f32,
    _padEnd: vec4f,
}

struct FSInput {
    @location(0) world_position: vec3f,
    @location(1) world_normal: vec3f,
    @location(2) uv: vec2f,
    @location(3) color: vec3f,
}

struct GBufferOutput {
    @location(0) albedo: vec4f,
    @location(1) normal_roughness: vec4f,
    @location(2) pbr: vec4f,
}

@group(1) @binding(0) var<uniform> material: MaterialUniforms;
@group(1) @binding(1) var albedo_tex: texture_2d<f32>;
@group(1) @binding(2) var normal_tex: texture_2d<f32>;
@group(1) @binding(3) var metallic_roughness_tex: texture_2d<f32>;
@group(1) @binding(4) var ao_tex: texture_2d<f32>;
@group(1) @binding(5) var emissive_tex: texture_2d<f32>;
@group(1) @binding(6) var tex_sampler: sampler;

fn octahedron_encode(v: vec3f) -> vec2f {
    let l1norm = abs(v.x) + abs(v.y) + abs(v.z);
    var result = v.xy * (1.0 / l1norm);
    if v.z < 0.0 {
        let old_x = result.x;
        result.x = (1.0 - abs(result.y)) * sign(old_x);
        result.y = (1.0 - abs(old_x)) * sign(result.y);
    }
    return result * 0.5 + 0.5;
}

@fragment
fn fs_main(in: FSInput) -> GBufferOutput {
    let albedo_sample = textureSample(albedo_tex, tex_sampler, in.uv).rgb;
    let mr_sample = textureSample(metallic_roughness_tex, tex_sampler, in.uv);
    let ao_sample = textureSample(ao_tex, tex_sampler, in.uv).r;
    let emissive_sample = textureSample(emissive_tex, tex_sampler, in.uv).rgb;

    let albedo_color = albedo_sample * in.color * material.albedo;
    let metallic = mr_sample.b * material.metallic;
    let roughness = mr_sample.g * material.roughness;
    let ao = ao_sample * material.ambient_occlusion;
    let emissive = emissive_sample + material.emissive;

    let N = normalize(in.world_normal);
    let encoded_normal = octahedron_encode(N);

    var out: GBufferOutput;
    out.albedo = vec4f(albedo_color, 1.0);
    out.normal_roughness = vec4f(encoded_normal, roughness, 0.0);
    out.pbr = vec4f(metallic, ao, emissive.r, 1.0);
    return out;
}
";
}
