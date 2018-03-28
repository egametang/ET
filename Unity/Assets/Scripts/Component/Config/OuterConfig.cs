using System.Net;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonIgnoreExtraElements]
	public class OuterConfig: AConfigComponent
	{
		public string Host { get; set; }
		public int Port { get; set; }

		public string Host2 { get; set; }

		[BsonIgnore]
		private IPEndPoint ipEndPoint;

		[BsonIgnore]
		private IPEndPoint ipEndPoint2;

		public override void EndInit()
		{
			base.EndInit();

			if (this.Host2 == null)
			{
				this.Host2 = this.Host;
			}

			this.ipEndPoint = NetworkHelper.ToIPEndPoint(this.Host, this.Port);
			this.ipEndPoint2 = NetworkHelper.ToIPEndPoint(this.Host2, this.Port);
		}

		[BsonIgnore]
		public IPEndPoint IPEndPoint
		{
			get
			{
				return this.ipEndPoint;
			}
		}

		[BsonIgnore]
		public IPEndPoint IPEndPoint2
		{
			get
			{
				return this.ipEndPoint2;
			}
		}
	}
}