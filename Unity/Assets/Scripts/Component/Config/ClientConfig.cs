using System.Net;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	public class ClientConfig: AConfigComponent
	{
		public string Host = "";
		public int Port;

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