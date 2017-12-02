using System.Net;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public class InnerConfig: AConfigComponent
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

		[BsonIgnore]
		public IPEndPoint IPEndPoint
		{
			get
			{
				return NetworkHelper.ToIPEndPoint(this.Host, this.Port);
			}
		}
	}
}