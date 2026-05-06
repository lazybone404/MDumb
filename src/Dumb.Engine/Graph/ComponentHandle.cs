namespace Dumb.Engine.Graph;

public readonly record struct ComponentHandle<T>(ulong Value)
    where T : unmanaged
{
    internal PointerData Data => new(Value);

    public nuint Index => Data.Index;

    public ushort Epoch => Data.Epoch;

    public byte StorageId => Data.StorageId;
}
