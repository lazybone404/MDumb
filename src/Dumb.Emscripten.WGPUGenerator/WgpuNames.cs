namespace Dumb.Emscripten.WGPUGenerator;

public static class WgpuNames
{
    public const string HEADER_FILE_NAME = "webgpu.h";
    public const string NEW_LINE = "\n";

    public static readonly string[] Callbacks =
    [
        "WGPUBufferMapCallback",
        "WGPUCompilationInfoCallback",
        "WGPUCreateComputePipelineAsyncCallback",
        "WGPUCreateRenderPipelineAsyncCallback",
        "WGPUDeviceLostCallback",
        "WGPUErrorCallback",
        "WGPUProc",
        "WGPUQueueWorkDoneCallback",
        "WGPURequestAdapterCallback",
        "WGPURequestDeviceCallback",
    ];

    public static readonly string[] Pointers =
    [
        "WGPUAdapter",
        "WGPUBindGroup",
        "WGPUBindGroupLayout",
        "WGPUBuffer",
        "WGPUCommandBuffer",
        "WGPUCommandEncoder",
        "WGPUComputePassEncoder",
        "WGPUComputePipeline",
        "WGPUDevice",
        "WGPUInstance",
        "WGPUPipelineLayout",
        "WGPUQuerySet",
        "WGPUQueue",
        "WGPURenderBundle",
        "WGPURenderBundleEncoder",
        "WGPURenderPassEncoder",
        "WGPURenderPipeline",
        "WGPUSampler",
        "WGPUShaderModule",
        "WGPUSurface",
        "WGPUSwapChain",
        "WGPUTexture",
        "WGPUTextureView",
    ];

    public static readonly HashSet<string> CallbackSet = new(Callbacks, StringComparer.Ordinal);

    public static readonly HashSet<string> PointerSet = new(Pointers, StringComparer.Ordinal);

    public static string SilkTypeName(string wgpuName) =>
        wgpuName.StartsWith("WGPU", StringComparison.Ordinal) ? wgpuName.Substring(4) : wgpuName;
}
