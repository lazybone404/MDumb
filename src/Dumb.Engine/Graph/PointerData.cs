using System.Diagnostics;

namespace Dumb.Engine.Graph;

internal readonly record struct PointerData(ulong Value)
{
    private const byte IndexBits = 32;
    private const byte EpochBits = 16;
    private const byte StorageIdBits = 16;

    private static readonly ulong IndexMask = (1UL << IndexBits) - 1;
    private static readonly byte EpochOffset = IndexBits;
    private static readonly ulong EpochMask = ((1UL << EpochBits) - 1) << EpochOffset;
    private static readonly byte StorageIdOffset = (byte)(EpochOffset + EpochBits);
    private static readonly ulong StorageIdMask = ((1UL << StorageIdBits) - 1) << StorageIdOffset;

    public static PointerData New(nuint index, ushort epoch, ushort storageId)
    {
        Debug.Assert(index >> IndexBits == 0);
        return new PointerData(
            index
            + ((ulong)epoch << EpochOffset)
            + ((ulong)storageId << StorageIdOffset));
    }

    public nuint Index => (nuint)(Value & IndexMask);

    public ushort Epoch => (ushort)((Value & EpochMask) >> EpochOffset);

    public ushort StorageId => (ushort)((Value & StorageIdMask) >> StorageIdOffset);

    public PointerData WithEpoch(ushort epoch) => new((Value & ~EpochMask) + ((ulong)epoch << EpochOffset));
}
