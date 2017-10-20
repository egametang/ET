using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public class OuterConfig: AConfigComponent
	{
		public string Host { get; set; }
		public int Port { get; set; }

		public string Host2 { get; set; }

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