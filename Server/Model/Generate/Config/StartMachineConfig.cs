using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class StartMachineConfigCategory : ProtoObject
    {
        public static StartMachineConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, StartMachineConfig> dict = new Dictionary<int, StartMachineConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<StartMachineConfig> list = new List<StartMachineConfig>();
		
        public StartMachineConfigCategory()
        {
            Instance = this;
        }
		
		[ProtoAfterDeserialization]
        public void AfterDeserialization()
        {
            foreach (StartMachineConfig config in list)
            {
                this.dict.Add(config.Id, config);
            }
            list.Clear();
            this.EndInit();
        }
		
        public StartMachineConfig Get(int id)
        {
            this.dict.TryGetValue(id, out StartMachineConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (StartMachineConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, StartMachineConfig> GetAll()
        {
            return this.dict;
        }

        public StartMachineConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class StartMachineConfig: ProtoObject, IConfig
	{
		[ProtoMember(1, IsRequired  = true)]
		public int Id { get; set; }
		[ProtoMember(2, IsRequired  = true)]
		public string InnerIP { get; set; }
		[ProtoMember(3, IsRequired  = true)]
		public string OuterIP { get; set; }
		[ProtoMember(4, IsRequired  = true)]
		public string WatcherPort { get; set; }


		[ProtoAfterDeserialization]
        public void AfterDeserialization()
        {
            this.EndInit();
        }
	}
}
