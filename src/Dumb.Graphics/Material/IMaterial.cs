using Sia;

namespace Dumb.Graphics;

public interface IMaterial
{
    /// <summary>材质名称，用于调试和日志。</summary>
    public static abstract string Name { get; }

    /// <summary>
    /// 材质配置 — 包含顶点描述符、BindGroup 布局、颜色格式、混合、深度模板等。
    /// 由 MaterialManager.Create&lt;T&gt;() 在构造 Pipeline 时读取。
    /// </summary>
    public static abstract MaterialConfig Config { get; }

    /// <summary>获取或编译此材质的着色器。</summary>
    public Entity GetShader(GraphicsContext ctx);

    /// <summary>
    /// 为此材质创建 BindGroup 数组。
    /// 返回数组中索引 0 通常为 null（留给渲染器的 Frame bind group）。
    /// </summary>
    public Entity?[] CreateBindGroups(GraphicsContext ctx, Entity pipelineLayout);
}
