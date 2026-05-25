using Sia;

namespace Dumb.Graphics.Pipeline;

public readonly record struct ResourceHandle(Entity View, string Name)
{
    public bool IsValid => View?.Host != null;
}
