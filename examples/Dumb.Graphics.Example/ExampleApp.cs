using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sia;
using Silk.NET.WebGPU;
#if BROWSER
using System.Text;
using Dumb.Emscripten;
using Dawn = Silk.NET.WebGPU.Extensions.Dawn;
#else
using Dumb.Engine.Window;
#endif

namespace Dumb.Graphics.Example;

[StructLayout(LayoutKind.Sequential)]
internal struct ExampleVertex(float x, float y, float r, float g, float b)
{
    public Vector2 Position = new(x, y);
    public Vector3 Color = new(r, g, b);
}

[StructLayout(LayoutKind.Sequential)]
internal struct ExampleUniforms
{
    public Vector2 TargetSize;
    public float Time;
    public float Pad;
}

public sealed class GraphicsExampleApp : IDisposable
{
    private const uint OffscreenWidth = 512;
    private const uint OffscreenHeight = 512;
    private const int NativeWidth = 960;
    private const int NativeHeight = 640;
    private const TextureFormat ColorFormat = TextureFormat.Rgba8Unorm;

    private const string TriangleShader = """
struct Uniforms {
    target_size: vec2f,
    time: f32,
    pad: f32,
}

@group(0) @binding(0) var<uniform> u: Uniforms;
@group(0) @binding(1) var<storage, read> compute_data: array<f32>;

struct VertexInput {
    @location(0) position: vec2f,
    @location(1) color: vec3f,
}

struct VertexOutput {
    @builtin(position) position: vec4f,
    @location(0) color: vec3f,
    @location(1) uv: vec2f,
}

@vertex
fn vs_main(input: VertexInput) -> VertexOutput {
    var output: VertexOutput;
    output.position = vec4f(input.position, 0.0, 1.0);
    output.color = input.color;
    output.uv = input.position * 0.5 + vec2f(0.5, 0.5);
    return output;
}

@fragment
fn fs_main(input: VertexOutput) -> @location(0) vec4f {
    let pulse = compute_data[0];
    let radial = distance(input.uv, vec2f(0.5, 0.5));
    let rings = sin(radial * 52.0 - u.time * 5.0) * 0.5 + 0.5;
    let beam = pow(1.0 - abs(input.uv.x - input.uv.y), 3.0);
    let glow = smoothstep(0.68, 0.05, radial);
    let base = mix(vec3f(0.035, 0.055, 0.085), input.color, 0.72);
    let color = base * (0.62 + rings * 0.28) + vec3f(0.15, 0.26, 0.34) * pulse + vec3f(0.20, 0.42, 0.36) * beam * glow;
    return vec4f(color, 1.0);
}
""";

    private const string TextureShader = """
struct Uniforms {
    target_size: vec2f,
    time: f32,
    pad: f32,
}

@group(0) @binding(0) var render_sampler: sampler;
@group(0) @binding(1) var render_texture: texture_2d<f32>;
@group(0) @binding(2) var<uniform> u: Uniforms;

struct VertexOutput {
    @builtin(position) position: vec4f,
    @location(0) uv: vec2f,
}

@vertex
fn vs_main(@builtin(vertex_index) vertex_index: u32) -> VertexOutput {
    var output: VertexOutput;
    switch vertex_index {
        case 0u: {
            output.position = vec4f(-1.0, -1.0, 0.0, 1.0);
            output.uv = vec2f(0.0, 1.0);
        }
        case 1u: {
            output.position = vec4f(1.0, -1.0, 0.0, 1.0);
            output.uv = vec2f(1.0, 1.0);
        }
        case 2u: {
            output.position = vec4f(-1.0, 1.0, 0.0, 1.0);
            output.uv = vec2f(0.0, 0.0);
        }
        case 3u: {
            output.position = vec4f(-1.0, 1.0, 0.0, 1.0);
            output.uv = vec2f(0.0, 0.0);
        }
        case 4u: {
            output.position = vec4f(1.0, -1.0, 0.0, 1.0);
            output.uv = vec2f(1.0, 1.0);
        }
        default: {
            output.position = vec4f(1.0, 1.0, 0.0, 1.0);
            output.uv = vec2f(1.0, 0.0);
        }
    }
    return output;
}

@fragment
fn fs_main(input: VertexOutput) -> @location(0) vec4f {
    let texel = textureSample(render_texture, render_sampler, input.uv);
    let p = input.uv - vec2f(0.5, 0.5);
    let scan = 0.90 + 0.10 * sin(input.uv.y * u.target_size.y * 0.42);
    let vignette = smoothstep(0.82, 0.16, length(p));
    let grid_x = smoothstep(0.018, 0.0, abs(fract(input.uv.x * 12.0) - 0.5));
    let grid_y = smoothstep(0.018, 0.0, abs(fract(input.uv.y * 12.0) - 0.5));
    let grid = (grid_x + grid_y) * 0.035;
    let bloom = pow(max(max(texel.r, texel.g), texel.b), 2.0);
    let color = texel.rgb * scan * vignette + vec3f(0.018, 0.024, 0.034) + vec3f(0.16, 0.30, 0.34) * bloom + grid;
    return vec4f(color, 1.0);
}
""";

