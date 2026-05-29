namespace Dumb.Graphics.Config;

public sealed class FrameConfig
{
    public bool ShadowsEnabled;
    public float ShadowDistance;
    public bool SSAOEnabled;
    public bool PostProcessEnabled;
    public int MaxLights;

    public static FrameConfig FromPipelineConfig(PipelineConfig cfg)
    {
        return new FrameConfig
        {
            ShadowsEnabled = cfg.EnableShadows,
            ShadowDistance = 500f,
            SSAOEnabled = cfg.EnableSSAO,
            PostProcessEnabled = cfg.EnablePostProcess,
            MaxLights = cfg.MaxLights,
        };
    }

    public void CopyFrom(FrameConfig source)
    {
        ShadowsEnabled = source.ShadowsEnabled;
        ShadowDistance = source.ShadowDistance;
        SSAOEnabled = source.SSAOEnabled;
        PostProcessEnabled = source.PostProcessEnabled;
        MaxLights = source.MaxLights;
    }
}
