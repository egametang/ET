// ��������ͻ���֮�����Ϣ Opcode��1-9999

using System.Collections.Generic;
using ProtoBuf;

namespace Model
{
	[ProtoContract]
	[Message(Opcode.C2R_Login)]
	public class C2R_Login: ARequest
	{
		[ProtoMember(1)]
		public string Account;

		[ProtoMember(2)]
		public string Password;
	}

	[ProtoContract]
	[Message(Opcode.R2C_Login)]
	public class R2C_Login: AResponse
	{
		[ProtoMember(1)]
		public string Address { get; set; }

		[ProtoMember(2)]
		public long Key { get; set; }
	}

	[ProtoContract]
	[Message(Opcode.C2G_LoginGate)]
	public class C2G_LoginGate: ARequest
	{
		[ProtoMember(1)]
		public long Key;
	}

	[ProtoContract]
	[Message(Opcode.G2C_LoginGate)]
	public class G2C_LoginGate: AResponse
	{
		[ProtoMember(1)]
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

	[ProtoContract]
	[Message(Opcode.C2G_EnterMap)]
	public class C2G_EnterMap: ARequest
	{
	}

	[ProtoContract]
	[Message(Opcode.G2C_EnterMap)]
	public class G2C_EnterMap: AResponse
	{
		[ProtoMember(1)]
		public long UnitId;
		[ProtoMember(2)]
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

	// ����˷����ͻ���,ÿ֡һ��
	[Message(Opcode.FrameMessage)]
	public class FrameMessage : AActorMessage
	{
		public int Frame;
		public List<AFrameMessage> Messages = new List<AFrameMessage>();
	}

	// �ͻ��˵����ͼ
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