    private const string ComputeShader = """
@group(0) @binding(0) var<storage, read_write> values: array<f32>;

@compute @workgroup_size(1)
fn cs_main(@builtin(global_invocation_id) id: vec3u) {
    if (id.x == 0u) {
        values[0] = 0.42;
        values[1] = 0.78;
        values[2] = 1.0;
        values[3] = 8.0;
    }
}
""";

    private readonly GraphicsContext _graphics = new();
#if BROWSER
    private WGPUBrowser _wgpu = null!;
    private unsafe Surface* _surface;
    private unsafe Dawn.SwapChain* _swapChain;
    private TextureFormat _surfaceFormat;
    private uint _surfaceWidth;
    private uint _surfaceHeight;
#else
    private readonly GlfwWindow _window = new(NativeWidth, NativeHeight, "Dumb.Graphics native WebGPU example");
    private readonly WebGPU _wgpu = null!;
    private unsafe Surface* _surface;
    private TextureFormat _surfaceFormat;
    private uint _surfaceWidth;
    private uint _surfaceHeight;
#endif

    private Entity _uniformBuffer;
    private Entity _computeBuffer;
    private Entity _vertexBuffer;
    private Entity _indexBuffer;
    private Entity _computePipeline;
    private Entity _computeBindGroup;
    private Entity _trianglePipeline;
    private Entity _triangleBindGroup;
    private Entity _texturePipeline;
    private Entity _textureBindGroup;
    private RenderTexture _renderTarget;
    private bool _disposed;

#if BROWSER
    private static GCHandle s_browserHandle;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe int BrowserAnimationFrame(double timeMs, void* userData)
    {
        var app = (GraphicsExampleApp)GCHandle.FromIntPtr((nint)userData).Target!;
        app.RenderFrame((float)(timeMs / 1000.0));
        app._graphics.Tick();
        return 1;
    }
#endif

    public async Task RunAsync()
    {
        RequestAdapterOptions adapterOptions = new()
        {
            PowerPreference = PowerPreference.HighPerformance,
            ForceFallbackAdapter = false,
            BackendType = BackendType.Undefined,
            CompatibleSurface = null
        };

        DeviceDescriptor deviceDescriptor = new()
        {
            RequiredLimits = null,
            DefaultQueue = new QueueDescriptor { Label = null },
            DeviceLostCallback = default,
            DeviceLostUserdata = null
        };

        _wgpu = _graphics.NativeApi;
#if BROWSER
        CreateBrowserSurface();
        unsafe { adapterOptions.CompatibleSurface = (Surface*)_surface; }
        await _graphics.InitializeAsync(adapterOptions, deviceDescriptor);
        CreateBrowserSwapChain();
        CreateSceneResources(_surfaceFormat);

        s_browserHandle = GCHandle.Alloc(this);
        Console.WriteLine("Dumb.Graphics browser WebGPU canvas example started.");

        unsafe
        {
            Emscripten.Emscripten.RequestAnimationFrameLoop(
                (delegate* unmanaged[Cdecl]<double, void*, int>)&BrowserAnimationFrame,
                (void*)GCHandle.ToIntPtr(s_browserHandle));
        }

        await new TaskCompletionSource().Task;
#else
        await _graphics.InitializeAsync(adapterOptions, deviceDescriptor);
        CreateSurface();
        ConfigureSurface();
        CreateSceneResources(_surfaceFormat);
        RunNativeLoop();
#endif
    }

