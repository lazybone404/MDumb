using Silk.NET.WebGPU;
using Dumb.Graphics.Material;

namespace Dumb.Graphics.Tests.Material;

public sealed class MaterialConfigTests
{
    [Fact]
    public void PBRMaterial_Config_HasCorrectColorFormats()
    {
        var config = PBRMaterial.Config;

        Assert.NotNull(config);
        // PBR 写入 3 个 GBuffer RT: BaseColor, NormalRoughness, PBR
        Assert.Equal(3, config.ColorFormats.Length);
        Assert.Equal(TextureFormat.Rgba8Unorm, config.ColorFormats[0]);
        Assert.Equal(TextureFormat.Rgba16float, config.ColorFormats[1]);
        Assert.Equal(TextureFormat.Rgba8Unorm, config.ColorFormats[2]);
    }

    [Fact]
    public void PBRMaterial_Config_HasTwoBindGroupLayouts()
    {
        var config = PBRMaterial.Config;

        Assert.Equal(2, config.BindGroupLayouts.Length);
        // Group 0: Frame (camera + model)
        // Group 1: Material (uniforms + textures + sampler)
    }

    [Fact]
    public void PBRMaterial_Config_HasDepthStencilEnabled()
    {
        var config = PBRMaterial.Config;

        Assert.NotNull(config.DepthStencil);
        Assert.Equal(TextureFormat.Depth32float, config.DepthStencil!.Value.Format);
        Assert.True(config.DepthStencil!.Value.DepthWriteEnabled);
    }

    [Fact]
    public void PBRMaterial_Name_IsCorrect()
    {
        Assert.Equal("PBR", PBRMaterial.Name);
    }

    [Fact]
    public void UnlitMaterial_Config_HasSingleColorFormat()
    {
        var config = UnlitMaterial.Config;

        Assert.NotNull(config);
        Assert.Single(config.ColorFormats);
        Assert.Equal(TextureFormat.Rgba8Unorm, config.ColorFormats[0]);
    }

    [Fact]
    public void UnlitMaterial_Config_HasTwoBindGroupLayouts()
    {
        var config = UnlitMaterial.Config;

        Assert.Equal(2, config.BindGroupLayouts.Length);
        // Group 0: Frame (camera + model)
        // Group 1: Material uniforms
    }

    [Fact]
    public void UnlitMaterial_Config_HasDepthStencil()
    {
        var config = UnlitMaterial.Config;

        Assert.NotNull(config.DepthStencil);
        Assert.Equal(TextureFormat.Depth32float, config.DepthStencil!.Value.Format);
        Assert.True(config.DepthStencil!.Value.DepthWriteEnabled);
        Assert.Equal(CompareFunction.Less, config.DepthStencil!.Value.DepthCompare);
    }

    [Fact]
    public void UnlitMaterial_Config_BlendIsNull()
    {
        var config = UnlitMaterial.Config;
        Assert.Null(config.Blend);
    }

    [Fact]
    public void UnlitMaterial_Name_IsCorrect()
    {
        Assert.Equal("Unlit", UnlitMaterial.Name);
    }

    [Fact]
    public void DeferredLightingMaterial_Config_HasDefaultColorFormat()
    {
        var config = DeferredLightingMaterial.Config;

        Assert.NotNull(config);
        Assert.Single(config.ColorFormats);
        Assert.Equal(TextureFormat.Rgba8Unorm, config.ColorFormats[0]);
    }

    [Fact]
    public void DeferredLightingMaterial_Config_HasThreeBindGroupLayouts()
    {
        var config = DeferredLightingMaterial.Config;

        Assert.Equal(3, config.BindGroupLayouts.Length);
        // Group 0: Frame (camera uniforms)
        // Group 1: G-buffer textures + sampler
        // Group 2: Lights
    }

    [Fact]
    public void DeferredLightingMaterial_Config_HasEmptyVertexDescriptor()
    {
        var config = DeferredLightingMaterial.Config;

        Assert.Empty(config.VertexDescriptor.Streams);
    }

    [Fact]
    public void DeferredLightingMaterial_Config_HasNoBlendOrDepthStencil()
    {
        var config = DeferredLightingMaterial.Config;

        Assert.Null(config.Blend);
        // 延迟光照是全屏四边形，不需要深度测试（但由管线外部管理深度）
    }

    [Fact]
    public void DeferredLightingMaterial_Name_IsCorrect()
    {
        Assert.Equal("DeferredLighting", DeferredLightingMaterial.Name);
    }

    [Fact]
    public void MaterialConfig_DefaultValues_AreCorrect()
    {
        var config = new MaterialConfig
        {
            VertexDescriptor = new Engine.Mesh.MeshDescriptor([]),
            BindGroupLayouts = []
        };

        // 默认 ColorFormats
        Assert.Single(config.ColorFormats);
        Assert.Equal(TextureFormat.Rgba8Unorm, config.ColorFormats[0]);

        // 默认 Blend 和 DepthStencil 为 null
        Assert.Null(config.Blend);
        Assert.Null(config.DepthStencil);
    }
}
