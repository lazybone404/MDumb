using System.Threading;
using Sia;
using Silk.NET.WebGPU;
using WgpuBuffer = Silk.NET.WebGPU.Buffer;

namespace Dumb.Graphics;

public readonly struct BindingLayout
{
    internal readonly BindGroupLayoutEntry Entry;

    private BindingLayout(BindGroupLayoutEntry entry)
    {
        Entry = entry;
    }

    public static BindingLayout UniformBuffer(uint binding, ShaderStage visibility, ulong minBindingSize)
    {
        return Buffer(binding, visibility, BufferBindingType.Uniform, minBindingSize);
    }

    public static BindingLayout StorageBuffer(
        uint binding,
        ShaderStage visibility,
        ulong minBindingSize,
        bool readOnly = false)
    {
        return Buffer(binding, visibility, readOnly ? BufferBindingType.ReadOnlyStorage : BufferBindingType.Storage, minBindingSize);
    }

    public static BindingLayout Sampler(
        uint binding,
        ShaderStage visibility,
        SamplerBindingType type = SamplerBindingType.Filtering)
    {
        return new BindingLayout(new BindGroupLayoutEntry
        {
            Binding = binding,
            Visibility = visibility,
            Sampler = new SamplerBindingLayout { Type = type },
            Buffer = default,
            Texture = default,
            StorageTexture = default
        });
    }

    public static BindingLayout Texture(
        uint binding,
        ShaderStage visibility,
        TextureSampleType sampleType = TextureSampleType.Float,
        TextureViewDimension dimension = TextureViewDimension.Dimension2D,
        bool multisampled = false)
    {
        return new BindingLayout(new BindGroupLayoutEntry
        {
            Binding = binding,
            Visibility = visibility,
            Texture = new TextureBindingLayout
            {
                SampleType = sampleType,
                ViewDimension = dimension,
                Multisampled = multisampled
            },
            Buffer = default,
            Sampler = default,
            StorageTexture = default
        });
    }

    private static BindingLayout Buffer(
        uint binding,
        ShaderStage visibility,
        BufferBindingType type,
        ulong minBindingSize)
    {
        return new BindingLayout(new BindGroupLayoutEntry
        {
            Binding = binding,
            Visibility = visibility,
            Buffer = new BufferBindingLayout
            {
                Type = type,
                HasDynamicOffset = false,
                MinBindingSize = minBindingSize
            },
            Sampler = default,
            Texture = default,
            StorageTexture = default
        });
    }
}

public readonly struct Binding
{
    internal readonly uint Slot;
    internal readonly BindingKind Kind;
    internal readonly Entity Entity;
    internal readonly nuint Size;

    private Binding(uint slot, BindingKind kind, Entity entity, nuint size)
    {
        Slot = slot;
        Kind = kind;
        Entity = entity;
        Size = size;
    }

    public static Binding Buffer(uint slot, Entity buffer, nuint size)
    {
        return new Binding(slot, BindingKind.Buffer, buffer, size);
    }

    public static unsafe Binding Uniform<T>(uint slot, Entity buffer) where T : unmanaged
    {
        return Buffer(slot, buffer, (nuint)sizeof(T));
    }

    public static unsafe Binding Storage<T>(uint slot, Entity buffer, int elementCount) where T : unmanaged
    {
        return Buffer(slot, buffer, (nuint)(sizeof(T) * elementCount));
    }

    public static Binding Texture(uint slot, Entity textureView)
    {
        return new Binding(slot, BindingKind.TextureView, textureView, 0);
    }

    public static Binding Sampler(uint slot, Entity sampler)
    {
        return new Binding(slot, BindingKind.Sampler, sampler, 0);
    }
}

internal enum BindingKind
{
    Buffer,
    TextureView,
    Sampler
}

public readonly struct VertexAttributeLayout
{
    public readonly uint ShaderLocation;
    public readonly VertexFormat Format;
    public readonly ulong Offset;

    public VertexAttributeLayout(uint shaderLocation, VertexFormat format, ulong offset)
    {
        ShaderLocation = shaderLocation;
        Format = format;
        Offset = offset;
    }
}

public static unsafe class Pipelines
{
    // --- BindGroupLayout ---

