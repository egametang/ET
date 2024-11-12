using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 数值组件数据基础 方便数据拷贝 不依赖Entity
    /// 亦可作为快照数据
    /// 对象池管理 请使用对象池维护: Create 与 Put
    /// </summary>
    [EnableClass]
    public class NumericData : IPool
    {
        //数值数据 无论如何你都不应该直接使用这个数据
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> NumericDic = new();

        private EntityRef<Entity> m_OwnerEntity;
        public  Entity            OwnerEntity => m_OwnerEntity;

        public static NumericData Create()
        {
            return ObjectPool.Fetch<NumericData>();
        }

        public void UpdateOwnerEntity(Entity ownerEntity)
        {
            m_OwnerEntity = ownerEntity;
        }

        public void Dispose()
        {
            m_OwnerEntity = default;
            NumericDic.Clear();
            ObjectPool.Recycle(this);
        }

        public bool IsFromPool { get; set; }
    }
}