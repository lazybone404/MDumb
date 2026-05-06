using System.Diagnostics;

namespace Dumb.Engine.Graph;

public unsafe readonly ref struct Slice<T>
    where T : unmanaged
{
    private readonly T* _items;
    private readonly nuint _length;
    private readonly PointerData _offset;

    internal Slice(T* items, nuint length, PointerData offset)
    {
        _items = items;
        _length = length;
        _offset = offset;
    }

    public bool IsEmpty => _length == 0;

    public nuint Length => _length;

    public ref T this[nuint index] => ref _items[index];

    public bool TryGet(Pointer<T> pointer, out T* value)
    {
        Debug.Assert(pointer.Data.StorageId == _offset.StorageId);
        var index = pointer.Data.Index - _offset.Index;
        if (index >= _length)
        {
            value = null;
            return false;
        }

        value = _items + index;
        return true;
    }

    public bool TryGetRef(Pointer<T> pointer, out Ref<T> value)
    {
        if (TryGet(pointer, out var item))
        {
            value = new Ref<T>(item);
            return true;
        }

        value = default;
        return false;
    }
}

public unsafe readonly ref struct Ref<T>
    where T : unmanaged
{
    private readonly T* _value;

    internal Ref(T* value)
    {
        _value = value;
    }

    public ref T Value => ref *_value;
}
