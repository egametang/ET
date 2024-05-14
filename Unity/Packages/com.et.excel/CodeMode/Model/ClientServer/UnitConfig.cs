using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class UnitConfigCategory : Singleton<UnitConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, UnitConfig> dict = new();
		
        public void Merge(object o)
        {
            UnitConfigCategory s = o as UnitConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
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
            
            var enumerator = this.dict.Values.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current; 
        }
    }

	public partial class UnitConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>Type</summary>
		public int Type { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>位置</summary>
		public int Position { get; set; }
		/// <summary>身高</summary>
		public int Height { get; set; }
		/// <summary>体重</summary>
		public int Weight { get; set; }

	}
}
