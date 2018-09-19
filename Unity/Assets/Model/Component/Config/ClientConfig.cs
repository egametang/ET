using System.Net;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	public class ClientConfig: AConfigComponent
	{
		public string Address { get; set; }
	}
}