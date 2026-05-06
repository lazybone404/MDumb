using System.Diagnostics;

namespace Dumb.Engine.Graph;

public unsafe ref struct CursorItem<T>
    where T : unmanaged
{
    private readonly Pending _pending;
    private readonly PointerData _data;
    private T* _item;

    internal CursorItem(T* item, Pending pending, PointerData data)
    {
        _item = item;
        _pending = pending;
        _data = data;
    }

    public ref T Value => ref *_item;

    public Pointer<T> Pin()
    {
        ushort epoch;
        lock (_pending.Gate)
        {
            _pending.AddRef.Push(_data.Index);
            epoch = _pending.GetEpoch(_data.Index);
        }

        return new Pointer<T>(_data.WithEpoch(epoch), _pending);
    }
}

public unsafe ref struct Cursor<T>
    where T : unmanaged
{
    private Storage<T> _storage;
    private nuint _index;

    internal Cursor(Storage<T> storage, nuint index)
    {
        _storage = storage;
        _index = index;
    }

    public bool Next(out Slice<T> left, out CursorItem<T> item, out Slice<T> right)
    {
        while (true)
        {
            var id = _index;
            _index++;
            if (id >= _storage.Length)
            {
                _index = id;
                left = default;
                item = default;
                right = default;
                return false;
            }

            if (_storage.Meta(id) != 0)
            {
                Split(id, out left, out item, out right);
                return true;
            }
        }
    }

    public bool Prev(out Slice<T> left, out CursorItem<T> item, out Slice<T> right)
    {
        while (true)
        {
            if (_index == 0)
            {
                left = default;
                item = default;
                right = default;
                return false;
            }

            _index--;
            var id = _index;
            Debug.Assert(id < _storage.Length);
            if (_storage.Meta(id) != 0)
            {
                Split(id, out left, out item, out right);
                return true;
            }
        }
    }

    private void Split(nuint index, out Slice<T> left, out CursorItem<T> item, out Slice<T> right)
    {
        var data = PointerData.New(index, 0, _storage.Id);
        _storage.SplitRaw(data, out left, out var current, out right);
        item = new CursorItem<T>(current, _storage.Pending, data);
    }
}
