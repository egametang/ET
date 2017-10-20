// 服务器与客户端之间的消息 Opcode从1-9999

using System.Collections.Generic;
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
	}


	[Message(Opcode.Actor_Test)]
	public class Actor_Test : AActorMessage
	{
		public string Info;
	}

	[Message(Opcode.Actor_TestRequest)]
	public class Actor_TestRequest : AActorRequest
	{
		public string request;
	}

	[Message(Opcode.Actor_TestResponse)]
	public class Actor_TestResponse : AActorResponse
	{
		public string response;
	}


	[Message(Opcode.Actor_TransferRequest)]
	public class Actor_TransferRequest : AActorRequest
	{
		public int MapIndex;
	}

	[Message(Opcode.Actor_TransferResponse)]
	public class Actor_TransferResponse : AActorResponse
	{
	}

	[Message(Opcode.C2G_EnterMap)]
	public class C2G_EnterMap: ARequest
	{
	}

	[Message(Opcode.G2C_EnterMap)]
	public class G2C_EnterMap: AResponse
	{
		public long UnitId;
		public int Count;
	}

	public class UnitInfo
	{
		public long UnitId;
		public int X;
		public int Z;
	}

	[Message(Opcode.Actor_CreateUnits)]
	public class Actor_CreateUnits : AActorMessage
	{
		public List<UnitInfo> Units = new List<UnitInfo>();
	}

	public struct FrameMessageInfo
	{
		public long Id;
		public AMessage Message;
	}

	// 服务端发给客户端,每帧一条
	[Message(Opcode.FrameMessage)]
	public class FrameMessage : AActorMessage
	{
		public int Frame;
		public List<AFrameMessage> Messages = new List<AFrameMessage>();
	}

	// 客户端点击地图
	[Message(Opcode.Frame_ClickMap)]
	public class Frame_ClickMap: AFrameMessage
	{
		public int X;
		public int Z;
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