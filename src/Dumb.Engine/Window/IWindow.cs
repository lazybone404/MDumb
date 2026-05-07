namespace Dumb.Engine.Window;

public interface IWindow : IDisposable
{
    nint NativeHandle { get; }
    int Width { get; }
    int Height { get; }
    bool ShouldClose { get; }
    void PollEvents();
}
