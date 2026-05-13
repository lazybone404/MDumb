namespace Dumb.Engine.Window;

public readonly record struct WindowDescriptor(
    int Width = 1280,
    int Height = 720,
    string Title = "",
    bool Visible = true);
