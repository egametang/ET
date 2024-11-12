using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 创建
    /// 注意新创建的数据都是没有OwnerEntity的
    /// 需要自行赋值
    /// </summary>
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataExtend
    {
        #region 静态创建

        public static NumericData Create(params NumericData[] allData)
        {
            var newNumeric = NumericData.Create();
            newNumeric.Copy(allData);
            return newNumeric;
        }

        public static NumericData Create(IEnumerable<NumericData> allData)
        {
            var newNumeric = NumericData.Create();
            newNumeric.Copy(allData);
            return newNumeric;
        }

        public static NumericData Create(params NumericDataComponent[] allComponent)
        {
            var newNumeric = NumericData.Create();
            newNumeric.Copy(allComponent);
            return newNumeric;
        }

        public static NumericData Create(IEnumerable<NumericDataComponent> allComponent)
        {
            var newNumeric = NumericData.Create();
            newNumeric.Copy(allComponent);
            return newNumeric;
        }

        public static NumericData Create(Dictionary<ENumericType, long> configData)
        {
            return configData.CreateNumericData();
        }

        public static NumericData Create(Dictionary<int, long> configData)
        {
            return configData.CreateNumericData();
        }

        #endregion

        #region 扩展创建

        /// <summary>
        /// 从一个配置档复制数据
        /// </summary>
        public static NumericData CreateNumericData(this Dictionary<ENumericType, long> configData)
        {
            var newNumeric = NumericData.Create();
            newNumeric.InitSet(configData);
            return newNumeric;
        }

        /// <summary>
        /// 从一个配置档复制数据
        /// </summary>
        public static NumericData CreateNumericData(this Dictionary<int, long> configData)
        {
            var newNumeric = NumericData.Create();
            newNumeric.InitSet(configData);
            return newNumeric;
        }

        /// <summary>
        /// 以自身为源数据 拷贝返回一个新数据
        /// </summary>
        public static NumericData Create(this NumericData self)
        {
            var newNumeric = NumericData.Create();
            newNumeric.Copy(self);
            return newNumeric;
        }

        /// <summary>
        /// 以自身为源数据 拷贝返回一个新数据
        /// </summary>
        public static NumericData Create(this NumericDataComponent self)
        {
            var newNumeric = NumericData.Create();
            newNumeric.Copy(self);
            return newNumeric;
        }

        #endregion
    }
}
