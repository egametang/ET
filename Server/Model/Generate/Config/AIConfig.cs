using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class AIConfigCategory : ProtoObject, IMerge
    {
        public static AIConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, AIConfig> dict = new Dictionary<int, AIConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<AIConfig> list = new List<AIConfig>();
		
        public AIConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            AIConfigCategory s = o as AIConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (AIConfig config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public AIConfig Get(int id)
        {
            this.dict.TryGetValue(id, out AIConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (AIConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, AIConfig> GetAll()
        {
            return this.dict;
        }

        public AIConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class AIConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>所属ai</summary>
		[ProtoMember(2)]
		public int AIConfigId { get; set; }
		/// <summary>此ai中的顺序</summary>
		[ProtoMember(3)]
		public int Order { get; set; }
		/// <summary>节点名字</summary>
		[ProtoMember(4)]
		public string Name { get; set; }
		/// <summary>节点参数</summary>
		[ProtoMember(5)]
		public int[] NodeParams { get; set; }

	}
}