    private unsafe void CreateSceneResources(TextureFormat presentFormat)
    {
        var uniforms = new ExampleUniforms
        {
            TargetSize = new Vector2(OffscreenWidth, OffscreenHeight),
            Time = 0.0f
        };

        _uniformBuffer = Buffers.Uniform(_graphics, uniforms);
        _computeBuffer = Buffers.Storage<float>(_graphics, 4, BufferUsage.CopySrc);
        _vertexBuffer = Buffers.Vertex(_graphics, TriangleVertices());
        _indexBuffer = Buffers.Index(_graphics, TriangleIndices());
        var sampler = Samplers.LinearClamp(_graphics);

        _renderTarget = Textures.RenderTarget(_graphics, OffscreenWidth, OffscreenHeight, ColorFormat);

        var computeShader = Shaders.Wgsl(_graphics, ComputeShader);
        var triangleShader = Shaders.Wgsl(_graphics, TriangleShader);
        var textureShader = Shaders.Wgsl(_graphics, TextureShader);

        var computeLayout = Pipelines.BindGroupLayout(_graphics,
        [
            BindingLayout.StorageBuffer(0, ShaderStage.Compute, 4 * sizeof(float))
        ]);
        var computePipelineLayout = Pipelines.Layout(_graphics, [computeLayout]);
        _computeBindGroup = Pipelines.BindGroup(_graphics, computeLayout,
        [
            Binding.Storage<float>(0, _computeBuffer, 4)
        ]);
        _computePipeline = Pipelines.Compute(_graphics, computeShader, computePipelineLayout);

        var triangleLayout = Pipelines.BindGroupLayout(_graphics,
        [
            BindingLayout.UniformBuffer(0, ShaderStage.Vertex | ShaderStage.Fragment, (ulong)sizeof(ExampleUniforms)),
            BindingLayout.StorageBuffer(1, ShaderStage.Fragment, 4 * sizeof(float), readOnly: true)
        ]);
        var trianglePipelineLayout = Pipelines.Layout(_graphics, [triangleLayout]);
        _triangleBindGroup = Pipelines.BindGroup(_graphics, triangleLayout,
        [
            Binding.Uniform<ExampleUniforms>(0, _uniformBuffer),
            Binding.Storage<float>(1, _computeBuffer, 4)
        ]);
        _trianglePipeline = Pipelines.Render(
            _graphics,
            triangleShader,
            trianglePipelineLayout,
            ColorFormat,
            [
                new VertexAttributeLayout(0, VertexFormat.Float32x2, 0),
                new VertexAttributeLayout(1, VertexFormat.Float32x3, (ulong)sizeof(Vector2))
            ],
            (ulong)sizeof(ExampleVertex));

        var textureLayout = Pipelines.BindGroupLayout(_graphics,
        [
            BindingLayout.Sampler(0, ShaderStage.Fragment),
            BindingLayout.Texture(1, ShaderStage.Fragment),
            BindingLayout.UniformBuffer(2, ShaderStage.Fragment, (ulong)sizeof(ExampleUniforms))
        ]);
        var texturePipelineLayout = Pipelines.Layout(_graphics, [textureLayout]);
        _textureBindGroup = Pipelines.BindGroup(_graphics, textureLayout,
        [
            Binding.Sampler(0, sampler),
            Binding.Texture(1, _renderTarget.View),
            Binding.Uniform<ExampleUniforms>(2, _uniformBuffer)
        ]);
        _texturePipeline = Pipelines.Render(_graphics, textureShader, texturePipelineLayout, presentFormat);
    }

#if !BROWSER
    private void RunNativeLoop()
    {
        var clock = Stopwatch.StartNew();
        Console.WriteLine("Dumb.Graphics native example opened a GLFW WebGPU window. Close the window to exit.");

        while (!_window.ShouldClose)
        {
            _window.PollEvents();

            if (_window.Width <= 0 || _window.Height <= 0)
                continue;

            var width = (uint)_window.Width;
            var height = (uint)_window.Height;
            if (width != _surfaceWidth || height != _surfaceHeight)
                ConfigureSurface();

            RenderFrame((float)clock.Elapsed.TotalSeconds);
            _graphics.Tick();
        }
    }

    private unsafe void CreateSurface()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException("The native Dumb.Graphics example currently creates a WebGPU surface through GLFW Win32 handles.");

        var hwnd = GlfwGetWin32Window(_window.NativeHandle);
        if (hwnd == 0)
            throw new InvalidOperationException("Failed to get the GLFW Win32 window handle.");

        var hwndDesc = new SurfaceDescriptorFromWindowsHWND
        {
            Chain = new ChainedStruct
            {
                Next = null,
                SType = SType.SurfaceDescriptorFromWindowsHwnd
            },
            Hinstance = (void*)GetModuleHandle(null),
            Hwnd = (void*)hwnd
        };

