using System.Diagnostics;

namespace Dumb.Engine.Graph;

internal readonly record struct PointerData(ulong Value)
{
    private static readonly byte IndexBits = IntPtr.Size == 4 ? (byte)20 : (byte)40;
    private static readonly byte EpochBits = IntPtr.Size == 4 ? (byte)8 : (byte)16;
    private static readonly byte StorageIdBits = IntPtr.Size == 4 ? (byte)4 : (byte)8;

    private static readonly ulong IndexMask = (1UL << IndexBits) - 1;
    private static readonly byte EpochOffset = IndexBits;
    private static readonly ulong EpochMask = ((1UL << EpochBits) - 1) << EpochOffset;
    private static readonly byte StorageIdOffset = (byte)(EpochOffset + EpochBits);
    private static readonly ulong StorageIdMask = ((1UL << StorageIdBits) - 1) << StorageIdOffset;

    public static PointerData New(nuint index, ushort epoch, byte storageId)
    {
        Debug.Assert((index >> IndexBits) == 0);
        return new PointerData(
            (ulong)index
            + ((ulong)epoch << EpochOffset)
            + ((ulong)storageId << StorageIdOffset));
    }

    public nuint Index => (nuint)(Value & IndexMask);

    public ushort Epoch => (ushort)((Value & EpochMask) >> EpochOffset);

    public byte StorageId => (byte)((Value & StorageIdMask) >> StorageIdOffset);

    public PointerData WithEpoch(ushort epoch)
    {
        return new PointerData((Value & ~EpochMask) + ((ulong)epoch << EpochOffset));
    }
}
