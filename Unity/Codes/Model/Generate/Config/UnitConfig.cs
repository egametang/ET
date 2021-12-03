using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
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
		
        public override void EndInit()
        {
            foreach (UnitConfig config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
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
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class UnitConfig: ProtoObject, IConfig
	{
		[ProtoMember(1)]
		public int Id { get; set; }
		[ProtoMember(3)]
		public string Name { get; set; }
		[ProtoMember(4)]
		public string Desc { get; set; }
		[ProtoMember(5)]
		public int Position { get; set; }
		[ProtoMember(6)]
		public int Height { get; set; }
		[ProtoMember(7)]
		public int Weight { get; set; }

	}
}