        var surfaceDesc = new SurfaceDescriptor
        {
            NextInChain = (ChainedStruct*)&hwndDesc,
            Label = null
        };

        _surface = _wgpu.InstanceCreateSurface((Instance*)_graphics.NativeInstanceHandle, &surfaceDesc);
        if (_surface == null)
            throw new InvalidOperationException("Failed to create the WebGPU surface.");

        _surfaceFormat = _wgpu.SurfaceGetPreferredFormat(_surface, (Adapter*)_graphics.NativeAdapterHandle);
    }

    private unsafe void ConfigureSurface()
    {
        _surfaceWidth = Math.Max(1u, (uint)_window.Width);
        _surfaceHeight = Math.Max(1u, (uint)_window.Height);

        var config = new SurfaceConfiguration
        {
            Device = (Device*)_graphics.NativeDeviceHandle,
            Format = _surfaceFormat,
            Usage = TextureUsage.RenderAttachment,
            Width = _surfaceWidth,
            Height = _surfaceHeight,
            PresentMode = PresentMode.Fifo,
            AlphaMode = CompositeAlphaMode.Opaque,
            ViewFormatCount = 0,
            ViewFormats = null,
            NextInChain = null
        };

        _wgpu.SurfaceConfigure(_surface, &config);
    }
#endif

#if BROWSER
    private unsafe void CreateBrowserSurface()
    {
        var selectorBytes = Encoding.UTF8.GetBytes("#canvas\0");
        var selectorHandle = GCHandle.Alloc(selectorBytes, GCHandleType.Pinned);

        var canvasDesc = new SurfaceDescriptorFromCanvasHTMLSelector
        {
            Chain = new ChainedStruct
            {
                Next = null,
                SType = SType.SurfaceDescriptorFromCanvasHtmlSelector
            },
            Selector = (byte*)selectorHandle.AddrOfPinnedObject()
        };

        var surfaceDesc = new SurfaceDescriptor
        {
            NextInChain = (ChainedStruct*)&canvasDesc,
            Label = null
        };

        _surface = _wgpu.InstanceCreateSurface((Instance*)_graphics.NativeInstanceHandle, surfaceDesc);
        selectorHandle.Free();

        if (_surface == null)
            throw new InvalidOperationException("Failed to create WebGPU surface from canvas.");

        _surfaceWidth = NativeWidth;
        _surfaceHeight = NativeHeight;
    }

    private unsafe void CreateBrowserSwapChain()
    {
        _surfaceFormat = _wgpu.SurfaceGetPreferredFormat(_surface, (Adapter*)_graphics.NativeAdapterHandle);

        var desc = new Dawn.SwapChainDescriptor
        {
            Usage = TextureUsage.RenderAttachment,
            Format = _surfaceFormat,
            Width = _surfaceWidth,
            Height = _surfaceHeight,
            PresentMode = PresentMode.Fifo
        };
        _swapChain = _wgpu.DeviceCreateSwapChain((Device*)_graphics.NativeDeviceHandle, _surface, desc);
    }
