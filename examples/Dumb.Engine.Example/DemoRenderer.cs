using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
#if BROWSER
using Dumb.Emscripten;
using EmscriptenApi = Dumb.Emscripten.Emscripten;
#else
using Dumb.Engine.Window;
#endif
using Silk.NET.WebGPU;
#if BROWSER
using Dawn = Silk.NET.WebGPU.Extensions.Dawn;
#endif
using WgpuBuffer = Silk.NET.WebGPU.Buffer;

namespace Dumb.Engine.Example;

[StructLayout(LayoutKind.Sequential)]
internal struct DemoUniforms
{
    public float MouseX;
    public float MouseY;
    public float Width;
    public float Height;
    public float Horizontal;
    public float Vertical;
    public float Gain;
    public float Pitch;
    public float Pulse;
    public float Muted;
    public float Time;
    public float Pad;
}

public sealed unsafe class DemoRenderer : IDisposable
{
    private enum InitState
    {
        Idle,
        WaitingForSurface,
        WaitingForAdapter,
        WaitingForDevice,
        Ready,
        Failed
    }

    private const string ShaderSource = @"
struct DemoUniforms {
    mouse: vec2f,
    resolution: vec2f,
    axis: vec2f,
    audio: vec2f,
    flags: vec4f,
}

@group(0) @binding(0) var<uniform> u: DemoUniforms;

struct VertexOutput {
    @builtin(position) position: vec4f,
    @location(0) uv: vec2f,
}

@vertex
fn vs_main(@builtin(vertex_index) vi: u32) -> VertexOutput {
    var pos: vec2f;
    var uv: vec2f;
    if (vi == 0u)      { pos = vec2f(-1.0, -1.0); uv = vec2f(0.0, 0.0); }
    else if (vi == 1u) { pos = vec2f(1.0, -1.0);  uv = vec2f(1.0, 0.0); }
    else if (vi == 2u) { pos = vec2f(-1.0, 1.0);  uv = vec2f(0.0, 1.0); }
    else if (vi == 3u) { pos = vec2f(-1.0, 1.0);  uv = vec2f(0.0, 1.0); }
    else if (vi == 4u) { pos = vec2f(1.0, -1.0);  uv = vec2f(1.0, 0.0); }
    else               { pos = vec2f(1.0, 1.0);   uv = vec2f(1.0, 1.0); }

    var output: VertexOutput;
    output.position = vec4f(pos, 0.0, 1.0);
    output.uv = uv;
    return output;
}

fn line(value: f32, center: f32, width: f32) -> f32 {
    return 1.0 - smoothstep(0.0, width, abs(value - center));
}

@fragment
fn fs_main(@location(0) uv: vec2f) -> @location(0) vec4f {
    let mouseUv = vec2f(
        clamp(u.mouse.x, 0.0, 1.0),
        clamp(u.mouse.y, 0.0, 1.0)
    );
    let d = distance(uv, mouseUv);

    let xAxis = 0.5 + clamp(u.axis.x, -1.0, 1.0) * 0.32;
    let yAxis = 0.5 - clamp(u.axis.y, -1.0, 1.0) * 0.32;
    let audioLevel = clamp(u.audio.x, 0.0, 0.8) / 0.8;
    let pitchTint = clamp((u.audio.y - 0.35) / 1.85, 0.0, 1.0);
    let pulse = clamp(u.flags.x, 0.0, 1.0);
    let muted = u.flags.y;
    let time = u.flags.z;

    let grid = line(fract(uv.x * 16.0), 0.0, 0.022) * 0.16 +
        line(fract(uv.y * 9.0), 0.0, 0.022) * 0.16;
    let cursor = 1.0 - smoothstep(0.022 + pulse * 0.035, 0.070 + pulse * 0.08, d);
    let axisMark = max(line(uv.x, xAxis, 0.014), line(uv.y, yAxis, 0.014));
    let beat = sin(time * 6.28318) * 0.5 + 0.5;

    let waveSpeed = time * (0.65 + u.audio.y * 0.25);
    let waveA = 0.50 + sin(uv.x * 18.0 + waveSpeed * 5.0) * (0.10 + audioLevel * 0.10) + u.axis.y * 0.06;
    let waveB = 0.50 + cos(uv.x * 10.0 - waveSpeed * 3.0) * 0.18 - u.axis.x * 0.05;
    let waveC = 0.50 + sin(uv.x * 30.0 + waveSpeed * 8.0) * 0.045 + cos(uv.x * 7.0) * 0.09;
    let curveA = line(uv.y, waveA, 0.014 + pulse * 0.012);
    let curveB = line(uv.y, waveB, 0.011);
    let curveC = line(uv.y, waveC, 0.008);
    let centerLine = line(uv.y, 0.5, 0.003);

    var color = vec3f(0.070, 0.085, 0.115);
    color += vec3f(0.12, 0.16, 0.20) * grid;
    color += vec3f(0.10, 0.34, 0.70) * axisMark;
    color += vec3f(0.95, 0.73 + pitchTint * 0.18, 0.16) * curveA;
    color += vec3f(0.20, 0.82, 0.95) * curveB;
    color += vec3f(0.60 + pitchTint * 0.25, 0.34, 0.96) * curveC;
    color += vec3f(0.28, 0.32, 0.38) * centerLine;
    color += vec3f(1.00, 0.52, 0.15) * cursor;
    if (uv.x < audioLevel && uv.y > 0.91 && uv.y < 0.955) {
        color += vec3f(0.25 + pitchTint * 0.50, 0.95, 0.45);
    }
    color += vec3f(0.58, 0.12, 0.08) * pulse * (1.0 - smoothstep(0.0, 0.75, d));
    color += vec3f(0.06, 0.07, 0.09) * beat * audioLevel;
    color = mix(color, color * 0.35, muted);

    return vec4f(color, 1.0);
}
";

