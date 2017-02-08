using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public class OuterConfig: AConfigComponent
	{
		public string Host { get; set; }
		public int Port { get; set; }

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