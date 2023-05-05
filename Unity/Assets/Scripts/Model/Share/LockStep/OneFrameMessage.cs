using System;
using System.Collections.Generic;

namespace ET
{
    public partial class OneFrameMessages
    {
        protected bool Equals(OneFrameMessages other)
        {
            return this.Frame == other.Frame && Equals(this.Inputs, other.Inputs);
        }

        public void CopyTo(OneFrameMessages to)
        {
            to.Frame = this.Frame;
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

            return Equals((OneFrameMessages) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Frame, this.Inputs);
        }

        public OneFrameMessages()
        {
            this.Inputs = new Dictionary<long, LSInput>(LSConstValue.MatchCount);
        }

        public static bool operator==(OneFrameMessages a, OneFrameMessages b)
        {
            if (a is null || b is null)
            {
                if (a is null && b is null)
                {
                    return true;
                }
                return false;
            }
            
            if (a.Frame != b.Frame)
            {
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

        public static bool operator !=(OneFrameMessages a, OneFrameMessages b)
        {
            return !(a == b);
        }
    }
    
    public partial class Room2C_Start
    {
        public Room2C_Start()
        {
            this.UnitInfo = new List<LockStepUnitInfo>(LSConstValue.MatchCount);
        }
    }
}