    private static DemoRenderer? s_current;

#if BROWSER
    private readonly WGPUBrowser _wgpu = new();
    private readonly string _canvasSelector;
#else
    private readonly WebGPU _wgpu = WebGPU.GetApi();
    private readonly WindowRuntime _runtime;
#endif

    private InitState _state = InitState.Idle;
    private bool _disposed;
    private int _width;
    private int _height;
    private double _time;
    private bool _didFirstRender;

    private Instance* _instance;
    private Surface* _surface;
    private Adapter* _adapter;
    private Device* _device;
    private Queue* _queue;
#if BROWSER
    private Dawn.SwapChain* _swapChain;
#endif
    private ShaderModule* _shaderModule;
    private RenderPipeline* _pipeline;
    private PipelineLayout* _pipelineLayout;
    private BindGroupLayout* _bindGroupLayout;
    private BindGroup* _bindGroup;
    private WgpuBuffer* _uniformBuffer;

#if BROWSER
    public DemoRenderer(string canvasSelector, int width, int height)
    {
        _canvasSelector = canvasSelector;
        Init(width, height);
    }
#else
    public DemoRenderer(WindowRuntime runtime, int width, int height)
    {
        _runtime = runtime;
        Init(width, height);
    }
#endif

    private void Init(int width, int height)
    {
        _width = Math.Max(1, width);
        _height = Math.Max(1, height);
        s_current = this;

        Log("[EngineExample/WGPU] Creating instance.");
        _instance = _wgpu.CreateInstance((InstanceDescriptor*)null);
        _state = _instance == null ? InitState.Failed : InitState.WaitingForSurface;
    }

