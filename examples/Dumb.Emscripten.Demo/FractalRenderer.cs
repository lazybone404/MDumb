using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Shit.Emscripten;
using Silk.NET.WebGPU;
using Buffer = Silk.NET.WebGPU.Buffer;
using Dawn = Silk.NET.WebGPU.Extensions.Dawn;

namespace Shit.Emscripten.Demo;

[StructLayout(LayoutKind.Sequential)]
struct FractalUniforms
{
    public float CenterX;
    public float CenterY;
    public float Zoom;
    public int MaxIterations;
    public float ColorShift;
    public float Width;
    public float Height;
    public float _pad;
}

public unsafe class FractalRenderer : IDisposable
{
    public enum InitState
    {
        Idle,
        WaitingForSurface,
        WaitingForAdapter,
        WaitingForDevice,
        Ready,
        Failed
    }

    private InitState _state = InitState.Idle;
    private bool _disposed;

    private WGPUBrowser _wgpu = null!;
    private string _canvasSelector = null!;
    private int _width, _height;

    private Instance* _instance;
    private Surface* _surface;
    private Adapter* _adapter;
    private Device* _device;
    private Queue* _queue;
    private Dawn.SwapChain* _swapChain;
    private ShaderModule* _shaderModule;
    private RenderPipeline* _pipeline;
    private PipelineLayout* _pipelineLayout;
    private BindGroupLayout* _bindGroupLayout;
    private BindGroup* _bindGroup;
    private Buffer* _uniformBuffer;

    public float CenterX = -0.5f;
    public float CenterY;
    public float Zoom = 1.0f;
    public int MaxIterations = 80;
    public float ColorShift;

    private static FractalRenderer? _current;

    private const string WGSL_SHADER = @"
struct Uniforms {
    center: vec2f,
    zoom: f32,
    max_iter: i32,
    color_shift: f32,
    width: f32,
    height: f32,
}

@group(0) @binding(0) var<uniform> u: Uniforms;

struct VertexOutput {
    @builtin(position) position: vec4f,
    @location(0) uv: vec2f,
}

@vertex
fn vs_main(@builtin(vertex_index) vi: u32) -> VertexOutput {
    let positions = array<vec2f, 6>(
        vec2f(-1.0, -1.0),
        vec2f(1.0, -1.0),
        vec2f(-1.0, 1.0),
        vec2f(-1.0, 1.0),
        vec2f(1.0, -1.0),
        vec2f(1.0, 1.0)
    );
    let uvs = array<vec2f, 6>(
        vec2f(0.0, 0.0),
        vec2f(1.0, 0.0),
        vec2f(0.0, 1.0),
        vec2f(0.0, 1.0),
        vec2f(1.0, 0.0),
        vec2f(1.0, 1.0)
    );
    var output: VertexOutput;
    output.position = vec4f(positions[vi], 0.0, 1.0);
    output.uv = uvs[vi];
    return output;
}

@fragment
fn fs_main(@location(0) uv: vec2f) -> @location(0) vec4f {
    let aspect = u.width / u.height;
    let p = uv * 2.0 - vec2f(1.0, 1.0);
    let c = vec2f(
        u.center.x + p.x * u.zoom * 1.75 * aspect,
        u.center.y - p.y * u.zoom * 1.75
    );
    var z = vec2f(0.0);
    var iter: i32 = 0;
    let max_iter = max(1, u.max_iter);
    for (var i = 0; i < max_iter; i++) {
        z = vec2f(z.x * z.x - z.y * z.y + c.x, 2.0 * z.x * z.y + c.y);
        if (dot(z, z) > 4.0) {
            break;
        }
        iter++;
    }
    if (iter >= max_iter) {
        return vec4f(0.0, 0.0, 0.0, 1.0);
    }
    let t = fract(f32(iter) / f32(max_iter) + u.color_shift);
    let r = sin(t * 12.56637 + 0.0) * 0.5 + 0.5;
    let g = sin(t * 12.56637 + 2.09439) * 0.5 + 0.5;
    let b = sin(t * 12.56637 + 4.18879) * 0.5 + 0.5;
    return vec4f(r, g, b, 1.0);
}
";

