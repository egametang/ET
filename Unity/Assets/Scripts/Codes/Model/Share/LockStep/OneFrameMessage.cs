using System;
using System.Collections.Generic;

namespace ET
{
    public partial class OneFrameMessages
    {
        public OneFrameMessages()
        {
            this.InputInfos = new Dictionary<long, LSInputInfo>(LSConstValue.MatchCount);
        }

        public static bool operator==(OneFrameMessages a, OneFrameMessages b)
        {
            if (a.Frame != b.Frame)
            {
                return false;
            }

            for (int i = 0; i < LSConstValue.MatchCount; ++i)
            {
                if (a.InputInfos[i] != b.InputInfos[i])
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
    
    public partial class Room2C_BattleStart
    {
        public Room2C_BattleStart()
        {
            this.UnitInfo = new List<LockStepUnitInfo>(LSConstValue.MatchCount);
            for (int i = 0; i < LSConstValue.MatchCount; ++i)
            {
                this.UnitInfo.Add(null);
            }
        }
    }
}