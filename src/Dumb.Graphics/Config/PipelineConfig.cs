namespace Dumb.Graphics.Config;

public sealed class PipelineConfig
{
    public bool EnableShadows;
    public bool EnableSSAO;
    public bool EnablePostProcess;
    public int MaxLights = 64;
    public bool EnableMotionVectors;
    public float GbufferScale = 1f;

    public static readonly PipelineConfig Default = new();
}