    public void Run(WGPUBrowser wgpu, string canvasSelector, int width, int height)
    {
        _wgpu = wgpu;
        _canvasSelector = canvasSelector;
        _width = width;
        _height = height;
        _current = this;

        Emscripten.ConsoleLog("[WGPU] Creating instance...");
        _instance = _wgpu.CreateInstance((InstanceDescriptor*)null);
        if (_instance == null)
        {
            Emscripten.ConsoleLog("[WGPU] ERROR: CreateInstance returned null!");
            _state = InitState.Failed;
            return;
        }
        Emscripten.ConsoleLog($"[WGPU] Instance: 0x{(nint)_instance:X}");
        _state = InitState.WaitingForSurface;
    }

    private void TryProgressInit()
    {
        switch (_state)
        {
            case InitState.WaitingForSurface:
                CreateSurface();
                if (_surface != null)
                {
                    _state = InitState.WaitingForAdapter;
                    RequestAdapter();
                }
                else
                {
                    Emscripten.ConsoleLog("[WGPU] Init: surface creation failed!");
                    _state = InitState.Failed;
                }
                break;

            case InitState.WaitingForAdapter:
            case InitState.WaitingForDevice:
            case InitState.Ready:
            case InitState.Failed:
            case InitState.Idle:
                break;
        }
    }

    private void CreateSurface()
    {
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

        _surface = _wgpu.InstanceCreateSurface(_instance, surfaceDesc);
        selectorHandle.Free();

        if (_surface == null)
            Emscripten.ConsoleLog("[WGPU] ERROR: CreateSurface returned null!");
        else
            Emscripten.ConsoleLog($"[WGPU] Surface: 0x{(nint)_surface:X}");
    }

