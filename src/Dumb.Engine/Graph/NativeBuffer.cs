using System.Runtime.InteropServices;

namespace Dumb.Engine.Graph;

internal unsafe sealed class NativeBuffer<T> : IDisposable
    where T : unmanaged
{
    private T* _items;
    private nuint _capacity;

    public NativeBuffer(nuint capacity = 0)
    {
        if (capacity != 0)
        {
            _items = (T*)NativeMemory.Alloc(capacity, (nuint)sizeof(T));
            _capacity = capacity;
        }
    }

    public nuint Length { get; private set; }

    public nuint Capacity => _capacity;

    public bool IsDisposed => _items == null && _capacity != 0;

    public ref T this[nuint index] => ref _items[index];

    public T* Pointer => _items;

    public void Push(T value)
    {
        EnsureCapacity(Length + 1);
        _items[Length++] = value;
    }

    public bool TryPop(out T value)
    {
        if (Length == 0)
        {
            value = default;
            return false;
        }

        value = _items[--Length];
        return true;
    }

    public void Clear()
    {
        Length = 0;
    }

    public void SetLength(nuint length)
    {
        EnsureCapacity(length);
        if (length > Length)
        {
            NativeMemory.Clear(_items + Length, (length - Length) * (nuint)sizeof(T));
        }

        Length = length;
    }

    public void EnsureCapacity(nuint capacity)
    {
        if (capacity <= _capacity)
        {
            return;
        }

        var next = _capacity == 0 ? (nuint)4 : _capacity * 2;
        while (next < capacity)
        {
            next *= 2;
        }

        _items = (T*)NativeMemory.Realloc(_items, next * (nuint)sizeof(T));
        _capacity = next;
    }

    public void Dispose()
    {
        NativeMemory.Free(_items);
        _items = null;
        _capacity = 0;
        Length = 0;
    }
}
