using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 相减
    /// </summary>
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataComponentSystem
    {
        #region 相减 得到一个新的结果

        public static NumericData Subtract(this NumericDataComponent self, NumericData target)
        {
            return self.NumericData.Subtract(target);
        }

        public static NumericData Subtract(this NumericDataComponent self, params NumericData[] allData)
        {
            return self.NumericData.Subtract(allData);
        }

        public static NumericData Subtract(this NumericDataComponent self, IEnumerable<NumericData> allData)
        {
            return self.NumericData.Subtract(allData);
        }

        public static NumericData Subtract(this NumericDataComponent self, NumericDataComponent target)
        {
            return self.NumericData.Subtract(target);
        }

        public static NumericData Subtract(this NumericDataComponent self, params NumericDataComponent[] allData)
        {
            return self.NumericData.Subtract(allData);
        }

        public static NumericData Subtract(this NumericDataComponent self, IEnumerable<NumericDataComponent> allData)
        {
            return self.NumericData.Subtract(allData);
        }

        #endregion

        #region 自身与目标相减后修改自己的数据 慎用

        public static void SubtractChange(this NumericDataComponent self, NumericData target, bool isPushEvent = false)
        {
            self.NumericData = self.NumericData.Subtract(target);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void SubtractChange(this NumericDataComponent self, bool isPushEvent = false, params NumericData[] allData)
        {
            self.NumericData = self.NumericData.Subtract(allData);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void SubtractChange(this NumericDataComponent self, IEnumerable<NumericData> allData, bool isPushEvent = false)
        {
            self.NumericData = self.NumericData.Subtract(allData);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void SubtractChange(this NumericDataComponent self, NumericDataComponent target, bool isPushEvent = false)
        {
            self.NumericData = self.NumericData.Subtract(target);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void SubtractChange(this NumericDataComponent self, bool isPushEvent = false, params NumericDataComponent[] allData)
        {
            self.NumericData = self.NumericData.Subtract(allData);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        public static void SubtractChange(this NumericDataComponent self, IEnumerable<NumericDataComponent> allData, bool isPushEvent = false)
        {
            self.NumericData = self.NumericData.Subtract(allData);
            if (isPushEvent)
            {
                self.NumericData.PushEventAll();
            }
        }

        #endregion
    }
}
