using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public class ClientConfig
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
