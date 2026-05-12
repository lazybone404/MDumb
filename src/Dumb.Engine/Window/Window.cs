using Sia;

namespace Dumb.Engine.Window;

public partial record struct Window(
    [Sia] int Width = 1280,
    [Sia] int Height = 720,
    [Sia] int FramebufferWidth = 1280,
    [Sia] int FramebufferHeight = 720,
    [Sia] bool ShouldClose = false);
