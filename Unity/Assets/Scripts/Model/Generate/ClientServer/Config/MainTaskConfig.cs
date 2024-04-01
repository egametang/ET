using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class MainTaskConfigCategory : Singleton<MainTaskConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, MainTaskConfig> dict = new();
		
        public void Merge(object o)
        {
            MainTaskConfigCategory s = o as MainTaskConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public MainTaskConfig Get(int id)
        {
            this.dict.TryGetValue(id, out MainTaskConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (MainTaskConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, MainTaskConfig> GetAll()
        {
            return this.dict;
        }

        public MainTaskConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

	public partial class MainTaskConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>主任务Id</summary>
		public int MainId { get; set; }
		/// <summary>子任务Id</summary>
		public int SubId { get; set; }
		/// <summary>任务类型</summary>
		public int Type { get; set; }
		/// <summary>任务名</summary>
		public string Name { get; set; }
		/// <summary>任务描述</summary>
		public string Desc { get; set; }
		/// <summary>是否自动提交</summary>
		public int AutoComplete { get; set; }

	}
}
