using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dumb.Engine.Graph;

public sealed unsafe class Storage<T> : IDisposable
    where T : unmanaged
{
    private T* _data;
    private ushort* _meta;
    private ushort* _epoch;
    private nuint _capacity;
    private readonly NativeBuffer<PointerData> _freeList;
    private bool _disposed;

    public Storage()
        : this(0)
    {
    }

    public Storage(nuint capacity)
    {
        if (capacity != 0)
        {
            _data = (T*)NativeMemory.Alloc(capacity, (nuint)sizeof(T));
            _meta = (ushort*)NativeMemory.Alloc(capacity, sizeof(ushort));
            _epoch = (ushort*)NativeMemory.Alloc(capacity, sizeof(ushort));
            NativeMemory.Clear(_meta, capacity * sizeof(ushort));
            NativeMemory.Clear(_epoch, capacity * sizeof(ushort));
            _capacity = capacity;
        }

        _freeList = new NativeBuffer<PointerData>();
        Id = StorageIdAllocator.Next();
    }

    public nuint Length { get; private set; }

    public ushort Id { get; }

    public ref T this[Handle<T> handle]
    {
        get
        {
            ThrowIfDisposed();
            return ref Get(handle);
        }
    }

    internal ushort Meta(nuint index) => _meta[index];

    internal ushort Epoch(nuint index) => _epoch[index];

    public static Storage<T> From(ReadOnlySpan<T> values)
    {
        var storage = new Storage<T>((nuint)values.Length);
        if (!values.IsEmpty)
        {
            values.CopyTo(new Span<T>(storage._data, values.Length));
            for (var i = 0; i < values.Length; i++)
            {
                storage._meta[i] = 1;
            }
        }

        storage.Length = (nuint)values.Length;
        return storage;
    }

    public Handle<T> Create(T value)
    {
        ThrowIfDisposed();
        if (_freeList.TryPop(out var data))
        {
            Store(data.Index, value);
        }
        else
        {
            var i = Length;
            EnsureCapacity(i + 1);
            Store(i, value);
            Length++;
            data = PointerData.New(i, _epoch[i], Id);
        }

        return new Handle<T>(data.Value);
    }

    public bool Destroy(Handle<T> handle)
    {
        ThrowIfDisposed();
        var data = handle.Data;
        if (!IsAlive(data))
        {
            return false;
        }

        _meta[data.Index] = 0;
        unchecked
        {
            _epoch[data.Index]++;
        }

        _freeList.Push(PointerData.New(data.Index, _epoch[data.Index], Id));
        return true;
    }

    public Iter<T> Iter()
    {
        ThrowIfDisposed();
        return new Iter<T>(_data, _meta, Length, skipLost: true);
    }

    public Iter<T> IterAll()
    {
        ThrowIfDisposed();
        return new Iter<T>(_data, _meta, Length, skipLost: false);
    }

    public IterMut<T> IterMut()
    {
        ThrowIfDisposed();
        return new IterMut<T>(_data, _meta, Length, skipLost: true);
    }

    public IterMut<T> IterAllMut()
    {
        ThrowIfDisposed();
        return new IterMut<T>(_data, _meta, Length, skipLost: false);
    }

    public Handle<T> Pin(Item<T> item)
    {
        ThrowIfDisposed();
        Debug.Assert(item.Index < Length);
        return new Handle<T>(PointerData.New(item.Index, _epoch[item.Index], Id).Value);
    }

    public bool Contains(Handle<T> handle)
    {
        ThrowIfDisposed();
        return IsAlive(handle.Data);
    }

    public bool TryGet(Handle<T> handle, out Ref<T> value)
    {
        ThrowIfDisposed();
        var data = handle.Data;
        if (!IsAlive(data))
        {
            value = default;
            return false;
        }

        value = new Ref<T>(_data + data.Index);
        return true;
    }

    public ref T Get(Handle<T> handle)
    {
        ThrowIfDisposed();
        var data = handle.Data;
        ThrowIfNotAlive(data);
        return ref _data[data.Index];
    }

    public ref T GetUnchecked(Handle<T> handle)
    {
        ThrowIfDisposed();
        return ref _data[handle.Index];
    }

    public void Split(Handle<T> handle, out Slice<T> left, out Ref<T> item, out Slice<T> right)
    {
        ThrowIfDisposed();
        var data = handle.Data;
        ThrowIfNotAlive(data);
        SplitRaw(data, out left, out var current, out right);
        item = new Ref<T>(current);
    }

    internal void SplitRaw(PointerData offset, out Slice<T> left, out T* item, out Slice<T> right)
    {
        var sid = offset.StorageId;
        var index = offset.Index;
        left = new Slice<T>(_data, index, PointerData.New(0, 0, sid));
        item = _data + index;
        right = new Slice<T>(_data + index + 1, Length - index - 1, PointerData.New(index + 1, 0, sid));
    }

    public Cursor<T> Cursor()
    {
        ThrowIfDisposed();
        return new Cursor<T>(this, 0);
    }

    public Cursor<T> CursorEnd()
    {
        ThrowIfDisposed();
        return new Cursor<T>(this, Length);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        NativeMemory.Free(_data);
        NativeMemory.Free(_meta);
        NativeMemory.Free(_epoch);
        _data = null;
        _meta = null;
        _epoch = null;
        _capacity = 0;
        Length = 0;
        _freeList.Dispose();
        StorageIdAllocator.Return(Id);
        _disposed = true;
    }

    private void EnsureCapacity(nuint capacity)
    {
        if (capacity <= _capacity)
        {
            return;
        }

        var next = GetNextCapacity(capacity);

        _data = (T*)NativeMemory.Realloc(_data, next * (nuint)sizeof(T));
        _meta = (ushort*)NativeMemory.Realloc(_meta, next * sizeof(ushort));
        _epoch = (ushort*)NativeMemory.Realloc(_epoch, next * sizeof(ushort));
        NativeMemory.Clear(_meta + _capacity, (next - _capacity) * sizeof(ushort));
        NativeMemory.Clear(_epoch + _capacity, (next - _capacity) * sizeof(ushort));
        _capacity = next;
    }

    private void Store(nuint index, T value)
    {
        Debug.Assert(_meta[index] == 0);
        _data[index] = value;
        _meta[index] = 1;
    }

    private nuint GetNextCapacity(nuint capacity)
    {
        var next = _capacity == 0 ? 4 : _capacity * 2;
        while (next < capacity)
        {
            next *= 2;
        }

        return next;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private bool IsAlive(PointerData data)
    {
        return data.StorageId == Id &&
            data.Index < Length &&
            _meta[data.Index] != 0 &&
            _epoch[data.Index] == data.Epoch;
    }

    private void ThrowIfNotAlive(PointerData data)
    {
        if (!IsAlive(data))
        {
            throw new InvalidHandleException();
        }
    }
}

internal static class StorageIdAllocator
{
    private static int _storageUid;
    private static readonly Lock _gate = new();
    private static readonly Stack<ushort> _freeIds = [];

    public static ushort Next()
    {
        lock (_gate)
        {
            if (_freeIds.TryPop(out var id))
            {
                return id;
            }

            var uid = ++_storageUid;
            if (uid > ushort.MaxValue)
            {
                throw new InvalidOperationException("The graph storage id space is exhausted.");
            }

            return (ushort)uid;
        }
    }

    public static void Return(ushort id)
    {
        if (id == 0)
        {
            return;
        }

        lock (_gate)
        {
            _freeIds.Push(id);
        }
    }
}

public readonly unsafe ref struct Item<T>
    where T : unmanaged
{
    private readonly T* _value;

    internal Item(T* value, nuint index)
    {
        _value = value;
        Index = index;
    }

    public nuint Index { get; }

    public ref readonly T Value => ref *_value;
}

public unsafe ref struct Iter<T>
    where T : unmanaged
{
    private readonly T* _data;
    private readonly ushort* _meta;
    private readonly nuint _length;
    private readonly bool _skipLost;
    private nuint _index;

    internal Iter(T* data, ushort* meta, nuint length, bool skipLost)
    {
        _data = data;
        _meta = meta;
        _length = length;
        _skipLost = skipLost;
        _index = 0;
    }

    public bool MoveNext(out Item<T> item)
    {
        while (_index < _length)
        {
            var id = _index++;
            if (!_skipLost || _meta[id] != 0)
            {
                item = new Item<T>(_data + id, id);
                return true;
            }
        }

        item = default;
        return false;
    }
}

public unsafe ref struct IterMut<T>
    where T : unmanaged
{
    private readonly T* _data;
    private readonly ushort* _meta;
    private readonly nuint _length;
    private readonly bool _skipLost;
    private nuint _front;
    private nuint _back;

    internal IterMut(T* data, ushort* meta, nuint length, bool skipLost)
    {
        _data = data;
        _meta = meta;
        _length = length;
        _skipLost = skipLost;
        _front = 0;
        _back = length;
    }

    public bool MoveNext(out Ref<T> item)
    {
        while (_front < _length)
        {
            var id = _front++;
            if (!_skipLost || _meta[id] != 0)
            {
                item = new Ref<T>(_data + id);
                return true;
            }
        }

        item = default;
        return false;
    }

    public bool MovePrevious(out Ref<T> item)
    {
        while (_back != 0)
        {
            var id = --_back;
            if (!_skipLost || _meta[id] != 0)
            {
                item = new Ref<T>(_data + id);
                return true;
            }
        }

        item = default;
        return false;
    }
}
