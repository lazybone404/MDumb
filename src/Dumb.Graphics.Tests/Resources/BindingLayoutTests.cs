using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics.Tests.Resources;

public sealed class BindingLayoutTests
{
    [Fact]
    public void UniformBuffer_CreatesCorrectEntry()
    {
        var layout = BindingLayout.UniformBuffer(0, ShaderStage.Vertex, 64);

        Assert.Equal(0u, layout.Entry.Binding);
        Assert.Equal(ShaderStage.Vertex, layout.Entry.Visibility);
        Assert.Equal(BufferBindingType.Uniform, layout.Entry.Buffer.Type);
        Assert.Equal(64ul, layout.Entry.Buffer.MinBindingSize);
        Assert.False(layout.Entry.Buffer.HasDynamicOffset);
    }

    [Fact]
    public void UniformBuffer_WithDynamicOffset_SetsFlag()
    {
        var layout = BindingLayout.UniformBuffer(1, ShaderStage.Fragment, 256, hasDynamicOffset: true);

        Assert.Equal(1u, layout.Entry.Binding);
        Assert.Equal(ShaderStage.Fragment, layout.Entry.Visibility);
        Assert.True(layout.Entry.Buffer.HasDynamicOffset);
    }

    [Fact]
    public void StorageBuffer_CreatesCorrectEntry()
    {
        var layout = BindingLayout.StorageBuffer(2, ShaderStage.Compute, 128);

        Assert.Equal(2u, layout.Entry.Binding);
        Assert.Equal(ShaderStage.Compute, layout.Entry.Visibility);
        Assert.Equal(BufferBindingType.Storage, layout.Entry.Buffer.Type);
        Assert.Equal(128ul, layout.Entry.Buffer.MinBindingSize);
    }

    [Fact]
    public void StorageBuffer_ReadOnly_SetsReadOnlyStorage()
    {
        var layout = BindingLayout.StorageBuffer(0, ShaderStage.Fragment, 64, readOnly: true);

        Assert.Equal(BufferBindingType.ReadOnlyStorage, layout.Entry.Buffer.Type);
    }

    [Fact]
    public void Sampler_CreatesCorrectEntry()
    {
        var layout = BindingLayout.Sampler(3, ShaderStage.Fragment, SamplerBindingType.Comparison);

        Assert.Equal(3u, layout.Entry.Binding);
        Assert.Equal(ShaderStage.Fragment, layout.Entry.Visibility);
        Assert.Equal(SamplerBindingType.Comparison, layout.Entry.Sampler.Type);
    }

    [Fact]
    public void Sampler_DefaultType_IsFiltering()
    {
        var layout = BindingLayout.Sampler(0, ShaderStage.Fragment);

        Assert.Equal(SamplerBindingType.Filtering, layout.Entry.Sampler.Type);
    }

    [Fact]
    public void Texture_CreatesCorrectEntry()
    {
        var layout = BindingLayout.Texture(1, ShaderStage.Fragment,
            TextureSampleType.Float, TextureViewDimension.Dimension2D, multisampled: false);

        Assert.Equal(1u, layout.Entry.Binding);
        Assert.Equal(ShaderStage.Fragment, layout.Entry.Visibility);
        Assert.Equal(TextureSampleType.Float, layout.Entry.Texture.SampleType);
        Assert.Equal(TextureViewDimension.Dimension2D, layout.Entry.Texture.ViewDimension);
        Assert.False(layout.Entry.Texture.Multisampled);
    }

    [Fact]
    public void Texture_DepthSampleType_SetsCorrectly()
    {
        var layout = BindingLayout.Texture(4, ShaderStage.Fragment,
            TextureSampleType.Depth, TextureViewDimension.Dimension2D);

        Assert.Equal(TextureSampleType.Depth, layout.Entry.Texture.SampleType);
    }

    [Fact]
    public void Texture_Multisampled_SetsFlag()
    {
        var layout = BindingLayout.Texture(0, ShaderStage.Fragment,
            TextureSampleType.Float, TextureViewDimension.Dimension2D, multisampled: true);

        Assert.True(layout.Entry.Texture.Multisampled);
    }

    [Fact]
    public void Binding_Buffer_CreatesCorrectBinding()
    {
        // 注：这只需要 struct 构造，不需要真实的 GPU Entity
        var entity = default(Entity);
        var binding = Binding.Buffer(0, entity, 64);

        Assert.Equal(0u, binding.Slot);
        Assert.Equal(BindingKind.Buffer, binding.Kind);
        Assert.Equal((nuint)64, binding.Size);
    }

    [Fact]
    public void Binding_Texture_CreatesCorrectBinding()
    {
        var entity = default(Entity);
        var binding = Binding.Texture(1, entity);

        Assert.Equal(1u, binding.Slot);
        Assert.Equal(BindingKind.TextureView, binding.Kind);
        Assert.Equal((nuint)0, binding.Size);
    }

    [Fact]
    public void Binding_Sampler_CreatesCorrectBinding()
    {
        var entity = default(Entity);
        var binding = Binding.Sampler(2, entity);

        Assert.Equal(2u, binding.Slot);
        Assert.Equal(BindingKind.Sampler, binding.Kind);
    }

    [Fact]
    public void VertexAttributeLayout_HasCorrectProperties()
    {
        var layout = new VertexAttributeLayout(0, VertexFormat.Float32x3, 16);

        Assert.Equal(0u, layout.ShaderLocation);
        Assert.Equal(VertexFormat.Float32x3, layout.Format);
        Assert.Equal(16ul, layout.Offset);
    }

    [Fact]
    public void VertexBufferLayoutDescriptor_DefaultStepMode_IsVertex()
    {
        var desc = new VertexBufferLayoutDescriptor([], 32);

        Assert.Equal(VertexStepMode.Vertex, desc.StepMode);
        Assert.Equal(32ul, desc.Stride);
    }

    [Fact]
    public void VertexBufferLayoutDescriptor_InstanceStepMode()
    {
        var desc = new VertexBufferLayoutDescriptor([], 48, VertexStepMode.Instance);

        Assert.Equal(VertexStepMode.Instance, desc.StepMode);
    }
}
