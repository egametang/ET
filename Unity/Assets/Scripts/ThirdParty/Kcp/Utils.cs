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

        // timediff was a macro in original Kcp. let's inline it if possible.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TimeDiff(uint later, uint earlier)
        {
            return (int)(later - earlier);
        }
    }
}
