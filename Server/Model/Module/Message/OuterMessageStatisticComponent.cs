using System;
using System.Collections.Generic;

namespace ET
{
    public class OuterMessageStatisticComponent: Entity
    {
        public long LastCheckTime;
        public int MessageCountPerSec;
        public Dictionary<Type, int> MessageTypeCount = new Dictionary<Type, int>();
    }
}