#endif

    private unsafe void RenderFrame(float time)
    {
        var uniforms = new ExampleUniforms
        {
            TargetSize = new Vector2(OffscreenWidth, OffscreenHeight),
            Time = time
        };
        Buffers.Write(_graphics, _uniformBuffer, uniforms);

#if BROWSER
        var swapChainView = _wgpu.SwapChainGetCurrentTextureView(_swapChain);
        if (swapChainView == null)
            return;
        var targetView = Textures.WrapNativeTextureView(_graphics, (nint)swapChainView);
        RecordAndSubmit(targetView, _surfaceWidth, _surfaceHeight);
        Textures.ReleaseView(_graphics, targetView);
#else
        var surfaceTexture = new SurfaceTexture();
        _wgpu.SurfaceGetCurrentTexture(_surface, &surfaceTexture);
        if (surfaceTexture.Status != SurfaceGetCurrentTextureStatus.Success || surfaceTexture.Texture == null)
        {
            if (surfaceTexture.Status is SurfaceGetCurrentTextureStatus.Outdated or SurfaceGetCurrentTextureStatus.Lost)
                ConfigureSurface();
            return;
        }

        var surfaceView = Textures.CreateViewForNativeTexture(_graphics, (nint)surfaceTexture.Texture, _surfaceFormat);
        RecordAndSubmit(surfaceView, _surfaceWidth, _surfaceHeight);
        _wgpu.SurfacePresent(_surface);
        Textures.ReleaseView(_graphics, surfaceView);
        _wgpu.TextureRelease(surfaceTexture.Texture);
#endif
    }

    private unsafe void RecordAndSubmit(
        Entity presentTargetView,
        uint presentTargetWidth,
        uint presentTargetHeight)
    {
        using var encoder = Commands.CreateEncoder(_graphics);

        ComputePassDescriptor computePassDescriptor = new()
        {
            TimestampWrites = null,
            Label = null
        };
        using (var computePass = encoder.BeginComputePass(&computePassDescriptor))
        {
            computePass.SetPipeline(_computePipeline);
            computePass.SetBindGroup(0, _computeBindGroup);
            computePass.Dispatch(1);
        }

        DrawTrianglePass(encoder);
        DrawTexturePass(encoder, presentTargetView, presentTargetWidth, presentTargetHeight);

        var commandBuffer = encoder.Finish();
        Commands.SubmitAndRelease(_graphics, commandBuffer);
    }

    private unsafe void DrawTrianglePass(Encoder encoder)
    {
        var attachment = Commands.ColorAttachment(
            _graphics,
            _renderTarget.View,
            new Color { R = 0.06, G = 0.08, B = 0.11, A = 1.0 });
        var descriptor = Commands.RenderPass(&attachment);

        using var pass = encoder.BeginRenderPass(&descriptor);
        pass.SetViewport(0, 0, _renderTarget.Width, _renderTarget.Height);
        pass.SetScissorRect(0, 0, _renderTarget.Width, _renderTarget.Height);
        pass.SetPipeline(_trianglePipeline);
        pass.SetBindGroup(0, _triangleBindGroup);
        pass.SetVertexBuffer(0, _vertexBuffer);
        pass.SetIndexBuffer(_indexBuffer, IndexFormat.Uint32);
        pass.DrawIndexed(12);
    }

    private unsafe void DrawTexturePass(
        Encoder encoder,
        Entity targetView,
        uint targetWidth,
        uint targetHeight)
    {
        var attachment = Commands.ColorAttachment(
            _graphics,
            targetView,
            new Color { R = 0.02, G = 0.025, B = 0.035, A = 1.0 });
        var descriptor = Commands.RenderPass(&attachment);

        using var pass = encoder.BeginRenderPass(&descriptor);
        pass.SetViewport(0, 0, targetWidth, targetHeight);
        pass.SetScissorRect(0, 0, targetWidth, targetHeight);
        pass.SetPipeline(_texturePipeline);
        pass.SetBindGroup(0, _textureBindGroup);
        pass.Draw(6);
    }

    private static ExampleVertex[] TriangleVertices() =>
    [
        new(-0.86f, -0.70f, 0.95f, 0.25f, 0.18f),
        new(0.76f, -0.62f, 0.08f, 0.72f, 0.96f),
        new(-0.06f, 0.86f, 0.98f, 0.76f, 0.18f),
        new(-0.58f, -0.12f, 0.22f, 0.88f, 0.72f),
        new(0.22f, -0.20f, 0.72f, 0.34f, 0.96f),
        new(-0.18f, 0.52f, 0.96f, 0.92f, 0.48f),
        new(0.16f, -0.76f, 0.98f, 0.44f, 0.28f),
        new(0.92f, 0.04f, 0.24f, 0.86f, 0.96f),
        new(0.42f, 0.74f, 0.96f, 0.88f, 0.24f)
    ];

    private static uint[] TriangleIndices() => [0, 1, 2, 3, 4, 5, 6, 7, 8, 1, 7, 4];

    public unsafe void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
#if BROWSER
        if (_swapChain != null)
        {
            _wgpu.SwapChainRelease(_swapChain);
            _swapChain = null;
        }
        if (_surface != null)
        {
            _wgpu.SurfaceRelease(_surface);
            _surface = null;
        }
#else
        if (_surface != null)
        {
            _wgpu.SurfaceUnconfigure(_surface);
            _wgpu.SurfaceRelease(_surface);
            _surface = null;
        }
        _window.Dispose();
#endif
        _graphics.Dispose();
    }

#if !BROWSER
    [DllImport("glfw3", EntryPoint = "glfwGetWin32Window")]
    private static extern nint GlfwGetWin32Window(nint window);

    [DllImport("kernel32", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint GetModuleHandle(string? moduleName);
#endif
}
