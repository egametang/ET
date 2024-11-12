using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 清空自身 拷贝目标
    /// </summary>
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataExtend
    {
        /// <summary>
        /// 拷贝数据
        /// 会清空当前 然后与目标保持一致
        /// </summary>
        public static void Copy(this NumericData self, NumericData target)
        {
            self.NumericDic.Clear();
            foreach (var (k, v) in target.NumericDic)
            {
                self.NumericDic.Add(k, v);
            }
        }

        /// <summary>
        /// 拷贝数据
        /// 会清空当前 然后与目标保持一致
        /// </summary>
        public static void Copy(this NumericData self, NumericDataComponent target)
        {
            self.NumericDic.Clear();
            foreach (var (k, v) in target.NumericData.NumericDic)
            {
                self.NumericDic.Add(k, v);
            }
        }

        /// <summary>
        /// 拷贝 其他组件的总和
        /// 目标所有1-N的总和 最后计算0的结果
        /// </summary>
        public static void Copy(this NumericData self, IEnumerable<NumericData> allData)
        {
            using var tempDic = ObjectPool.Fetch<NumericDictionaryPool<int, long>>();
            foreach (var target in allData)
            {
                foreach (var (key, value) in target.NumericDic)
                {
                    if (key <= NumericConst.Max && !key.IsNotGrowNumeric())
                    {
                        continue;
                    }

                    if (!tempDic.TryAdd(key, value))
                    {
                        tempDic[key] += value;
                    }
                }
            }

            self.InitSet(tempDic);
        }

        /// <summary>
        /// 拷贝 其他组件的总和
        /// 目标所有1-N的总和 最后计算0的结果
        /// </summary>
        public static void Copy(this NumericData self, IEnumerable<NumericDataComponent> allComponent)
        {
            using var tempDic = ObjectPool.Fetch<NumericDictionaryPool<int, long>>();

            foreach (var component in allComponent)
            {
                foreach (var (key, value) in component.NumericData.NumericDic)
                {
                    if (key <= NumericConst.Max && !key.IsNotGrowNumeric())
                    {
                        continue;
                    }

                    if (!tempDic.TryAdd(key, value))
                    {
                        tempDic[key] += value;
                    }
                }
            }

            self.InitSet(tempDic);
        }
    }
}
