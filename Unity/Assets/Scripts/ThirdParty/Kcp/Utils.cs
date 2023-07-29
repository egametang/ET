using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ET
{
    public static partial class Utils
    {
        // Clamp so we don't have to depend on UnityEngine
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // // encode 8 bits unsigned int
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int Encode8u(byte[] p, int offset, byte value)
        // {
        //     p[0 + offset] = value;
        //     return 1;
        // }
        //
        // // decode 8 bits unsigned int
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int Decode8u(byte[] p, int offset, out byte value)
        // {
        //     value = p[0 + offset];
        //     return 1;
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int Decode8u(ReadOnlySpan<byte> data,int offset,out byte value)
        // {
        //     value = data[offset];
        //     return 1;
        // }
        //
        // // encode 16 bits unsigned int (lsb)
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int Encode16U(byte[] p, int offset, ushort value)
        // {
        //     p[0 + offset] = (byte)(value >> 0);
        //     p[1 + offset] = (byte)(value >> 8);
        //     return 2;
        // }
        //
        // // decode 16 bits unsigned int (lsb)
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int Decode16U(byte[] p, int offset, out ushort value)
        // {
        //     ushort result = 0;
        //     result |= p[0 + offset];
        //     result |= (ushort)(p[1 + offset] << 8);
        //     value = result;
        //     return 2;
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int Decode16U(ReadOnlySpan<byte> data, int offset, out ushort value)
        // {
        //     value = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(data.Slice(offset)));
        //     return 2;
        // }
        //
        // // encode 32 bits unsigned int (lsb)
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int Encode32U(byte[] p, int offset, uint value)
        // {
        //     p[0 + offset] = (byte)(value >> 0);
        //     p[1 + offset] = (byte)(value >> 8);
        //     p[2 + offset] = (byte)(value >> 16);
        //     p[3 + offset] = (byte)(value >> 24);
        //     return 4;
        // }
        //
        // // decode 32 bits unsigned int (lsb)
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int Decode32U(byte[] p, int offset, out uint value)
        // {
        //     uint result = 0;
        //     result |= p[0 + offset];
        //     result |= (uint)(p[1 + offset] << 8);
        //     result |= (uint)(p[2 + offset] << 16);
        //     result |= (uint)(p[3 + offset] << 24);
        //     value = result;
        //     return 4;
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int Decode32U(ReadOnlySpan<byte> data, int offset, out uint value)
        // {
        //     value = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(data.Slice(offset)));
        //     return 4;
        // }

        // timediff was a macro in original Kcp. let's inline it if possible.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TimeDiff(uint later, uint earlier)
        {
            return (int)(later - earlier);
        }
    }
}
