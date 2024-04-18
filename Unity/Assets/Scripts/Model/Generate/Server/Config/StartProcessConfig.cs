using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class StartProcessConfigCategory : Singleton<StartProcessConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, StartProcessConfig> dict = new();
		
        public void Merge(object o)
        {
            StartProcessConfigCategory s = o as StartProcessConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
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
            
            var enumerator = this.dict.Values.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current; 
        }
    }

	public partial class StartProcessConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>所属机器</summary>
		public int MachineId { get; set; }
		/// <summary>外网端口</summary>
		public int Port { get; set; }

	}
}
