using System;
using System.Collections.Generic;

namespace ET
{
    public partial class OneFrameInputs
    {
        protected bool Equals(OneFrameInputs other)
        {
            return Equals(this.Inputs, other.Inputs);
        }

        public void CopyTo(OneFrameInputs to)
        {
            to.Inputs.Clear();
            foreach (var kv in this.Inputs)
            {
                to.Inputs.Add(kv.Key, kv.Value);
            }
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

            return Equals((OneFrameInputs) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Inputs);
        }

        public static bool operator==(OneFrameInputs a, OneFrameInputs b)
        {
            if (a is null || b is null)
            {
                if (a is null && b is null)
                {
                    return true;
                }
                return false;
            }
            
            if (a.Inputs.Count != b.Inputs.Count)
            {
                return false;
            }

            foreach (var kv in a.Inputs)
            {
                if (!b.Inputs.TryGetValue(kv.Key, out LSInput inputInfo))
                {
                    return false;
                }

                if (kv.Value != inputInfo)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator !=(OneFrameInputs a, OneFrameInputs b)
        {
            return !(a == b);
        }
    }
}