    public void Render(
        int width,
        int height,
        Vector2 mouse,
        float horizontal,
        float vertical,
        float gain,
        float pitch,
        float pulse,
        bool muted)
    {
        if (_disposed)
            return;

        ProgressInit();
        if (_state != InitState.Ready)
            return;

        if (width > 0 && height > 0 && (width != _width || height != _height))
            Resize(width, height);

        if (!_didFirstRender)
        {
            _didFirstRender = true;
            Log("[EngineExample/WGPU] First curve frame submitted.");
        }

        _time += 1.0 / 60.0;
        var uniforms = new DemoUniforms
        {
            MouseX = mouse.X,
            MouseY = mouse.Y,
            Width = _width,
            Height = _height,
            Horizontal = horizontal,
            Vertical = vertical,
            Gain = gain,
            Pitch = pitch,
            Pulse = pulse,
            Muted = muted ? 1f : 0f,
            Time = (float)_time
        };
        _wgpu.QueueWriteBuffer(_queue, _uniformBuffer, 0, &uniforms, (nuint)sizeof(DemoUniforms));

        var texView = GetCurrentTextureView();
        if (texView == null)
            return;

        var colorAttachment = new RenderPassColorAttachment
        {
            View = texView,
            DepthSlice = uint.MaxValue,
            ResolveTarget = null,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new Color { R = 0.08, G = 0.10, B = 0.14, A = 1.0 }
        };

        var renderPassDesc = new RenderPassDescriptor
        {
            ColorAttachmentCount = 1,
            ColorAttachments = &colorAttachment,
            DepthStencilAttachment = null,
            OcclusionQuerySet = null,
            TimestampWrites = null,
            Label = null
        };

        var encoder = _wgpu.DeviceCreateCommandEncoder(_device, null);
#if BROWSER
        var pass = _wgpu.CommandEncoderBeginRenderPass(encoder, renderPassDesc);
#else
        var pass = _wgpu.CommandEncoderBeginRenderPass(encoder, in renderPassDesc);
#endif
        _wgpu.RenderPassEncoderSetViewport(pass, 0, 0, _width, _height, 0, 1);
        _wgpu.RenderPassEncoderSetPipeline(pass, _pipeline);
        _wgpu.RenderPassEncoderSetBindGroup(pass, 0, _bindGroup, 0, null);
        _wgpu.RenderPassEncoderDraw(pass, 6, 1, 0, 0);
        _wgpu.RenderPassEncoderEnd(pass);

        var cmdBuffer = _wgpu.CommandEncoderFinish(encoder, null);
        _wgpu.QueueSubmit(_queue, 1, &cmdBuffer);

        _wgpu.CommandEncoderRelease(encoder);
        _wgpu.RenderPassEncoderRelease(pass);
        _wgpu.CommandBufferRelease(cmdBuffer);
        ReleaseTextureView(texView);
        PresentSurface();
    }

    private void ProgressInit()
    {
        if (_state != InitState.WaitingForSurface)
            return;

        _surface = CreateSurface();
        if (_surface == null)
        {
            _state = InitState.Failed;
            return;
        }

        _state = InitState.WaitingForAdapter;
        var options = new RequestAdapterOptions
        {
            CompatibleSurface = _surface,
            PowerPreference = PowerPreference.HighPerformance,
            BackendType = BackendType.Undefined,
            ForceFallbackAdapter = false
        };
#if BROWSER
        _wgpu.InstanceRequestAdapter(_instance, options, &AdapterCallback, null);
#else
        _wgpu.InstanceRequestAdapter(
            _instance, &options, new(&AdapterCallback), null);
#endif
    }

    private Surface* CreateSurface()
    {
#if BROWSER
        var selectorBytes = Encoding.UTF8.GetBytes(_canvasSelector + '\0');
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

        var surface = _wgpu.InstanceCreateSurface(_instance, surfaceDesc);
        selectorHandle.Free();
        Log(surface == null
            ? "[EngineExample/WGPU] Surface creation failed."
            : "[EngineExample/WGPU] Surface ready.");
        return surface;
#else
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException(
                "The native Dumb.Engine example currently creates a WebGPU surface through GLFW Win32 handles.");

        var (hwnd, _, hInstance) = _runtime.Native!.Win32!.Value;
        if (hwnd == 0)
            throw new InvalidOperationException("Failed to get the GLFW Win32 window handle.");

        var hwndDesc = new SurfaceDescriptorFromWindowsHWND
        {
            Chain = new ChainedStruct
            {
                Next = null,
                SType = SType.SurfaceDescriptorFromWindowsHwnd
            },
            Hinstance = (void*)hInstance,
            Hwnd = (void*)hwnd
        };

        var surfaceDesc = new SurfaceDescriptor
        {
            NextInChain = (ChainedStruct*)&hwndDesc,
            Label = null
        };

        var surface = _wgpu.InstanceCreateSurface(_instance, in surfaceDesc);
        Log(surface == null
            ? "[EngineExample/WGPU] Surface creation failed."
            : "[EngineExample/WGPU] Surface ready.");
        return surface;
#endif
    }

