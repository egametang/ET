using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
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