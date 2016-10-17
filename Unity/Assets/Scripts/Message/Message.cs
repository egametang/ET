using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[Opcode(1)]
	[BsonIgnoreExtraElements]
	public class C2S_Login
	{
		public string Account;
		public string Password;
	}

	[Opcode(2)]
	[BsonIgnoreExtraElements]
	public class S2C_Login: IErrorMessage
	{
		public ErrorMessage ErrorMessage { get; set; }

		public string Host;
		public int Port;
	}

	[Opcode(3)]
	[BsonIgnoreExtraElements]
	public class S2C_ServerLog
	{
		public LogType Type;
		public string Log;
	}

	[Opcode(4)]
	[BsonIgnoreExtraElements]
	public class C2S_SubscribeLog
	{
	}

	[Opcode(5)]
	[BsonIgnoreExtraElements]
	public class S2C_SubscribeLog: IErrorMessage
	{
		public ErrorMessage ErrorMessage { get; set; }
	}
}
