using System;
using MemoryPack;

namespace ET
{
    [MemoryPackable]
    public partial struct LSInput
    {
        [MemoryPackOrder(0)]
        public TrueSync.TSVector2 V;

        [MemoryPackOrder(1)]
        public int Button;
        
        public bool Equals(LSInput other)
        {
            return this.V == other.V && this.Button == other.Button;
        }

        public override bool Equals(object obj)
        {
            return obj is LSInput other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.V, this.Button);
        }

        public static bool operator==(LSInput a, LSInput b)
        {
            if (a.V != b.V)
            {
                return false;
            }

            if (a.Button != b.Button)
            {
                return false;
            }

            return true;
        }

        public static bool operator !=(LSInput a, LSInput b)
        {
            return !(a == b);
        }
    }
}