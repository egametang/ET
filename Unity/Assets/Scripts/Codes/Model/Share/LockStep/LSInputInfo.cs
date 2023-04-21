using System;
using TrueSync;

namespace ET
{
    public partial class LSInputInfo
    {
        protected bool Equals(LSInputInfo other)
        {
            return this.V.Equals(other.V) && this.Button == other.Button;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((LSInputInfo) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.V, this.Button);
        }

        public static bool operator==(LSInputInfo a, LSInputInfo b)
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

        public static bool operator !=(LSInputInfo a, LSInputInfo b)
        {
            return !(a == b);
        }
    }
}