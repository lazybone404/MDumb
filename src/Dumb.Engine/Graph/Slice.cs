using System.Diagnostics;

namespace Dumb.Engine.Graph;

public readonly unsafe ref struct Slice<T>
    where T : unmanaged
{
    private readonly T* _items;
    private readonly PointerData _offset;

    internal Slice(T* items, nuint length, PointerData offset)
    {
        _items = items;
        Length = length;
        _offset = offset;
    }

    public bool IsEmpty => Length == 0;

    public nuint Length { get; }

    public ref T this[nuint index] => ref _items[index];

    public bool TryGet(Handle<T> handle, out T* value)
    {
        var data = handle.Data;
        Debug.Assert(data.StorageId == _offset.StorageId);
        var index = data.Index - _offset.Index;
        if (index >= Length)
        {
            value = null;
            return false;
        }

        value = _items + index;
        return true;
    }

    public bool TryGetRef(Handle<T> handle, out Ref<T> value)
    {
        if (TryGet(handle, out var item))
        {
            value = new Ref<T>(item);
            return true;
        }

        value = default;
        return false;
    }
}

public readonly unsafe ref struct Ref<T>
    where T : unmanaged
{
    private readonly T* _value;

    internal Ref(T* value) => _value = value;

    public ref T Value => ref *_value;
}
