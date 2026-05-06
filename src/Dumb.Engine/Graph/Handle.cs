namespace Dumb.Engine.Graph;

public sealed class InvalidHandleException()
    : Exception("The component handle does not refer to a live component.");

public readonly record struct Handle<T>(ulong Value)
    where T : unmanaged
{
    internal PointerData Data => new(Value);

    public bool IsDefault => Value == 0;

    public nuint Index => Data.Index;

    public ushort Epoch => Data.Epoch;

    public ushort StorageId => Data.StorageId;

    public void Deconstruct(out nuint index, out ushort epoch, out ushort storageId)
    {
        var data = Data;
        index = data.Index;
        epoch = data.Epoch;
        storageId = data.StorageId;
    }

    public override string ToString() =>
        $"Handle<{typeof(T).Name}> {{ index = {Index}, epoch = {Epoch}, storage_id = {StorageId} }}";
}
