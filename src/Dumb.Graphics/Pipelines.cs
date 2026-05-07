using System.Runtime.InteropServices;
using System.Threading;
using Dumb.Engine.Graph;
using Silk.NET.WebGPU;

namespace Dumb.Graphics;

public static unsafe class Pipelines
{
    // --- BindGroupLayout ---

    public static Handle<BindGroupLayoutData> CreateBindGroupLayout(GraphicsContext ctx, BindGroupLayoutDescriptor descriptor)
    {
        nint native = ctx.Device.CreateBindGroupLayout(ctx.NativeDevice, &descriptor);
        return ctx._bindGroupLayouts.Create(new BindGroupLayoutData { NativePtr = native, RefCount = 1 });
    }

    internal static void ReleaseBindGroupLayout(GraphicsContext ctx, Handle<BindGroupLayoutData> handle)
    {
        if (!ctx._bindGroupLayouts.TryGet(handle, out var r)) return;
        ref var bgl = ref r.Value;
        if (Interlocked.Decrement(ref bgl.RefCount) == 0)
        {
            ctx.Device.ReleaseBindGroupLayout(bgl.NativePtr);
            ctx._bindGroupLayouts.Destroy(handle);
        }
    }

    // --- BindGroup ---

    public static Handle<BindGroupData> CreateBindGroup(
        GraphicsContext ctx, BindGroupDescriptor descriptor, Handle<BindGroupLayoutData> layout)
    {
        if (!ctx._bindGroupLayouts.Contains(layout))
            throw new InvalidOperationException("BindGroupLayout handle not alive.");

        nint native = ctx.Device.CreateBindGroup(ctx.NativeDevice, &descriptor);
        return ctx._bindGroups.Create(new BindGroupData
        {
            NativePtr = native,
            LayoutHandle = layout.Value,
            RefCount = 1
        });
    }

    internal static void ReleaseBindGroup(GraphicsContext ctx, Handle<BindGroupData> handle)
    {
        if (!ctx._bindGroups.TryGet(handle, out var r)) return;
        ref var bg = ref r.Value;
        if (Interlocked.Decrement(ref bg.RefCount) == 0)
        {
            ctx.Device.ReleaseBindGroup(bg.NativePtr);
            ctx._bindGroups.Destroy(handle);
        }
    }

    // --- PipelineLayout ---

    public static Handle<PipelineLayoutData> CreatePipelineLayout(
        GraphicsContext ctx, PipelineLayoutDescriptor descriptor,
        ReadOnlySpan<Handle<BindGroupLayoutData>> bindGroupLayouts)
    {
        nint native = ctx.Device.CreatePipelineLayout(ctx.NativeDevice, &descriptor);

        ulong* handles = null;
        if (bindGroupLayouts.Length > 0)
        {
            handles = (ulong*)NativeMemory.Alloc((nuint)(bindGroupLayouts.Length * sizeof(ulong)));
            for (int i = 0; i < bindGroupLayouts.Length; i++)
                handles[i] = bindGroupLayouts[i].Value;
        }

        return ctx._pipelineLayouts.Create(new PipelineLayoutData
        {
            NativePtr = native,
            BindGroupLayoutCount = (uint)bindGroupLayouts.Length,
            BindGroupLayoutHandles = handles,
            RefCount = 1
        });
    }

    internal static void ReleasePipelineLayout(GraphicsContext ctx, Handle<PipelineLayoutData> handle)
    {
        if (!ctx._pipelineLayouts.TryGet(handle, out var r)) return;
        ref var pl = ref r.Value;
        if (Interlocked.Decrement(ref pl.RefCount) == 0)
        {
            if (pl.BindGroupLayoutHandles != null)
                NativeMemory.Free(pl.BindGroupLayoutHandles);
            ctx.Device.ReleasePipelineLayout(pl.NativePtr);
            ctx._pipelineLayouts.Destroy(handle);
        }
    }

    // --- RenderPipeline (with dedup) ---

    public static Handle<RenderPipelineData> CreateRenderPipeline(
        GraphicsContext ctx,
        RenderPipelineDescriptor* descriptor,
        Handle<ShaderData> vertexShader,
        Handle<ShaderData> fragmentShader,
        Handle<PipelineLayoutData> layout)
    {
        if (!ctx._shaders.Contains(vertexShader) ||
            !ctx._shaders.Contains(fragmentShader) ||
            !ctx._pipelineLayouts.Contains(layout))
            throw new InvalidOperationException("Pipeline dependency not alive.");

        ulong vs = vertexShader.Value;
        ulong fs = fragmentShader.Value;
        ulong lo = layout.Value;

        // Dedup: search existing pipelines for matching shader+layout combination.
        var cursor = ctx._renderPipelines.Cursor();
        while (cursor.Next(out _, out var item, out _))
        {
            ref var existing = ref item.Value;
            if (existing.VertexShaderHandle == vs &&
                existing.FragmentShaderHandle == fs &&
                existing.LayoutHandle == lo)
            {
                Interlocked.Increment(ref existing.RefCount);
                return item.Pin();
            }
        }

        nint native = ctx.Device.CreateRenderPipeline(ctx.NativeDevice, descriptor);
        return ctx._renderPipelines.Create(new RenderPipelineData
        {
            NativePtr = native,
            VertexShaderHandle = vs,
            FragmentShaderHandle = fs,
            LayoutHandle = lo,
            RefCount = 1
        });
    }

    internal static void ReleaseRenderPipeline(GraphicsContext ctx, Handle<RenderPipelineData> handle)
    {
        if (!ctx._renderPipelines.TryGet(handle, out var r)) return;
        ref var rp = ref r.Value;
        if (Interlocked.Decrement(ref rp.RefCount) == 0)
        {
            ctx.Device.ReleaseRenderPipeline(rp.NativePtr);
            ctx._renderPipelines.Destroy(handle);
        }
    }

    // --- ComputePipeline (with dedup) ---

    public static Handle<ComputePipelineData> CreateComputePipeline(
        GraphicsContext ctx,
        ComputePipelineDescriptor* descriptor,
        Handle<ShaderData> computeShader,
        Handle<PipelineLayoutData> layout)
    {
        if (!ctx._shaders.Contains(computeShader) ||
            !ctx._pipelineLayouts.Contains(layout))
            throw new InvalidOperationException("Pipeline dependency not alive.");

        ulong cs = computeShader.Value;
        ulong lo = layout.Value;

        // Dedup.
        var cursor = ctx._computePipelines.Cursor();
        while (cursor.Next(out _, out var item, out _))
        {
            ref var existing = ref item.Value;
            if (existing.ComputeShaderHandle == cs && existing.LayoutHandle == lo)
            {
                Interlocked.Increment(ref existing.RefCount);
                return item.Pin();
            }
        }

        nint native = ctx.Device.CreateComputePipeline(ctx.NativeDevice, descriptor);
        return ctx._computePipelines.Create(new ComputePipelineData
        {
            NativePtr = native,
            ComputeShaderHandle = cs,
            LayoutHandle = lo,
            RefCount = 1
        });
    }

    internal static void ReleaseComputePipeline(GraphicsContext ctx, Handle<ComputePipelineData> handle)
    {
        if (!ctx._computePipelines.TryGet(handle, out var r)) return;
        ref var cp = ref r.Value;
        if (Interlocked.Decrement(ref cp.RefCount) == 0)
        {
            ctx.Device.ReleaseComputePipeline(cp.NativePtr);
            ctx._computePipelines.Destroy(handle);
        }
    }
}
