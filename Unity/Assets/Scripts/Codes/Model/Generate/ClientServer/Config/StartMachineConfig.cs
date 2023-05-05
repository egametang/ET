using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [Config]
    public partial class StartMachineConfigCategory : ConfigSingleton<StartMachineConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, StartMachineConfig> dict = new Dictionary<int, StartMachineConfig>();
		
        public void Merge(object o)
        {
            StartMachineConfigCategory s = o as StartMachineConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
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

	public partial class StartMachineConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>内网地址</summary>
		public string InnerIP { get; set; }
		/// <summary>外网地址</summary>
		public string OuterIP { get; set; }
		/// <summary>守护进程端口</summary>
		public string WatcherPort { get; set; }

	}
}
