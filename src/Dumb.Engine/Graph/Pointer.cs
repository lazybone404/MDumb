namespace Dumb.Engine.Graph;

public sealed class DeadComponentException : Exception
{
    public DeadComponentException()
        : base("The component referenced by this weak pointer is dead.")
    {
    }
}

public sealed class Pointer<T> : IDisposable, IEquatable<Pointer<T>>
    where T : unmanaged
{
    private readonly Pending _pending;
    private bool _disposed;

    internal Pointer(PointerData data, Pending pending)
    {
        Data = data;
        _pending = pending;
    }

    internal PointerData Data { get; }

    internal Pending Pending => _pending;

    public ComponentHandle<T> Handle => new(Data.Value);

    public WeakPointer<T> Downgrade()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new WeakPointer<T>(Data, _pending);
    }

    public Pointer<T> Clone()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        lock (_pending.Gate)
        {
            _pending.AddRef.Push(Data.Index);
        }

        return new Pointer<T>(Data, _pending);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_pending.Gate)
        {
            _pending.SubRef.Push(Data.Index);
        }

        _disposed = true;
    }

    public bool Equals(Pointer<T>? other)
    {
        return other is not null && Data == other.Data;
    }

    public override bool Equals(object? obj)
    {
        return obj is Pointer<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }

    public override string ToString()
    {
        return $"Pointer {{ index = {Data.Index}, epoch = {Data.Epoch}, storage_id = {Data.StorageId} }}";
    }
}

public readonly struct WeakPointer<T> : IEquatable<WeakPointer<T>>
    where T : unmanaged
{
    private readonly PointerData _data;
    private readonly Pending? _pending;

    internal WeakPointer(PointerData data, Pending pending)
    {
        _data = data;
        _pending = pending;
    }

    public Pointer<T> Upgrade()
    {
        if (_pending is null)
        {
            throw new DeadComponentException();
        }

        lock (_pending.Gate)
        {
            if (_pending.GetEpoch(_data.Index) != _data.Epoch)
            {
                throw new DeadComponentException();
            }

            _pending.AddRef.Push(_data.Index);
        }

        return new Pointer<T>(_data, _pending);
    }

    public bool TryUpgrade(out Pointer<T>? pointer)
    {
        try
        {
            pointer = Upgrade();
            return true;
        }
        catch (DeadComponentException)
        {
            pointer = null;
            return false;
        }
    }

    public bool Equals(WeakPointer<T> other)
    {
        return _data == other._data && ReferenceEquals(_pending, other._pending);
    }

    public override bool Equals(object? obj)
    {
        return obj is WeakPointer<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_data, _pending);
    }
}
