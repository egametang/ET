using MongoDB.Bson.Serialization.Attributes;

namespace Hotfix
{
	[BsonIgnoreExtraElements]
	public class RunServerConfig: AConfigComponent
	{
		public string IP = "";
	}
}