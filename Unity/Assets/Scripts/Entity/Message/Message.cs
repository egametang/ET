using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[Message(1)]
	[BsonIgnoreExtraElements]
	public class C2R_Login: ARequest
	{
		[BsonElement("A")]
		public string Account;
		[BsonElement("P")]
		public string Password;
	}

	[Message(2)]
	[BsonIgnoreExtraElements]
	public class R2C_Login: AResponse
	{
		[BsonElement("A")]
		public string Address { get; set; }
		[BsonElement("K")]
		public long Key { get; set; }
	}

	[Message(3)]
	[BsonIgnoreExtraElements]
	public class R2C_ServerLog: AMessage
	{
		[BsonElement("AT")]
		public AppType AppType { get; set; }
		[BsonElement("A")]
		public int AppId { get; set; }
		[BsonElement("T")]
		public LogType Type { get; set; }
		[BsonElement("L")]
		public string Log { get; set; }
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
		[BsonElement("K")]
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

	[Message(10)]
	[BsonIgnoreExtraElements]
	public class C2M_Reload : ARequest
	{
		public AppType AppType;
	}

	[Message(11)]
	[BsonIgnoreExtraElements]
	public class M2C_Reload : AResponse
	{
	}

	[Message(12)]
	[BsonIgnoreExtraElements]
	public class M2A_Reload : ARequest
	{
	}

	[Message(13)]
	[BsonIgnoreExtraElements]
	public class A2M_Reload : AResponse
	{
	}

	[Message(14)]
	[BsonIgnoreExtraElements]
	public class C2R_Ping : ARequest
	{
	}

	[Message(15)]
	[BsonIgnoreExtraElements]
	public class R2C_Ping : AResponse
	{
	}
}
