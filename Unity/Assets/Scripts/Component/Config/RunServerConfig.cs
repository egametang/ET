using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonIgnoreExtraElements]
	public class RunServerConfig: AConfigComponent
	{
		public string IP = "";
	}
}