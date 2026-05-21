using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics.Rendering.Material;

public struct DeferredLightingMaterial : IMaterial
{
    public Entity GBufferRT0;
    public Entity GBufferRT1;
    public Entity GBufferRT2;
    public Entity GBufferDepth;
    public Entity Sampler;
    public Entity CameraBuffer;
    public Entity LightBuffer;

    private Entity? _cachedShader;

    public static string Name => "DeferredLighting";

    public static Engine.Mesh.MeshDescriptor VertexDescriptor => new([]);

    private static readonly BindingLayout[][] s_bindGroupLayouts =
    [
        // Group 0: Frame
        [
            BindingLayout.UniformBuffer(0, ShaderStage.Fragment, 336),     // CameraUniforms
        ],
        // Group 1: G-buffer textures
        [
            BindingLayout.Sampler(0, ShaderStage.Fragment),
            BindingLayout.Texture(1, ShaderStage.Fragment),
            BindingLayout.Texture(2, ShaderStage.Fragment),
            BindingLayout.Texture(3, ShaderStage.Fragment),
            BindingLayout.Texture(4, ShaderStage.Fragment),
        ],
        // Group 2: Lights
        [
            BindingLayout.UniformBuffer(0, ShaderStage.Fragment, (ulong)(GPULight.MaxLights * GPULight.Size)),
        ]
    ];

    public static BindingLayout[][] BindGroupLayouts => s_bindGroupLayouts;

    public static TextureFormat[] ColorFormats => [TextureFormat.Rgba8Unorm];

    public Entity GetShader(GraphicsContext ctx)
    {
        if (_cachedShader is { Host: not null } s)
            return s;

        _cachedShader = Shaders.Wgsl(ctx, FullScreenVertexShader + LightingFragmentShader);
        return _cachedShader!;
    }

    public Entity?[] CreateBindGroups(GraphicsContext ctx, Entity pipelineLayout)
    {
        ref var plData = ref pipelineLayout.Get<PipelineLayoutData>();
        var bgl1 = plData.BindGroupLayouts?[1]
            ?? throw new InvalidOperationException("Pipeline layout missing bind group layout 1.");
        var bgl2 = plData.BindGroupLayouts?[2]
            ?? throw new InvalidOperationException("Pipeline layout missing bind group layout 2.");

        var group1 = Pipelines.BindGroup(ctx, bgl1,
        [
            Binding.Sampler(0, Sampler),
            Binding.Texture(1, GBufferRT0),
            Binding.Texture(2, GBufferRT1),
            Binding.Texture(3, GBufferRT2),
            Binding.Texture(4, GBufferDepth),
        ]);

        var group2 = Pipelines.BindGroup(ctx, bgl2,
        [
            Binding.Uniform<GPULight>(0, LightBuffer),
        ]);

        return [null, group1, group2];
    }

    private const string FullScreenVertexShader = """
        @vertex
        fn vs_main(@builtin(vertex_index) vertex_index: u32) -> @builtin(position) vec4f {
            let x = f32(i32(vertex_index & 1u) * 2 - 1);
            let y = f32(i32(vertex_index >> 1u) * 2 - 1);
            return vec4f(x, y, 0.0, 1.0);
        }
        """;

