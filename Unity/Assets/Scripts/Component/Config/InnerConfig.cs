using System.Net;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonIgnoreExtraElements]
	public class InnerConfig: AConfigComponent
	{
		public string Host { get; set; }
		public int Port { get; set; }

		[BsonIgnore]
		private IPEndPoint ipEndPoint;

		public override void EndInit()
		{
			base.EndInit();

			this.ipEndPoint = NetworkHelper.ToIPEndPoint(this.Host, this.Port);
		}

		[BsonIgnore]
		public IPEndPoint IPEndPoint
		{
			get
			{
				return this.ipEndPoint;
			}
		}
	}
}