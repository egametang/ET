using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
	[Config]
	public partial class StartSceneConfigCategory : ACategory<StartSceneConfig>
	{
		public static StartSceneConfigCategory Instance;
		public StartSceneConfigCategory()
		{
			Instance = this;
		}
	}

	public partial class StartSceneConfig: IConfig
	{
		[BsonId]
		public long Id { get; set; }
		public int Process;
		public int Zone;
		public string SceneType;
		public string Name;
		public int OuterPort;
	}
}
