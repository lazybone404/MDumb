namespace Dumb.Engine.Graph;

internal sealed class Pending : IDisposable
{
    private readonly Lock _gate = new();

    public NativeBuffer<nuint> AddRef { get; } = new();

    public NativeBuffer<nuint> SubRef { get; } = new();

    public NativeBuffer<ushort> Epoch { get; } = new();

    public Lock Gate => _gate;

    public ushort GetEpoch(nuint index)
    {
        return index < Epoch.Length ? Epoch[index] : (ushort)0;
    }

    public void Dispose()
    {
        AddRef.Dispose();
        SubRef.Dispose();
        Epoch.Dispose();
    }
}
