using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public class ClientConfig: Component
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
