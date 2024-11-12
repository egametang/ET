using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 相加
    /// </summary>
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataComponentSystem
    {
        #region 相加得到一个新的结果

        public static NumericData Add(this NumericDataComponent self, NumericData target)
        {
            return self.NumericData.Add(target);
        }

        public static NumericData Add(this NumericDataComponent self, params NumericData[] allData)
        {
            return self.NumericData.Add(allData);
        }

        public static NumericData Add(this NumericDataComponent self, IEnumerable<NumericData> allData)
        {
            return self.NumericData.Add(allData);
        }

        public static NumericData Add(this NumericDataComponent self, NumericDataComponent target)
        {
            return self.NumericData.Add(target);
        }

        public static NumericData Add(this NumericDataComponent self, params NumericDataComponent[] allComponent)
        {
            return self.NumericData.Add(allComponent);
        }

        public static NumericData Add(this NumericDataComponent self, IEnumerable<NumericDataComponent> allComponent)
        {
            return self.NumericData.Add(allComponent);
        }

        #endregion

        #region 自身与目标相加后修改自己的数据 慎用

        public static void AddChange(this NumericDataComponent self, NumericData target, bool isPushEvent = false)
        {
            self.NumericData = self.NumericData.Add(target);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void AddChange(this NumericDataComponent self, bool isPushEvent = false, params NumericData[] allData)
        {
            self.NumericData = self.NumericData.Add(allData);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void AddChange(this NumericDataComponent self, IEnumerable<NumericData> allData, bool isPushEvent = false)
        {
            self.NumericData = self.NumericData.Add(allData);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void AddChange(this NumericDataComponent self, NumericDataComponent target, bool isPushEvent = false)
        {
            self.NumericData = self.NumericData.Add(target);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void AddChange(this NumericDataComponent self, bool isPushEvent = false, params NumericDataComponent[] allComponent)
        {
            self.NumericData = self.NumericData.Add(allComponent);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void AddChange(this NumericDataComponent self, IEnumerable<NumericDataComponent> allComponent, bool isPushEvent = false)
        {
            self.NumericData = self.NumericData.Add(allComponent);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        #endregion
    }
}
