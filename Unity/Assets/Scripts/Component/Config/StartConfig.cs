using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public class StartConfig: AConfig
	{
		public int AppId { get; set; }

		[BsonRepresentation(BsonType.String)]
		public AppType AppType { get; set; }

		public string ServerIP { get; set; }
	}
}