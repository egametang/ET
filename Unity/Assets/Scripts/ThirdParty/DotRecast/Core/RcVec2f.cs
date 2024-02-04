using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core
{
    public struct RcVec2f
    {
        public float x;
        public float y;

        public static RcVec2f Zero { get; } = new RcVec2f { x = 0, y = 0 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Get(int idx)
        {
            if (0 == idx)
                return x;

            if (1 == idx)
                return y;

            throw new IndexOutOfRangeException("vector2f index out of range");
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RcVec2f))
                return false;

            return Equals((RcVec2f)obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RcVec2f other)
        {
            return x.Equals(other.x) &&
                   y.Equals(other.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            int hash = x.GetHashCode();
            hash = RcHashCodes.CombineHashCodes(hash, y.GetHashCode());
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RcVec2f left, RcVec2f right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RcVec2f left, RcVec2f right)
        {
            return !left.Equals(right);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return $"{x}, {y}";
        }
    }
}