    public static Entity BindGroupLayout(
        GraphicsContext ctx,
        ReadOnlySpan<BindingLayout> entries)
    {
        BindGroupLayoutEntry* nativeEntries = stackalloc BindGroupLayoutEntry[entries.Length];
        for (var i = 0; i < entries.Length; i++)
            nativeEntries[i] = entries[i].Entry;

        BindGroupLayoutDescriptor descriptor = new()
        {
            EntryCount = (nuint)entries.Length,
            Entries = nativeEntries,
            Label = null
        };
        return CreateBindGroupLayout(ctx, descriptor);
    }

    public static Entity CreateBindGroupLayout(GraphicsContext ctx, BindGroupLayoutDescriptor descriptor)
    {
        nint native = ctx.Device.CreateBindGroupLayout(ctx.NativeDevice, &descriptor);
        return ctx._bindGroupLayouts.Create(HList.From(new BindGroupLayoutData { NativePtr = native, RefCount = 1 }));
    }

    internal static void ReleaseBindGroupLayout(GraphicsContext ctx, Entity bgl)
    {
        ref var data = ref bgl.Get<BindGroupLayoutData>();
        if (Interlocked.Decrement(ref data.RefCount) == 0)
        {
            ctx.Device.ReleaseBindGroupLayout(data.NativePtr);
            bgl.Destroy();
        }
    }

    // --- BindGroup ---

    public static Entity BindGroup(
        GraphicsContext ctx,
        Entity layout,
        ReadOnlySpan<Binding> bindings)
    {
        BindGroupEntry* entries = stackalloc BindGroupEntry[bindings.Length];
        for (var i = 0; i < bindings.Length; i++)
        {
            var binding = bindings[i];
            entries[i] = new BindGroupEntry
            {
                Binding = binding.Slot,
                Offset = 0,
                Size = binding.Size,
                Buffer = null,
                TextureView = null,
                Sampler = null
            };

            switch (binding.Kind)
            {
                case BindingKind.Buffer:
                    entries[i].Buffer = (WgpuBuffer*)binding.Entity.Get<BufferData>().NativePtr;
                    break;
                case BindingKind.TextureView:
                    entries[i].TextureView = (TextureView*)binding.Entity.Get<TextureViewData>().NativePtr;
                    break;
                case BindingKind.Sampler:
                    entries[i].Sampler = (Sampler*)binding.Entity.Get<SamplerData>().NativePtr;
                    break;
            }
        }

        BindGroupDescriptor descriptor = new()
        {
            Layout = (BindGroupLayout*)layout.Get<BindGroupLayoutData>().NativePtr,
            EntryCount = (nuint)bindings.Length,
            Entries = entries,
            Label = null
        };
        return CreateBindGroup(ctx, descriptor, layout);
    }

    public static Entity CreateBindGroup(
        GraphicsContext ctx, BindGroupDescriptor descriptor, Entity layout)
    {
        if (layout.Host == null)
            throw new InvalidOperationException("BindGroupLayout entity not alive.");

        nint native = ctx.Device.CreateBindGroup(ctx.NativeDevice, &descriptor);
        return ctx._bindGroups.Create(HList.From(new BindGroupData
        {
            NativePtr = native,
            Layout = layout,
            RefCount = 1
        }));
    }

    internal static void ReleaseBindGroup(GraphicsContext ctx, Entity bindGroup)
    {
        ref var bg = ref bindGroup.Get<BindGroupData>();
        if (Interlocked.Decrement(ref bg.RefCount) == 0)
        {
            ctx.Device.ReleaseBindGroup(bg.NativePtr);
            bindGroup.Destroy();
        }
    }

    // --- PipelineLayout ---

    public static Entity Layout(
        GraphicsContext ctx,
        ReadOnlySpan<Entity> bindGroupLayouts)
    {
        BindGroupLayout** nativeLayouts = stackalloc BindGroupLayout*[bindGroupLayouts.Length];
        for (var i = 0; i < bindGroupLayouts.Length; i++)
            nativeLayouts[i] = (BindGroupLayout*)bindGroupLayouts[i].Get<BindGroupLayoutData>().NativePtr;

        PipelineLayoutDescriptor descriptor = new()
        {
            BindGroupLayoutCount = (nuint)bindGroupLayouts.Length,
            BindGroupLayouts = nativeLayouts,
            Label = null
        };
        return CreatePipelineLayout(ctx, descriptor, bindGroupLayouts);
    }

