using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(RedPointNode))]
    [FriendOf(typeof(RedPointNode))]
    public static partial class RedPointNodeSystem
    {
        [EntitySystem]
        private static void Awake(this RedPointNode self, string uiName)
        {
            self.Key = uiName;
            self.TypeRedPointDic = new Dictionary<string, int>();
        }

        public static void SetValueByType(this RedPointNode self, string typeName, int newValue)
        {
            if (self.TypeRedPointDic.TryGetValue(typeName,out var oldValue))
            {
                if (oldValue != newValue)
                {
                    self.TypeRedPointDic[typeName] = newValue;
                }
            }
        }
        
        public static int GetValueByType(this RedPointNode self, string typeName)
        {
            if (!self.TypeRedPointDic.TryGetValue(typeName,out var value))
            {
                Log.Error("异常红点" + self.Key +" "+ typeName);
                return 0;
            }
            return value;
        }

        [EntitySystem]
        private static void Destroy(this RedPointNode self)
        {
            
        }
    }
}