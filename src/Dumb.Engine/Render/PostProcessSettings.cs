using Sia;

namespace Dumb.Engine.Render;

public partial record struct PostProcessSettings(
    [Sia] float BloomIntensity = 0f,
    [Sia] float BloomThreshold = 1f,
    [Sia] float Exposure = 1f,
    [Sia] float AmbientR = 0.03f,
    [Sia] float AmbientG = 0.03f,
    [Sia] float AmbientB = 0.03f)
{
    public PostProcessSettings() : this(0f, 1f, 1f, 0.03f, 0.03f, 0.03f) { }
}
