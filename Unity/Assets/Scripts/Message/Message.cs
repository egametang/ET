using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[Message(1)]
	[BsonIgnoreExtraElements]
	public class C2R_Login: ARequest
	{
		public string Account;
		public string Password;
	}

	[Message(2)]
	[BsonIgnoreExtraElements]
	public class R2C_Login: AResponse
	{
		public string Address { get; set; }
		public long Key { get; set; }
	}

	[Message(3)]
	[BsonIgnoreExtraElements]
	public class R2C_ServerLog: AMessage
	{
		public string AppType { get; set; }
		public int AppId { get; set; }
		public LogType Type { get; set; }
		public string Log { get; set; }
	}

	[Message(4)]
	[BsonIgnoreExtraElements]
	public class C2R_SubscribeLog: ARequest
	{
	}

	[Message(5)]
	[BsonIgnoreExtraElements]
	public class R2C_SubscribeLog: AResponse
	{
	}

	[Message(6)]
	[BsonIgnoreExtraElements]
	public class R2G_GetLoginKey : ARequest
	{
	}

	[Message(7)]
	[BsonIgnoreExtraElements]
	public class G2R_GetLoginKey : AResponse
	{
		public long Key;

		public G2R_GetLoginKey(long key)
		{
			this.Key = key;
		}
	}

	[Message(8)]
	[BsonIgnoreExtraElements]
	public class C2G_LoginGate : ARequest
	{
		public long Key;

		public C2G_LoginGate(long key)
		{
			this.Key = key;
		}
	}

	[Message(9)]
	[BsonIgnoreExtraElements]
	public class G2C_LoginGate : AResponse
	{
	}
}