    private void ConfigurePresentSurface()
    {
#if BROWSER
        var format = _wgpu.SurfaceGetPreferredFormat(_surface, _adapter);
        var desc = new Dawn.SwapChainDescriptor
        {
            Usage = TextureUsage.RenderAttachment,
            Format = format,
            Width = (uint)_width,
            Height = (uint)_height,
            PresentMode = PresentMode.Fifo
        };
        _swapChain = _wgpu.DeviceCreateSwapChain(_device, _surface, desc);
        Log($"[EngineExample/WGPU] SwapChain: 0x{(nint)_swapChain:X}");
#else
        var config = new SurfaceConfiguration
        {
            Device = _device,
            Format = TextureFormat.Bgra8Unorm,
            Usage = TextureUsage.RenderAttachment,
            ViewFormatCount = 0,
            ViewFormats = null,
            AlphaMode = CompositeAlphaMode.Auto,
            Width = (uint)_width,
            Height = (uint)_height,
            PresentMode = PresentMode.Fifo
        };
        _wgpu.SurfaceConfigure(_surface, &config);
        Log("[EngineExample/WGPU] Surface configured.");
#endif
    }

    private TextureView* GetCurrentTextureView()
    {
#if BROWSER
        return _wgpu.SwapChainGetCurrentTextureView(_swapChain);
#else
        var surfaceTexture = new SurfaceTexture();
        _wgpu.SurfaceGetCurrentTexture(_surface, &surfaceTexture);
        if (surfaceTexture.Status != SurfaceGetCurrentTextureStatus.Success ||
            surfaceTexture.Texture == null)
        {
            if (surfaceTexture.Status is SurfaceGetCurrentTextureStatus.Outdated
                or SurfaceGetCurrentTextureStatus.Lost)
                ConfigurePresentSurface();
            return null;
        }
        return _wgpu.TextureCreateView(surfaceTexture.Texture, null);
#endif
    }

