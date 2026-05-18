using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace Dumb.Engine.Mesh;

public sealed class Indices
{
    private byte[] _data;
    private int _count;
    private int _stride = 2;

    public IndexFormat Format { get; private set; }
    public int Count => _count;
    public bool IsEmpty => _count == 0;

    public Indices()
    {
        _data = [];
        Format = IndexFormat.Uint16;
    }

    public Indices(IndexFormat format, int capacity = 0)
    {
        _stride = format == IndexFormat.Uint16 ? 2 : 4;
        _data = new byte[capacity * _stride];
        Format = format;
    }

    public void EnsureCapacity(int count)
    {
        var needed = count * _stride;
        if (_data.Length < needed)
            Array.Resize(ref _data, Math.Max(needed, _data.Length * 2));
    }

    public byte[] GetBytes()
    {
        if (_count == 0) return [];
        var result = new byte[_count * _stride];
        Array.Copy(_data, result, result.Length);
        return result;
    }

    public ReadOnlySpan<byte> GetSpan() => _data.AsSpan(0, _count * _stride);

    public void Clear()
    {
        _count = 0;
        Format = IndexFormat.Uint16;
        _stride = 2;
        _data = [];
    }

    public void SetAt(int index, uint value)
    {
        if ((uint)index >= (uint)_count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (_stride == 2)
            Unsafe.As<byte, ushort>(ref _data[index * 2]) = (ushort)value;
        else
            Unsafe.As<byte, uint>(ref _data[index * 4]) = value;
    }

    public uint this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (_stride == 2)
                return Unsafe.As<byte, ushort>(ref _data[index * 2]);
            return Unsafe.As<byte, uint>(ref _data[index * 4]);
        }
    }

    public void Push(uint index)
    {
        if (_stride == 2 && index > ushort.MaxValue)
            PromoteTo32();

        if (_count * _stride >= _data.Length)
            Array.Resize(ref _data, Math.Max(16, _data.Length * 2));

        if (_stride == 2)
            Unsafe.As<byte, ushort>(ref _data[_count * 2]) = (ushort)index;
        else
            Unsafe.As<byte, uint>(ref _data[_count * 4]) = index;

        _count++;
    }

    public void Extend(IEnumerable<uint> indices)
    {
        if (indices is IReadOnlyCollection<uint> coll && coll.Count == 0)
            return;

        // Fast path for arrays
        if (indices is uint[] arr)
        {
            Extend(arr.AsSpan());
            return;
        }

        // Fast path for lists
        if (indices is List<uint> list)
        {
            Extend(CollectionsMarshal.AsSpan(list));
            return;
        }

        // Fallback: buffer and delegate
        Extend(indices.ToArray().AsSpan());
    }

    public void Extend(ReadOnlySpan<uint> indices)
    {
        if (indices.IsEmpty) return;

        // Check promotion once
        if (_stride == 2)
        {
            foreach (var idx in indices)
            {
                if (idx > ushort.MaxValue)
                {
                    PromoteTo32();
                    break;
                }
            }
        }

        // Single capacity expansion
        var newCount = _count + indices.Length;
        var needed = newCount * _stride;
        if (_data.Length < needed)
            Array.Resize(ref _data, Math.Max(needed, _data.Length * 2));

        // Bulk write
        if (_stride == 2)
        {
            var dest = MemoryMarshal.Cast<byte, ushort>(_data.AsSpan()).Slice(_count);
            for (var i = 0; i < indices.Length; i++)
                dest[i] = (ushort)indices[i];
        }
        else
        {
            indices.CopyTo(MemoryMarshal.Cast<byte, uint>(_data.AsSpan()).Slice(_count));
        }

        _count = newCount;
    }

    private void PromoteTo32()
    {
        var oldData = _data;
        var oldCount = _count;
        _data = new byte[oldCount * 4];
        _stride = 4;
        Format = IndexFormat.Uint32;

        for (var i = 0; i < oldCount; i++)
        {
            var val = Unsafe.As<byte, ushort>(ref oldData[i * 2]);
            Unsafe.As<byte, uint>(ref _data[i * 4]) = val;
        }
    }
}
