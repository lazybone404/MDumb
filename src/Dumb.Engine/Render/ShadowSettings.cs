using Sia;

namespace Dumb.Engine.Render;

public partial record struct ShadowSettings(
    [Sia] float MaxDistance = 500f,
    [Sia] int CascadeCount = 4,
    [Sia] float DepthBias = 0.001f,
    [Sia] float NormalBias = 0.5f)
{
    public ShadowSettings() : this(500f, 4, 0.001f, 0.5f) { }
}