    private void ReleaseTextureView(TextureView* texView)
    {
        _wgpu.TextureViewRelease(texView);
    }

#if BROWSER
    private void PresentSurface() { /* swapchain auto-presents */ }
#else
    private void PresentSurface()
    {
        _wgpu.SurfacePresent(_surface);
    }
#endif

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void AdapterCallback(RequestAdapterStatus status, Adapter* adapter, byte* message, void* userdata)
    {
        var self = s_current!;
        if (status != RequestAdapterStatus.Success || adapter == null)
        {
            self.LogRequestFailure("adapter", status.ToString(), message);
            self._state = InitState.Failed;
            return;
        }

        self._adapter = adapter;
        var deviceDesc = new DeviceDescriptor
        {
            RequiredLimits = null,
            DefaultQueue = new QueueDescriptor { Label = null },
            DeviceLostCallback = default,
            DeviceLostUserdata = null
        };
        self._state = InitState.WaitingForDevice;
#if BROWSER
        self._wgpu.AdapterRequestDevice(adapter, deviceDesc, &DeviceCallback, null);
#else
        self._wgpu.AdapterRequestDevice(
            adapter, &deviceDesc, new(&DeviceCallback), null);
#endif
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void DeviceCallback(RequestDeviceStatus status, Device* device, byte* message, void* userdata)
    {
        var self = s_current!;
        if (status != RequestDeviceStatus.Success || device == null)
        {
            self.LogRequestFailure("device", status.ToString(), message);
            self._state = InitState.Failed;
            return;
        }

        self._device = device;
        self._queue = self._wgpu.DeviceGetQueue(device);
        self._wgpu.DeviceSetUncapturedErrorCallback(
            device,
            (delegate* unmanaged[Cdecl]<ErrorType, byte*, void*, void>)&UncapturedErrorCallback,
            null);

        self.ConfigurePresentSurface();
        self.CreateShader();
        self.CreatePipeline();
        self.CreateUniforms();
        self._state = InitState.Ready;
        Log("[EngineExample/WGPU] Renderer ready.");
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UncapturedErrorCallback(ErrorType type, byte* message, void* userdata)
    {
        if (type == ErrorType.NoError)
            return;

        var msg = message != null ? Marshal.PtrToStringUTF8((nint)message) : string.Empty;
        Log($"[EngineExample/WGPU] {type}: {msg}");
    }

    private void CreateShader()
    {
        var wgslBytes = Encoding.UTF8.GetBytes(ShaderSource);
        var handle = GCHandle.Alloc(wgslBytes, GCHandleType.Pinned);

        var wgslDesc = new ShaderModuleWGSLDescriptor
        {
            Chain = new ChainedStruct
            {
                Next = null,
                SType = SType.ShaderModuleWgslDescriptor
            },
            Code = (byte*)handle.AddrOfPinnedObject()
        };

        var shaderDesc = new ShaderModuleDescriptor
        {
            NextInChain = (ChainedStruct*)&wgslDesc,
            Label = null,
            HintCount = 0,
            Hints = null
        };

#if BROWSER
        _shaderModule = _wgpu.DeviceCreateShaderModule(_device, shaderDesc);
#else
        _shaderModule = _wgpu.DeviceCreateShaderModule(_device, in shaderDesc);
#endif
        handle.Free();
        Log($"[EngineExample/WGPU] ShaderModule: 0x{(nint)_shaderModule:X}");
    }

    private void CreatePipeline()
    {
        var bglEntry = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Fragment,
            Buffer = new BufferBindingLayout
            {
                Type = BufferBindingType.Uniform,
                HasDynamicOffset = false,
                MinBindingSize = (ulong)sizeof(DemoUniforms)
            },
            Sampler = default,
            Texture = default,
            StorageTexture = default
        };

        var bglDesc = new BindGroupLayoutDescriptor
        {
            EntryCount = 1,
            Entries = &bglEntry,
            Label = null
        };
#if BROWSER
        _bindGroupLayout = _wgpu.DeviceCreateBindGroupLayout(_device, bglDesc);
#else
        _bindGroupLayout = _wgpu.DeviceCreateBindGroupLayout(_device, in bglDesc);
#endif
        Log($"[EngineExample/WGPU] BindGroupLayout: 0x{(nint)_bindGroupLayout:X}");

        var bgl = _bindGroupLayout;
        var layoutDesc = new PipelineLayoutDescriptor
        {
            BindGroupLayoutCount = 1,
            BindGroupLayouts = &bgl,
            Label = null
        };
#if BROWSER
        _pipelineLayout = _wgpu.DeviceCreatePipelineLayout(_device, layoutDesc);
#else
        _pipelineLayout = _wgpu.DeviceCreatePipelineLayout(_device, in layoutDesc);
#endif

        var vsNameBytes = Encoding.UTF8.GetBytes("vs_main\0");
        var fsNameBytes = Encoding.UTF8.GetBytes("fs_main\0");
        var vsHandle = GCHandle.Alloc(vsNameBytes, GCHandleType.Pinned);
        var fsHandle = GCHandle.Alloc(fsNameBytes, GCHandleType.Pinned);

        var vertexState = new VertexState
        {
            Module = _shaderModule,
            EntryPoint = (byte*)vsHandle.AddrOfPinnedObject(),
            BufferCount = 0,
            Buffers = null
        };

        var colorTarget = new ColorTargetState
        {
            Format = TextureFormat.Bgra8Unorm,
            Blend = null,
            WriteMask = ColorWriteMask.All
        };

        var fragmentState = new FragmentState
        {
            Module = _shaderModule,
            EntryPoint = (byte*)fsHandle.AddrOfPinnedObject(),
            TargetCount = 1,
            Targets = &colorTarget
        };

        var pipelineDesc = new RenderPipelineDescriptor
        {
            Layout = _pipelineLayout,
            Vertex = vertexState,
            Primitive = new PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                StripIndexFormat = IndexFormat.Undefined,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None
            },
            Multisample = new MultisampleState
            {
                Count = 1,
                Mask = ~0u,
                AlphaToCoverageEnabled = false
            },
            Fragment = &fragmentState,
            DepthStencil = null,
            Label = null
        };

#if BROWSER
        _pipeline = _wgpu.DeviceCreateRenderPipeline(_device, pipelineDesc);
#else
        _pipeline = _wgpu.DeviceCreateRenderPipeline(_device, in pipelineDesc);
#endif
        vsHandle.Free();
        fsHandle.Free();
        Log($"[EngineExample/WGPU] Pipeline: 0x{(nint)_pipeline:X}");
    }

