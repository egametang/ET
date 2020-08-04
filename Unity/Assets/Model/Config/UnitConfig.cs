using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
	[Config]
	public partial class UnitConfigCategory : ACategory<UnitConfig>
	{
		public static UnitConfigCategory Instance;
		public UnitConfigCategory()
		{
			Instance = this;
		}
	}

	public partial class UnitConfig: IConfig
	{
		[BsonId]
		public long Id { get; set; }
		public string Name;
		public string Desc;
		public int Position;
		public int Height;
		public int Weight;
	}
}