    public static Entity CreatePipelineLayout(
        GraphicsContext ctx, PipelineLayoutDescriptor descriptor,
        ReadOnlySpan<Entity> bindGroupLayouts)
    {
        nint native = ctx.Device.CreatePipelineLayout(ctx.NativeDevice, &descriptor);

        var handles = bindGroupLayouts.Length > 0
            ? bindGroupLayouts.ToArray()
            : null;

        return ctx._pipelineLayouts.Create(HList.From(new PipelineLayoutData
        {
            NativePtr = native,
            BindGroupLayoutCount = (uint)bindGroupLayouts.Length,
            BindGroupLayouts = handles,
            RefCount = 1
        }));
    }

    internal static void ReleasePipelineLayout(GraphicsContext ctx, Entity pipelineLayout)
    {
        ref var pl = ref pipelineLayout.Get<PipelineLayoutData>();
        if (Interlocked.Decrement(ref pl.RefCount) == 0)
        {
            ctx.Device.ReleasePipelineLayout(pl.NativePtr);
            pipelineLayout.Destroy();
        }
    }

    // --- RenderPipeline (with dedup) ---

    public static Entity Render(
        GraphicsContext ctx,
        Entity shader,
        Entity layout,
        TextureFormat colorFormat,
        string vertexEntryPoint = "vs_main",
        string fragmentEntryPoint = "fs_main")
    {
        return Render(
            ctx,
            shader,
            layout,
            colorFormat,
            ReadOnlySpan<VertexAttributeLayout>.Empty,
            vertexStride: 0,
            vertexEntryPoint,
            fragmentEntryPoint);
    }

    public static Entity Render(
        GraphicsContext ctx,
        Entity shader,
        Entity layout,
        TextureFormat colorFormat,
        ReadOnlySpan<VertexAttributeLayout> attributes,
        ulong vertexStride,
        string vertexEntryPoint = "vs_main",
        string fragmentEntryPoint = "fs_main")
    {
        var vsBytes = System.Text.Encoding.UTF8.GetBytes(vertexEntryPoint + '\0');
        var fsBytes = System.Text.Encoding.UTF8.GetBytes(fragmentEntryPoint + '\0');
        fixed (byte* vsPtr = vsBytes)
        fixed (byte* fsPtr = fsBytes)
        {
            Span<VertexAttribute> nativeAttributes = stackalloc VertexAttribute[attributes.Length];
            Span<VertexBufferLayout> vertexBuffers = stackalloc VertexBufferLayout[attributes.Length > 0 ? 1 : 0];

            if (attributes.Length > 0)
            {
                fixed (VertexAttribute* nativeAttributePtr = nativeAttributes)
                fixed (VertexBufferLayout* vertexBufferPtr = vertexBuffers)
                {
                    for (var i = 0; i < attributes.Length; i++)
                    {
                        nativeAttributePtr[i] = new VertexAttribute
                        {
                            ShaderLocation = attributes[i].ShaderLocation,
                            Format = attributes[i].Format,
                            Offset = attributes[i].Offset
                        };
                    }

                    vertexBufferPtr[0] = new VertexBufferLayout
                    {
                        ArrayStride = vertexStride,
                        StepMode = VertexStepMode.Vertex,
                        AttributeCount = (nuint)attributes.Length,
                        Attributes = nativeAttributePtr
                    };

                    return RenderCore(ctx, shader, layout, colorFormat, vsPtr, fsPtr, vertexBufferPtr, 1);
                }
            }

            return RenderCore(ctx, shader, layout, colorFormat, vsPtr, fsPtr, null, 0);
        }
    }

    static Entity RenderCore(
        GraphicsContext ctx,
        Entity shader,
        Entity layout,
        TextureFormat colorFormat,
        byte* vertexEntryPoint,
        byte* fragmentEntryPoint,
        VertexBufferLayout* vertexBuffers,
        uint vertexBufferCount)
    {
        ColorTargetState colorTarget = new()
        {
            Format = colorFormat,
            Blend = null,
            WriteMask = ColorWriteMask.All
        };

        FragmentState fragment = new()
        {
            Module = (ShaderModule*)shader.Get<ShaderData>().NativePtr,
            EntryPoint = fragmentEntryPoint,
            TargetCount = 1,
            Targets = &colorTarget
        };

        RenderPipelineDescriptor descriptor = new()
        {
            Layout = (PipelineLayout*)layout.Get<PipelineLayoutData>().NativePtr,
            Vertex = new VertexState
            {
                Module = (ShaderModule*)shader.Get<ShaderData>().NativePtr,
                EntryPoint = vertexEntryPoint,
                BufferCount = vertexBufferCount,
                Buffers = vertexBuffers
            },
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
                Mask = uint.MaxValue,
                AlphaToCoverageEnabled = false
            },
            Fragment = &fragment,
            DepthStencil = null,
            Label = null
        };

