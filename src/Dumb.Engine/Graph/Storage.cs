using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dumb.Engine.Graph;

public unsafe sealed class Storage<T> : IDisposable
    where T : unmanaged
{
    private static int s_storageUid;

    private T* _data;
    private ushort* _meta;
    private nuint _capacity;
    private readonly NativeBuffer<PointerData> _freeList;
    private readonly Pending _pending;
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
            _meta = (ushort*)NativeMemory.Alloc(capacity, (nuint)sizeof(ushort));
            NativeMemory.Clear(_meta, capacity * (nuint)sizeof(ushort));
            _capacity = capacity;
        }

        _freeList = new NativeBuffer<PointerData>();
        _pending = new Pending();
        Id = unchecked((byte)Interlocked.Increment(ref s_storageUid));
    }

    public nuint Length { get; private set; }

    public byte Id { get; }

    internal Pending Pending => _pending;

    public ref T this[Pointer<T> pointer]
    {
        get
        {
            ThrowIfDisposed();
            Debug.Assert(pointer.Data.StorageId == Id);
            Debug.Assert(pointer.Data.Index < Length);
            return ref _data[pointer.Data.Index];
        }
    }

    public ref T this[ComponentHandle<T> handle]
    {
        get
        {
            ThrowIfDisposed();
            var data = handle.Data;
            Debug.Assert(data.StorageId == Id);
            Debug.Assert(data.Index < Length);
            return ref _data[data.Index];
        }
    }

    internal ushort Meta(nuint index) => _meta[index];

    public static Storage<T> From(ReadOnlySpan<T> values)
    {
        var storage = new Storage<T>((nuint)values.Length);
        for (var i = 0; i < values.Length; i++)
        {
            storage._data[i] = values[i];
            storage._meta[i] = 0;
        }

        storage.Length = (nuint)values.Length;
        storage._pending.Epoch.SetLength(storage.Length);
        return storage;
    }

    public Pointer<T> Create(T value)
    {
        ThrowIfDisposed();
        PointerData data;
        if (_freeList.TryPop(out data))
        {
            var i = data.Index;
            Debug.Assert(_meta[i] == 0);
            _data[i] = value;
            _meta[i] = 1;
        }
        else
        {
            var i = Length;
            EnsureCapacity(i + 1);
            _data[i] = value;
            _meta[i] = 1;
            Length++;
            data = PointerData.New(i, 0, Id);
        }

        return new Pointer<T>(data, _pending);
    }

    public void SyncPending()
    {
        ThrowIfDisposed();
        lock (_pending.Gate)
        {
            _pending.Epoch.SetLength(Length);

            for (nuint i = 0; i < _pending.AddRef.Length; i++)
            {
                _meta[_pending.AddRef[i]]++;
            }

            _pending.AddRef.Clear();

            for (nuint i = 0; i < _pending.SubRef.Length; i++)
            {
                var index = _pending.SubRef[i];
                _meta[index]--;
                if (_meta[index] == 0)
                {
                    _pending.Epoch[index]++;
                    _freeList.Push(PointerData.New(index, _pending.Epoch[index], Id));
                }
            }

            _pending.SubRef.Clear();
        }
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

    public Pointer<T> Pin(Item<T> item)
    {
        ThrowIfDisposed();
        lock (_pending.Gate)
        {
            _pending.AddRef.Push(item.Index);
            return new Pointer<T>(PointerData.New(item.Index, _pending.GetEpoch(item.Index), Id), _pending);
        }
    }

    public void Split(Pointer<T> pointer, out Slice<T> left, out Ref<T> item, out Slice<T> right)
    {
        ThrowIfDisposed();
        Debug.Assert(pointer.Data.StorageId == Id);
        SplitRaw(pointer.Data, out left, out var current, out right);
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
        _data = null;
        _meta = null;
        _capacity = 0;
        Length = 0;
        _freeList.Dispose();
        _pending.Dispose();
        _disposed = true;
    }

    private void EnsureCapacity(nuint capacity)
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

        _data = (T*)NativeMemory.Realloc(_data, next * (nuint)sizeof(T));
        _meta = (ushort*)NativeMemory.Realloc(_meta, next * (nuint)sizeof(ushort));
        NativeMemory.Clear(_meta + _capacity, (next - _capacity) * (nuint)sizeof(ushort));
        _capacity = next;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}

public unsafe readonly ref struct Item<T>
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
