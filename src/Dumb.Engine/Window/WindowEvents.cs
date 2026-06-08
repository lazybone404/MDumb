using Sia;

namespace Dumb.Engine.Window;

public readonly record struct WindowResizeEvent(int Width, int Height, int FramebufferWidth, int FramebufferHeight) : IEvent;
public readonly record struct WindowCloseEvent : IEvent;
