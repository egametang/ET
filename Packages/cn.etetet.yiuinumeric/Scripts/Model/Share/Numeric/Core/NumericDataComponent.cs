using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 数值组件 现在可以挂载在任何entity下
    /// 一个entity只能挂载一个
    /// 如果需要快照数据时 可拷贝目标的NumericData 就可以打破只能挂载一个数据的局限性
    /// </summary>
    [ComponentOf]
    public class NumericDataComponent : Entity, IAwake, IDestroy, ITransfer, IDeserialize
    {
        //外部禁止访问使用此值 你应该只使用 NumericDataComponentSystem 中的扩展方法
        [BsonElement]
        public NumericData NumericData = NumericData.Create();

        public Dictionary<int, long> NumericDic => NumericData.NumericDic;
    }
}