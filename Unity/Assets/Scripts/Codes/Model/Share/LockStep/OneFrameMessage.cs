using System;
using System.Collections.Generic;

namespace ET
{
    public partial class OneFrameMessages
    {
        protected bool Equals(OneFrameMessages other)
        {
            return this.Frame == other.Frame && Equals(this.InputInfos, other.InputInfos);
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
            return HashCode.Combine(this.Frame, this.InputInfos);
        }

        public OneFrameMessages()
        {
            this.InputInfos = new Dictionary<long, LSInputInfo>(LSConstValue.MatchCount);
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

            if (a.InputInfos.Count != b.InputInfos.Count)
            {
                return false;
            }

            foreach (var kv in a.InputInfos)
            {
                if (!b.InputInfos.TryGetValue(kv.Key, out LSInputInfo inputInfo))
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
    
    public partial class Battle2C_BattleStart
    {
        public Battle2C_BattleStart()
        {
            this.UnitInfo = new List<LockStepUnitInfo>(LSConstValue.MatchCount);
        }
    }
}