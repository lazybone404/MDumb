namespace Dumb.Engine.Window;

public sealed class WindowEventSink
{
    private bool _hasResize;
    private int _width;
    private int _height;
    private int _framebufferWidth;
    private int _framebufferHeight;
    private bool _closeRequested;

    public void Resize(int width, int height, int framebufferWidth, int framebufferHeight)
    {
        _hasResize = true;
        _width = width;
        _height = height;
        _framebufferWidth = framebufferWidth;
        _framebufferHeight = framebufferHeight;
    }

    public void CloseRequested()
    {
        _closeRequested = true;
    }

    public bool ConsumeResize(out int width, out int height, out int framebufferWidth, out int framebufferHeight)
    {
        width = _width;
        height = _height;
        framebufferWidth = _framebufferWidth;
        framebufferHeight = _framebufferHeight;

        if (!_hasResize) return false;

        _hasResize = false;
        return true;
    }

    public bool ConsumeCloseRequested()
    {
        if (!_closeRequested) return false;

        _closeRequested = false;
        return true;
    }
}
