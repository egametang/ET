using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class StartSceneConfigCategory : ProtoObject
    {
        public static StartSceneConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, StartSceneConfig> dict = new Dictionary<int, StartSceneConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<StartSceneConfig> list = new List<StartSceneConfig>();
		
        public StartSceneConfigCategory()
        {
            Instance = this;
        }
		
		[ProtoAfterDeserialization]
        public void AfterDeserialization()
        {
            foreach (StartSceneConfig config in list)
            {
                this.dict.Add(config.Id, config);
            }
            list.Clear();
            this.EndInit();
        }
		
        public StartSceneConfig Get(int id)
        {
            this.dict.TryGetValue(id, out StartSceneConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (StartSceneConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, StartSceneConfig> GetAll()
        {
            return this.dict;
        }

        public StartSceneConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class StartSceneConfig: ProtoObject, IConfig
	{
		[ProtoMember(1, IsRequired  = true)]
		public int Id { get; set; }
		[ProtoMember(2, IsRequired  = true)]
		public int Process { get; set; }
		[ProtoMember(3, IsRequired  = true)]
		public int Zone { get; set; }
		[ProtoMember(4, IsRequired  = true)]
		public string SceneType { get; set; }
		[ProtoMember(5, IsRequired  = true)]
		public string Name { get; set; }
		[ProtoMember(6, IsRequired  = true)]
		public int OuterPort { get; set; }


		[ProtoAfterDeserialization]
        public void AfterDeserialization()
        {
            this.EndInit();
        }
	}
}
