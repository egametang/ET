// 服务器与客户端之间的消息 Opcode从1-9999

using MongoDB.Bson.Serialization;

namespace Model
{
	[Message(Opcode.C2R_Login)]
	public class C2R_Login: ARequest
	{
		public string Account;
		public string Password;
	}

	[Message(Opcode.R2C_Login)]
	public class R2C_Login: AResponse
	{
		public string Address { get; set; }
		
		public long Key { get; set; }
	}

	[Message(Opcode.C2G_LoginGate)]
	public class C2G_LoginGate: ARequest
	{
		public long Key;

		public C2G_LoginGate(long key)
		{
			this.Key = key;
		}
	}

	[Message(Opcode.G2C_LoginGate)]
	public class G2C_LoginGate: AResponse
	{
		public long PlayerId;
		public long UnitId;
	}


	[Message(Opcode.Actor_Test)]
	public class Actor_Test : AActorMessage
	{
		public string Info;
	}

	[Message(Opcode.ActorRpc_TestRequest)]
	public class ActorRpc_TestRequest : AActorRequest
	{
		public string request;
	}

	[Message(Opcode.ActorRpc_TestResponse)]
	public class ActorRpc_TestResponse : AActorResponse
	{
		public string response;
	}
	
	[Message(Opcode.C2M_Reload)]
	public class C2M_Reload: ARequest
	{
		public AppType AppType;
	}

	[Message(11)]
	public class M2C_Reload: AResponse
	{
	}

	[Message(14)]
	public class C2R_Ping: ARequest
	{
	}

	[Message(15)]
	public class R2C_Ping: AResponse
	{
	}
}