        return CreateRenderPipeline(ctx, &descriptor, shader, shader, layout);
    }

    public static Entity CreateRenderPipeline(
        GraphicsContext ctx,
        RenderPipelineDescriptor* descriptor,
        Entity vertexShader,
        Entity fragmentShader,
        Entity layout)
    {
        if (vertexShader.Host == null ||
            fragmentShader.Host == null ||
            layout.Host == null)
            throw new InvalidOperationException("Pipeline dependency not alive.");

        int vsId = vertexShader.Id.Value;
        int fsId = fragmentShader.Id.Value;
        int loId = layout.Id.Value;

        // Dedup: search existing pipelines for matching shader+layout combination.
        foreach (var entity in ctx._renderPipelines)
        {
            ref var existing = ref entity.Get<RenderPipelineData>();
            if (existing.VertexShader.Id.Value == vsId &&
                existing.FragmentShader.Id.Value == fsId &&
                existing.Layout.Id.Value == loId)
            {
                Interlocked.Increment(ref existing.RefCount);
                return entity;
            }
        }

        nint native = ctx.Device.CreateRenderPipeline(ctx.NativeDevice, descriptor);
        return ctx._renderPipelines.Create(HList.From(new RenderPipelineData
        {
            NativePtr = native,
            VertexShader = vertexShader,
            FragmentShader = fragmentShader,
            Layout = layout,
            RefCount = 1
        }));
    }

    internal static void ReleaseRenderPipeline(GraphicsContext ctx, Entity pipeline)
    {
        ref var rp = ref pipeline.Get<RenderPipelineData>();
        if (Interlocked.Decrement(ref rp.RefCount) == 0)
        {
            ctx.Device.ReleaseRenderPipeline(rp.NativePtr);
            pipeline.Destroy();
        }
    }

    // --- ComputePipeline (with dedup) ---

    public static Entity Compute(
        GraphicsContext ctx,
        Entity shader,
        Entity layout,
        string entryPoint = "cs_main")
    {
        var entryBytes = System.Text.Encoding.UTF8.GetBytes(entryPoint + '\0');
        fixed (byte* entryPtr = entryBytes)
        {
            ComputePipelineDescriptor descriptor = new()
            {
                Layout = (PipelineLayout*)layout.Get<PipelineLayoutData>().NativePtr,
                Compute = new ProgrammableStageDescriptor
                {
                    Module = (ShaderModule*)shader.Get<ShaderData>().NativePtr,
                    EntryPoint = entryPtr,
                    ConstantCount = 0,
                    Constants = null
                },
                Label = null
            };
            return CreateComputePipeline(ctx, &descriptor, shader, layout);
        }
    }

    public static Entity CreateComputePipeline(
        GraphicsContext ctx,
        ComputePipelineDescriptor* descriptor,
        Entity computeShader,
        Entity layout)
    {
        if (computeShader.Host == null || layout.Host == null)
            throw new InvalidOperationException("Pipeline dependency not alive.");

        int csId = computeShader.Id.Value;
        int loId = layout.Id.Value;

        // Dedup.
        foreach (var entity in ctx._computePipelines)
        {
            ref var existing = ref entity.Get<ComputePipelineData>();
            if (existing.ComputeShader.Id.Value == csId && existing.Layout.Id.Value == loId)
            {
                Interlocked.Increment(ref existing.RefCount);
                return entity;
            }
        }

        nint native = ctx.Device.CreateComputePipeline(ctx.NativeDevice, descriptor);
        return ctx._computePipelines.Create(HList.From(new ComputePipelineData
        {
            NativePtr = native,
            ComputeShader = computeShader,
            Layout = layout,
            RefCount = 1
        }));
    }

    internal static void ReleaseComputePipeline(GraphicsContext ctx, Entity pipeline)
    {
        ref var cp = ref pipeline.Get<ComputePipelineData>();
        if (Interlocked.Decrement(ref cp.RefCount) == 0)
        {
            ctx.Device.ReleaseComputePipeline(cp.NativePtr);
            pipeline.Destroy();
        }
    }
}
