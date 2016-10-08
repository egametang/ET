using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	[BsonIgnoreExtraElements]
	public class BuffProto : AConfig
	{
		public string Name { get; set; }
		public int Time { get; set; }
    }

	[Config]
	public class BuffCategory : ACategory<BuffProto>
	{
	}
}