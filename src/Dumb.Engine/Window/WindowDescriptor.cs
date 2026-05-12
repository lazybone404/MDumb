namespace Dumb.Engine.Window;

public record struct WindowDescriptor
{
    public int Width;
    public int Height;
    public string Title;
    public bool Visible;

    public WindowDescriptor()
    {
        Width = 1280;
        Height = 720;
        Title = "";
        Visible = true;
    }
}
