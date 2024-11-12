using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataComponentSystem
    {
        #region Copy

        /// <summary>
        /// 拷贝数据
        /// 会清空当前 然后与目标保持一致
        /// </summary>
        public static void Copy(this NumericDataComponent self, NumericData target)
        {
            self.NumericData.Copy(target);
        }

        /// <summary>
        /// 拷贝数据
        /// 会清空当前 然后与目标保持一致
        /// </summary>
        public static void Copy(this NumericDataComponent self, NumericDataComponent target)
        {
            self.NumericData.Copy(target);
        }

        /// <summary>
        /// 拷贝 其他组件的总和
        /// 目标所有1-N的总和 最后计算0的结果
        /// </summary>
        public static void Copy(this NumericDataComponent self, IEnumerable<NumericData> allData)
        {
            self.NumericData.Copy(allData);
        }

        /// <summary>
        /// 拷贝 其他组件的总和
        /// 目标所有1-N的总和 最后计算0的结果
        /// </summary>
        public static void Copy(this NumericDataComponent self, IEnumerable<NumericDataComponent> allComponent)
        {
            self.NumericData.Copy(allComponent);
        }

        #endregion
    }
}
