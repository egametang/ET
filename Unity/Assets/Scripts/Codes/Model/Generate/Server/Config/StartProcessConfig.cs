using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class StartProcessConfigCategory : ConfigSingleton<StartProcessConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, StartProcessConfig> dict = new Dictionary<int, StartProcessConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<StartProcessConfig> list = new List<StartProcessConfig>();
		
        public void Merge(object o)
        {
            StartProcessConfigCategory s = o as StartProcessConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (StartProcessConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public StartProcessConfig Get(int id)
        {
            this.dict.TryGetValue(id, out StartProcessConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (StartProcessConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, StartProcessConfig> GetAll()
        {
            return this.dict;
        }

        public StartProcessConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class StartProcessConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>所属机器</summary>
		[ProtoMember(2)]
		public int MachineId { get; set; }
		/// <summary>内网端口</summary>
		[ProtoMember(3)]
		public int InnerPort { get; set; }

	}
}