    private void RequestAdapter()
    {
        if (_surface == null) { _state = InitState.Failed; return; }

        var options = new RequestAdapterOptions
        {
            CompatibleSurface = _surface,
            PowerPreference = PowerPreference.HighPerformance,
            BackendType = BackendType.Undefined,
            ForceFallbackAdapter = false
        };

        Emscripten.ConsoleLog("[WGPU] Requesting adapter...");
        _wgpu.InstanceRequestAdapter(_instance, options, &AdapterCallback, null);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void AdapterCallback(RequestAdapterStatus status, Adapter* adapter, byte* message, void* userdata)
    {
        var self = _current!;
        if (status != RequestAdapterStatus.Success || adapter == null)
        {
            var msg = message != null ? Marshal.PtrToStringUTF8((nint)message) : "unknown";
            Emscripten.ConsoleLog($"[WGPU] Adapter request failed: {status} - {msg}");
            self._state = InitState.Failed;
            return;
        }

        self._adapter = adapter;
        Emscripten.ConsoleLog($"[WGPU] Adapter: 0x{(nint)adapter:X}");

        AdapterProperties props = default;
        self._wgpu.AdapterGetProperties(adapter, &props);
        var name = Marshal.PtrToStringUTF8((nint)props.Name) ?? "?";
        Emscripten.ConsoleLog($"[WGPU] Adapter: {name} ({props.AdapterType}, backend {props.BackendType})");

        Emscripten.ConsoleLog("[WGPU] Requesting device...");

        DeviceDescriptor deviceDesc = new()
        {
            RequiredLimits = null,
            DefaultQueue = new QueueDescriptor { Label = null },
            DeviceLostCallback = default,
            DeviceLostUserdata = null
        };

        self._wgpu.AdapterRequestDevice(adapter, deviceDesc, &DeviceCallback, null);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void DeviceCallback(RequestDeviceStatus status, Device* device, byte* message, void* userdata)
    {
        var self = _current!;
        if (status != RequestDeviceStatus.Success || device == null)
        {
            var msg = message != null ? Marshal.PtrToStringUTF8((nint)message) : "unknown";
            Emscripten.ConsoleLog($"[WGPU] Device request failed: {status} - {msg}");
            self._state = InitState.Failed;
            return;
        }

        self._device = device;
        self._queue = self._wgpu.DeviceGetQueue(device);
        Emscripten.ConsoleLog($"[WGPU] Device: 0x{(nint)device:X}, Queue: 0x{(nint)self._queue:X}");
        self._wgpu.DeviceSetUncapturedErrorCallback(
            device,
            (delegate* unmanaged[Cdecl]<ErrorType, byte*, void*, void>)&UncapturedErrorCallback,
            null);
        Emscripten.ConsoleLog("[WGPU] Error callbacks installed.");

        self.CreateSwapChain();
        self.CreateShader();
        self.CreatePipeline();
        self.CreateUniformBuffer();
        self.CreateBindGroup();

        self._state = InitState.Ready;
        Emscripten.ConsoleLog("[WGPU] Pipeline ready.");
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UncapturedErrorCallback(ErrorType type, byte* message, void* userdata)
    {
        LogWgpuError("uncaptured", type, message);
    }

    private static void LogWgpuError(string source, ErrorType type, byte* message)
    {
        var msg = message != null ? Marshal.PtrToStringUTF8((nint)message) : "";
        if (type == ErrorType.NoError)
            Emscripten.ConsoleLog($"[WGPU] {source} error scope: NoError");
        else
            Emscripten.ConsoleLog($"[WGPU] {source} error: {type} - {msg}");
    }

    private void CreateSwapChain()
    {
        var desc = new Dawn.SwapChainDescriptor
        {
            Usage = TextureUsage.RenderAttachment,
            Format = TextureFormat.Bgra8Unorm,
            Width = (uint)_width,
            Height = (uint)_height,
            PresentMode = PresentMode.Fifo
        };
        _swapChain = _wgpu.DeviceCreateSwapChain(_device, _surface, desc);
        Emscripten.ConsoleLog($"[WGPU] SwapChain: 0x{(nint)_swapChain:X}");
    }

    private void CreateShader()
    {
        var wgslBytes = Encoding.UTF8.GetBytes(WGSL_SHADER);
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

        _shaderModule = _wgpu.DeviceCreateShaderModule(_device, shaderDesc);
        handle.Free();
        Emscripten.ConsoleLog($"[WGPU] ShaderModule: 0x{(nint)_shaderModule:X}");
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
                MinBindingSize = (ulong)sizeof(FractalUniforms)
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
        _bindGroupLayout = _wgpu.DeviceCreateBindGroupLayout(_device, bglDesc);
        Emscripten.ConsoleLog($"[WGPU] BindGroupLayout: 0x{(nint)_bindGroupLayout:X}");

        var bglLocal = _bindGroupLayout;
        var plDesc = new PipelineLayoutDescriptor
        {
            BindGroupLayoutCount = 1,
            BindGroupLayouts = &bglLocal,
            Label = null
        };
        _pipelineLayout = _wgpu.DeviceCreatePipelineLayout(_device, plDesc);

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

        _pipeline = _wgpu.DeviceCreateRenderPipeline(_device, pipelineDesc);

        vsHandle.Free();
        fsHandle.Free();

        Emscripten.ConsoleLog($"[WGPU] Pipeline: 0x{(nint)_pipeline:X}");
    }

    private void CreateUniformBuffer()
    {
        var desc = new BufferDescriptor
        {
            Usage = BufferUsage.Uniform | BufferUsage.CopyDst,
            Size = (ulong)sizeof(FractalUniforms),
            MappedAtCreation = false,
            Label = null
        };
        _uniformBuffer = _wgpu.DeviceCreateBuffer(_device, desc);
        Emscripten.ConsoleLog($"[WGPU] UniformBuffer: 0x{(nint)_uniformBuffer:X}");
        WriteUniforms();
    }

    private void WriteUniforms()
    {
        var uniforms = new FractalUniforms
        {
            CenterX = CenterX,
            CenterY = CenterY,
            Zoom = Zoom,
            MaxIterations = MaxIterations,
            ColorShift = ColorShift,
            Width = _width,
            Height = _height
        };
        _wgpu.QueueWriteBuffer(_queue, _uniformBuffer, 0, &uniforms, (nuint)sizeof(FractalUniforms));
    }

    private void CreateBindGroup()
    {
        var entry = new BindGroupEntry
        {
            Binding = 0,
            Buffer = _uniformBuffer,
            Offset = 0,
            Size = (nuint)sizeof(FractalUniforms),
            Sampler = null,
            TextureView = null
        };

        var desc = new BindGroupDescriptor
        {
            Layout = _bindGroupLayout,
            EntryCount = 1,
            Entries = &entry,
            Label = null
        };
        _bindGroup = _wgpu.DeviceCreateBindGroup(_device, desc);
        Emscripten.ConsoleLog($"[WGPU] BindGroup: 0x{(nint)_bindGroup:X}");
    }

    private bool _didFirstRender;
    private int _renderFrameCount;

    public void Render()
    {
        TryProgressInit();

        if (_state != InitState.Ready || _disposed)
            return;

        if (!_didFirstRender)
        {
            Emscripten.ConsoleLog($"[WGPU] First render frame! center=({CenterX:F2},{CenterY:F2}) zoom={Zoom:F2}");
            _didFirstRender = true;
        }
        _renderFrameCount++;

        WriteUniforms();

        var texView = _wgpu.SwapChainGetCurrentTextureView(_swapChain);
        if (texView == null)
            return;

        var colorAttachment = new RenderPassColorAttachment
        {
            View = texView,
            DepthSlice = uint.MaxValue,
            ResolveTarget = null,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new Color { R = 0.02, G = 0.02, B = 0.05, A = 1.0 }
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

        var encoder = _wgpu.DeviceCreateCommandEncoder(_device, default);
        var pass = _wgpu.CommandEncoderBeginRenderPass(encoder, renderPassDesc);
        _wgpu.RenderPassEncoderSetViewport(pass, 0, 0, _width, _height, 0, 1);
        _wgpu.RenderPassEncoderSetPipeline(pass, _pipeline);
        _wgpu.RenderPassEncoderSetBindGroup(pass, 0, _bindGroup, 0, null);
        _wgpu.RenderPassEncoderDraw(pass, 6, 1, 0, 0);
        _wgpu.RenderPassEncoderEnd(pass);
        var cmdBuffer = _wgpu.CommandEncoderFinish(encoder, default);

        _wgpu.QueueSubmit(_queue, 1, &cmdBuffer);

        _wgpu.CommandEncoderRelease(encoder);
        _wgpu.RenderPassEncoderRelease(pass);
        _wgpu.CommandBufferRelease(cmdBuffer);
        _wgpu.TextureViewRelease(texView);
    }

    public bool IsReady => _state == InitState.Ready;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_bindGroup != null) _wgpu?.BindGroupRelease(_bindGroup);
        if (_bindGroupLayout != null) _wgpu?.BindGroupLayoutRelease(_bindGroupLayout);
        if (_pipelineLayout != null) _wgpu?.PipelineLayoutRelease(_pipelineLayout);
        if (_pipeline != null) _wgpu?.RenderPipelineRelease(_pipeline);
        if (_shaderModule != null) _wgpu?.ShaderModuleRelease(_shaderModule);
        if (_uniformBuffer != null) _wgpu?.BufferRelease(_uniformBuffer);
        if (_swapChain != null) _wgpu?.SwapChainRelease(_swapChain);
        if (_queue != null) _wgpu?.QueueRelease(_queue);
        if (_device != null) _wgpu?.DeviceRelease(_device);
        if (_adapter != null) _wgpu?.AdapterRelease(_adapter);
        if (_surface != null) _wgpu?.SurfaceRelease(_surface);
        if (_instance != null) _wgpu?.InstanceRelease(_instance);

        Emscripten.ConsoleLog("[WGPU] Shutdown complete.");
    }
}