    private const string LightingFragmentShader = """
        struct CameraUniforms {
            view_projection: mat4x4f,
            view: mat4x4f,
            projection: mat4x4f,
            camera_position: vec3f,
            _pad0: f32,
            view_inverse: mat4x4f,
            projection_inverse: mat4x4f,
        }

        struct LightData {
            color: vec3f,
            intensity: f32,
            direction: vec3f,
            range: f32,
            position: vec3f,
            type: u32,
            inner_cone: f32,
            outer_cone: f32,
        }

        @group(0) @binding(0) var<uniform> camera: CameraUniforms;

        @group(1) @binding(0) var gbuffer_sampler: sampler;
        @group(1) @binding(1) var albedo_tex: texture_2d<f32>;
        @group(1) @binding(2) var normal_roughness_tex: texture_2d<f32>;
        @group(1) @binding(3) var pbr_tex: texture_2d<f32>;
        @group(1) @binding(4) var depth_tex: texture_depth_2d;

        @group(2) @binding(0) var<uniform> lights: array<LightData, 64>;

        const LIGHT_DIRECTIONAL = 0u;
        const LIGHT_POINT = 1u;
        const LIGHT_SPOT = 2u;

        fn octahedron_decode(v: vec2f) -> vec3f {
            var n = v * 2.0 - 1.0;
            let abs_sum = abs(n.x) + abs(n.y);
            var result: vec3f;
            if abs_sum <= 1.0 {
                result.z = 1.0 - abs_sum;
                result.xy = n;
            } else {
                result.z = abs_sum - 1.0;
                result.x = n.x >= 0.0 ? 1.0 - abs(n.y) : abs(n.y) - 1.0;
                result.y = n.y >= 0.0 ? 1.0 - abs(n.x) : abs(n.x) - 1.0;
            }
            return normalize(result);
        }

        fn specular_brdf(N: vec3f, V: vec3f, L: vec3f, roughness: f32, metallic: f32, albedo: vec3f) -> vec3f {
            let H = normalize(L + V);
            let NdotL = max(dot(N, L), 0.0);
            let NdotV = max(dot(N, V), 0.001);
            let NdotH = max(dot(N, H), 0.0);
            let VdotH = max(dot(V, H), 0.0);

            let alpha = roughness * roughness;
            let alpha2 = alpha * alpha;

            let denom = NdotH * NdotH * (alpha2 - 1.0) + 1.0;
            let D = alpha2 / (3.14159265 * denom * denom);

            let k = (roughness + 1.0) * (roughness + 1.0) / 8.0;
            let G1 = NdotL / (NdotL * (1.0 - k) + k);
            let G2 = NdotV / (NdotV * (1.0 - k) + k);
            let G = G1 * G2;

            let F0 = mix(vec3f(0.04), albedo, metallic);
            let F = F0 + (1.0 - F0) * pow(1.0 - VdotH, 5.0);

            let specular = D * G * F / max(4.0 * NdotV * NdotL, 0.001);
            let diffuse = albedo * (1.0 - F0) * (1.0 - metallic) / 3.14159265;

            return (diffuse + specular) * NdotL;
        }

        fn evaluate_directional(light: LightData, N: vec3f, V: vec3f, roughness: f32, metallic: f32, albedo: vec3f) -> vec3f {
            let L = normalize(-light.direction);
            return light.color * light.intensity * specular_brdf(N, V, L, roughness, metallic, albedo);
        }

        fn evaluate_point(light: LightData, world_pos: vec3f, N: vec3f, V: vec3f, roughness: f32, metallic: f32, albedo: vec3f) -> vec3f {
            let L_vec = light.position - world_pos;
            let dist = length(L_vec);
            if dist > light.range { return vec3f(0.0); }
            let attenuation = pow(max(1.0 - dist / light.range, 0.0), 2.0);
            let L = L_vec / dist;
            return light.color * light.intensity * attenuation * specular_brdf(N, V, L, roughness, metallic, albedo);
        }

        fn evaluate_spot(light: LightData, world_pos: vec3f, N: vec3f, V: vec3f, roughness: f32, metallic: f32, albedo: vec3f) -> vec3f {
            let L_vec = light.position - world_pos;
            let dist = length(L_vec);
            if dist > light.range { return vec3f(0.0); }
            let attenuation = pow(max(1.0 - dist / light.range, 0.0), 2.0);
            let L = L_vec / dist;
            let theta = dot(L, normalize(-light.direction));
            let epsilon = light.inner_cone - light.outer_cone;
            let spot_falloff = clamp((theta - light.outer_cone) / max(epsilon, 0.001), 0.0, 1.0);
            return light.color * light.intensity * attenuation * spot_falloff * specular_brdf(N, V, L, roughness, metallic, albedo);
        }

        @fragment
        fn fs_main(@builtin(position) screen_pos: vec4f) -> @location(0) vec4f {
            let uv = screen_pos.xy / vec2f(textureDimensions(albedo_tex, 0));
            let albedo_sample = textureSample(albedo_tex, gbuffer_sampler, uv);
            let nr_sample = textureSample(normal_roughness_tex, gbuffer_sampler, uv);
            let pbr_sample = textureSample(pbr_tex, gbuffer_sampler, uv);
            let depth = textureSample(depth_tex, gbuffer_sampler, uv);

            let albedo = albedo_sample.rgb;
            let normal = octahedron_decode(nr_sample.rg);
            let roughness = nr_sample.b;
            let metallic = pbr_sample.r;
            let ao = pbr_sample.g;
            let emissive = pbr_sample.b * albedo;

            // Reconstruct world position from depth
            let clip = vec4f(uv * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0);
            let view_space = camera.projection_inverse * clip;
            let view_pos = view_space.xyz / view_space.w;
            let world_pos = (camera.view_inverse * vec4f(view_pos, 1.0)).xyz;
            let V = normalize(camera.camera_position - world_pos);

            var color = vec3f(0.0);
            // Ambient term
            color += albedo * 0.03 * ao;

            for (var i = 0u; i < 64u; i++) {
                let light = lights[i];
                if light.intensity <= 0.0 { continue; }

                if light.type == LIGHT_DIRECTIONAL {
                    color += evaluate_directional(light, normal, V, roughness, metallic, albedo);
                } else if light.type == LIGHT_POINT {
                    color += evaluate_point(light, world_pos, normal, V, roughness, metallic, albedo);
                } else if light.type == LIGHT_SPOT {
                    color += evaluate_spot(light, world_pos, normal, V, roughness, metallic, albedo);
                }
            }

            color += emissive;
            return vec4f(color, 1.0);
        }
        """;
}
