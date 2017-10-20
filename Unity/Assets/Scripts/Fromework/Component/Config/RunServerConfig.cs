using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public class RunServerConfig: AConfigComponent
	{
		public string IP = "";
	}
}