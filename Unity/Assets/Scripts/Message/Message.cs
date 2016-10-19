using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[Message(1)]
	[BsonIgnoreExtraElements]
	public class C2S_Login: ARequest
	{
		public string Account;
		public string Password;
	}

	[Message(2)]
	[BsonIgnoreExtraElements]
	public class S2C_Login: AResponse
	{
		public string Host;
		public int Port;
	}

	[Message(3)]
	[BsonIgnoreExtraElements]
	public class S2C_ServerLog: AMessage
	{
		public string AppType;
		public LogType Type;
		public string Log;
	}

	[Message(4)]
	[BsonIgnoreExtraElements]
	public class C2S_SubscribeLog: ARequest
	{
	}

	[Message(5)]
	[BsonIgnoreExtraElements]
	public class S2C_SubscribeLog: AResponse
	{
	}
}
