using MongoDB.Bson.Serialization.Attributes;

namespace Hotfix
{
	[BsonIgnoreExtraElements]
	public class ClientConfig: AConfigComponent
	{
		public string Host = "";
		public int Port;

		[BsonIgnore]
		public string Address
		{
			get
			{
				return $"{this.Host}:{this.Port}";
			}
		}
	}
}