    private void CreateUniforms()
    {
        var bufferDesc = new BufferDescriptor
        {
            Usage = BufferUsage.Uniform | BufferUsage.CopyDst,
            Size = (ulong)sizeof(DemoUniforms),
            MappedAtCreation = false,
            Label = null
        };
#if BROWSER
        _uniformBuffer = _wgpu.DeviceCreateBuffer(_device, bufferDesc);
#else
        _uniformBuffer = _wgpu.DeviceCreateBuffer(_device, in bufferDesc);
#endif
        Log($"[EngineExample/WGPU] UniformBuffer: 0x{(nint)_uniformBuffer:X}");

        var entry = new BindGroupEntry
        {
            Binding = 0,
            Buffer = _uniformBuffer,
            Offset = 0,
            Size = (nuint)sizeof(DemoUniforms),
            Sampler = null,
            TextureView = null
        };

        var bindGroupDesc = new BindGroupDescriptor
        {
            Layout = _bindGroupLayout,
            EntryCount = 1,
            Entries = &entry,
            Label = null
        };
#if BROWSER
        _bindGroup = _wgpu.DeviceCreateBindGroup(_device, bindGroupDesc);
#else
        _bindGroup = _wgpu.DeviceCreateBindGroup(_device, in bindGroupDesc);
#endif
        Log($"[EngineExample/WGPU] BindGroup: 0x{(nint)_bindGroup:X}");
    }

    private void Resize(int width, int height)
    {
        _width = Math.Max(1, width);
        _height = Math.Max(1, height);
#if BROWSER
        if (_swapChain != null)
            _wgpu.SwapChainRelease(_swapChain);
#endif
        ConfigurePresentSurface();
    }

    private void LogRequestFailure(string target, string status, byte* message)
    {
        var msg = message != null ? Marshal.PtrToStringUTF8((nint)message) : string.Empty;
        Log($"[EngineExample/WGPU] Failed to request {target}: {status} {msg}");
    }

#if BROWSER
    private static void Log(string msg) => EmscriptenApi.ConsoleLog(msg);
#else
    private static void Log(string msg) => Console.WriteLine(msg);
#endif

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_bindGroup != null) _wgpu.BindGroupRelease(_bindGroup);
        if (_bindGroupLayout != null) _wgpu.BindGroupLayoutRelease(_bindGroupLayout);
        if (_pipelineLayout != null) _wgpu.PipelineLayoutRelease(_pipelineLayout);
        if (_pipeline != null) _wgpu.RenderPipelineRelease(_pipeline);
        if (_shaderModule != null) _wgpu.ShaderModuleRelease(_shaderModule);
        if (_uniformBuffer != null) _wgpu.BufferRelease(_uniformBuffer);
#if BROWSER
        if (_swapChain != null) _wgpu.SwapChainRelease(_swapChain);
#endif
        if (_queue != null) _wgpu.QueueRelease(_queue);
        if (_device != null) _wgpu.DeviceRelease(_device);
        if (_adapter != null) _wgpu.AdapterRelease(_adapter);
        if (_surface != null) _wgpu.SurfaceRelease(_surface);
        if (_instance != null) _wgpu.InstanceRelease(_instance);
#if BROWSER
        _wgpu.Dispose();
#endif
    }
}
