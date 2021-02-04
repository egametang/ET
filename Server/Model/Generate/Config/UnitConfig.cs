using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [Config]
    public partial class UnitConfigCategory : ProtoObject
    {
        public static UnitConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, UnitConfig> dict = new Dictionary<int, UnitConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<UnitConfig> list = new List<UnitConfig>();
		
        public UnitConfigCategory()
        {
            Instance = this;
        }
		
        public override void AfterDeserialization()
        {
            foreach (UnitConfig config in list)
            {
                this.dict.Add(config.Id, config);
            }
            list.Clear();
            base.AfterDeserialization();
        }
		
        public UnitConfig Get(int id)
        {
            this.dict.TryGetValue(id, out UnitConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (UnitConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, UnitConfig> GetAll()
        {
            return this.dict;
        }

        public UnitConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.First();
        }
    }

    public partial class UnitConfig: IConfig
    {
        [ProtoMember(1, IsRequired  = true)]
        public int Id { get; set; }
        [ProtoMember(2, IsRequired  = true)]
        public string Name { get; set; }
        [ProtoMember(3, IsRequired  = true)]
        public string Desc { get; set; }
        [ProtoMember(4, IsRequired  = true)]
        public int Position { get; set; }
        [ProtoMember(5, IsRequired  = true)]
        public int Height { get; set; }
        [ProtoMember(6, IsRequired  = true)]
        public int Weight { get; set; }
    }
}