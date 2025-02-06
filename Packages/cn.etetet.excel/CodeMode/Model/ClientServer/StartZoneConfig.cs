using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class StartZoneConfigCategory : Singleton<StartZoneConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, StartZoneConfig> dict = new();
		
        public void Merge(object o)
        {
            StartZoneConfigCategory s = o as StartZoneConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public StartZoneConfig Get(int id)
        {
            this.dict.TryGetValue(id, out StartZoneConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (StartZoneConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, StartZoneConfig> GetAll()
        {
            return this.dict;
        }

        public StartZoneConfig GetOne()
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

	public partial class StartZoneConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>ZoneType</summary>
		public int ZoneType { get; set; }
		/// <summary>数据库地址</summary>
		public string DBConnection { get; set; }
		/// <summary>数据库名</summary>
		public string DBName { get; set; }

	}
}
