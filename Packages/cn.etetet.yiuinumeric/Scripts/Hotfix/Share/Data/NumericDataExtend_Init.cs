#if UNITY_EDITOR
#define NUMERIC_CHECK_SYMBOLS //数值检查宏 离开Unity要使用就在unity设置中添加这个宏
#endif
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 初始化
    /// </summary>
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataExtend
    {
        #region Init

        //根据配置档直接传入初始化
        //所以他是需要重新计算结果的
        public static void InitSet(this NumericData self, Dictionary<ENumericType, long> configData, bool isPushEvent = false)
        {
            self.NumericDic.Clear();
            HashSet<int> existResult = new();
            var          index       = 0;
            var          maxCount    = configData.Count;
            foreach (var (key, numericValue) in configData)
            {
                index++;
                var numericType = (int)key;
                self.InitSet(ref existResult, numericType, numericValue, index >= maxCount, isPushEvent);
            }
        }

        //根据配置档直接传入初始化
        //所以他是需要重新计算结果的
        public static void InitSet(this NumericData self, Dictionary<int, long> configData, bool isPushEvent = false)
        {
            self.NumericDic.Clear();
            HashSet<int> existResult = new();
            var          index       = 0;
            var          maxCount    = configData.Count;
            foreach (var (key, numericValue) in configData)
            {
                index++;
                var numericType = key;
                self.InitSet(ref existResult, numericType, numericValue, index >= maxCount, isPushEvent);
            }
        }

        //同步服务器的数值
        //是计算了结果的
        public static void InitToServer(this NumericData self, Dictionary<int, long> serverData, bool isPushEvent = false)
        {
            self.NumericDic.Clear();
            foreach (var (key, numericValue) in serverData)
            {
                self.NumericDic[key] = numericValue;
            }

            if (isPushEvent)
            {
                self.PushEventAll();
            }
        }

        //数值初始化用 根据配置档直接赋值
        //因为不知道你的源数据结构 所以自行调用
        //最后的时候调用刷新 (防止每次都刷新最终值)
        private static void InitSet(this NumericData  self,
                                    ref  HashSet<int> existResult,
                                    int               numericType,
                                    long              value,
                                    bool              updateResult,
                                    bool              isPushEvent)
        {
            #if NUMERIC_CHECK_SYMBOLS
            if (!numericType.CheckChangeNumeric()) return;
            #endif

            if (!numericType.IsNotGrowNumeric())
            {
                existResult.Add(numericType / 10);
            }

            ChangeValue(self, numericType, value, isPushEvent);

            if (!updateResult) return;

            foreach (var key in existResult)
            {
                UpdateResult(self, key * 10 + 1, isPushEvent);
            }
        }

        #endregion
    }
}
