using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataComponentSystem
    {
        public static void InitSet(this NumericDataComponent self, Dictionary<ENumericType, long> configData, bool isPushEvent = false)
        {
            self.NumericData.InitSet(configData, isPushEvent);
        }

        public static void InitSet(this NumericDataComponent self, Dictionary<int, long> configData, bool isPushEvent = false)
        {
            self.NumericData.InitSet(configData, isPushEvent);
        }

        public static void InitToServer(this NumericDataComponent self, Dictionary<int, long> serverData, bool isPushEvent = false)
        {
            self.NumericData.InitToServer(serverData, isPushEvent);
        }
    }
}
