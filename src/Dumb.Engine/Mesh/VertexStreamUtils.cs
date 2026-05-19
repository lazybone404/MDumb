namespace Dumb.Engine.Mesh;

using System.Numerics;
using System.Runtime.CompilerServices;

internal static class VertexStreamUtils
{
    internal static Vector3 ReadFloat3(byte[] buf, int offset)
    {
        ref var f0 = ref Unsafe.As<byte, float>(ref buf[offset]);
        return new Vector3(f0, Unsafe.Add(ref f0, 1), Unsafe.Add(ref f0, 2));
    }

    internal static Vector4 ReadFloat4(byte[] buf, int offset)
    {
        ref var f0 = ref Unsafe.As<byte, float>(ref buf[offset]);
        return new Vector4(f0, Unsafe.Add(ref f0, 1), Unsafe.Add(ref f0, 2), Unsafe.Add(ref f0, 3));
    }

    internal static Vector2 ReadFloat2(byte[] buf, int offset)
    {
        ref var f0 = ref Unsafe.As<byte, float>(ref buf[offset]);
        return new Vector2(f0, Unsafe.Add(ref f0, 1));
    }

    internal static void WriteFloat3(byte[] buf, int offset, Vector3 v)
    {
        Unsafe.WriteUnaligned(ref buf[offset], v.X);
        Unsafe.WriteUnaligned(ref buf[offset + 4], v.Y);
        Unsafe.WriteUnaligned(ref buf[offset + 8], v.Z);
    }

    internal static void WriteFloat4(byte[] buf, int offset, Vector4 v)
    {
        Unsafe.WriteUnaligned(ref buf[offset], v.X);
        Unsafe.WriteUnaligned(ref buf[offset + 4], v.Y);
        Unsafe.WriteUnaligned(ref buf[offset + 8], v.Z);
        Unsafe.WriteUnaligned(ref buf[offset + 12], v.W);
    }

    internal static void WriteFloat2(byte[] buf, int offset, Vector2 v)
    {
        Unsafe.WriteUnaligned(ref buf[offset], v.X);
        Unsafe.WriteUnaligned(ref buf[offset + 4], v.Y);
    }

    internal static void WriteUshort4(byte[] buf, int offset, ushort i0, ushort i1, ushort i2, ushort i3)
    {
        Unsafe.WriteUnaligned(ref buf[offset], i0);
        Unsafe.WriteUnaligned(ref buf[offset + 2], i1);
        Unsafe.WriteUnaligned(ref buf[offset + 4], i2);
        Unsafe.WriteUnaligned(ref buf[offset + 6], i3);
    }
}
