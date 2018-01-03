using ProtoBuf;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
namespace Model
{
	[Message(Opcode.C2R_Login)]
	[ProtoContract]
	public partial class C2R_Login:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string Account;

		[ProtoMember(2, IsRequired = true)]
		public string Password;

	}

	[Message(Opcode.R2C_Login)]
	[ProtoContract]
	public partial class R2C_Login:  AResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public string Address;

		[ProtoMember(2, IsRequired = true)]
		public long Key;

	}

	[Message(Opcode.C2G_LoginGate)]
	[ProtoContract]
	public partial class C2G_LoginGate:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(Opcode.G2C_LoginGate)]
	[ProtoContract]
	public partial class G2C_LoginGate:  AResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public long PlayerId;

	}

	[Message(Opcode.Actor_Test)]
	[ProtoContract]
	public partial class Actor_Test:  AActorMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public string Info;

	}

	[Message(Opcode.Actor_TestRequest)]
	[ProtoContract]
	public partial class Actor_TestRequest:  AActorRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string request;

	}

	[Message(Opcode.Actor_TestResponse)]
	[ProtoContract]
	public partial class Actor_TestResponse:  AActorResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public string response;

	}

	[Message(Opcode.Actor_TransferRequest)]
	[ProtoContract]
	public partial class Actor_TransferRequest:  AActorRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public int MapIndex;

	}

	[Message(Opcode.Actor_TransferResponse)]
	[ProtoContract]
	public partial class Actor_TransferResponse:  AActorResponse
	{
	}

	[Message(Opcode.C2G_EnterMap)]
	[ProtoContract]
	public partial class C2G_EnterMap:  ARequest
	{
	}

	[Message(Opcode.G2C_EnterMap)]
	[ProtoContract]
	public partial class G2C_EnterMap:  AResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public long UnitId;

		[ProtoMember(2, IsRequired = true)]
		public int Count;

	}

	[Message(Opcode.UnitInfo)]
	[ProtoContract]
	public partial class UnitInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public long UnitId;

		[ProtoMember(2, IsRequired = true)]
		public int X;

		[ProtoMember(3, IsRequired = true)]
		public int Z;

	}

	[Message(Opcode.Actor_CreateUnits)]
	[ProtoContract]
	public partial class Actor_CreateUnits:  AActorMessage
	{
		[ProtoMember(1)]
		public List<UnitInfo> Units = new List<UnitInfo>();

	}

	[Message(Opcode.FrameMessageInfo)]
	[ProtoContract]
	public partial class FrameMessageInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public AMessage Message;

	}

	[Message(Opcode.FrameMessage)]
	[ProtoContract]
	public partial class FrameMessage:  AActorMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public int Frame;

		[ProtoMember(2)]
		public List<AFrameMessage> Messages = new List<AFrameMessage>();

	}

	[Message(Opcode.Frame_ClickMap)]
	[ProtoContract]
	public partial class Frame_ClickMap:  AFrameMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public int X;

		[ProtoMember(2, IsRequired = true)]
		public int Z;

	}

	[Message(Opcode.C2M_Reload)]
	[ProtoContract]
	public partial class C2M_Reload:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public AppType AppType;

	}

	[Message(Opcode.M2C_Reload)]
	[ProtoContract]
	public partial class M2C_Reload:  AResponse
	{
	}

	[Message(Opcode.C2R_Ping)]
	[ProtoContract]
	public partial class C2R_Ping:  ARequest
	{
	}

	[Message(Opcode.R2C_Ping)]
	[ProtoContract]
	public partial class R2C_Ping:  AResponse
	{
	}

	[ProtoInclude((int)Opcode.Actor_Test, typeof(Actor_Test))]
	[ProtoInclude((int)Opcode.Actor_CreateUnits, typeof(Actor_CreateUnits))]
	[ProtoInclude((int)Opcode.FrameMessage, typeof(FrameMessage))]
	[BsonKnownTypes(typeof(Actor_Test))]
	[BsonKnownTypes(typeof(Actor_CreateUnits))]
	[BsonKnownTypes(typeof(FrameMessage))]
	public partial class  AActorMessage {}

	[ProtoInclude((int)Opcode.Actor_TestRequest, typeof(Actor_TestRequest))]
	[ProtoInclude((int)Opcode.Actor_TransferRequest, typeof(Actor_TransferRequest))]
	[BsonKnownTypes(typeof(Actor_TestRequest))]
	[BsonKnownTypes(typeof(Actor_TransferRequest))]
	public partial class  AActorRequest {}

	[ProtoInclude((int)Opcode.Actor_TestResponse, typeof(Actor_TestResponse))]
	[ProtoInclude((int)Opcode.Actor_TransferResponse, typeof(Actor_TransferResponse))]
	[BsonKnownTypes(typeof(Actor_TestResponse))]
	[BsonKnownTypes(typeof(Actor_TransferResponse))]
	public partial class  AActorResponse {}

	[ProtoInclude((int)Opcode.Frame_ClickMap, typeof(Frame_ClickMap))]
	[BsonKnownTypes(typeof(Frame_ClickMap))]
	public partial class  AFrameMessage {}

	[ProtoInclude((int)Opcode.C2R_Login, typeof(C2R_Login))]
	[ProtoInclude((int)Opcode.C2G_LoginGate, typeof(C2G_LoginGate))]
	[ProtoInclude((int)Opcode.C2G_EnterMap, typeof(C2G_EnterMap))]
	[ProtoInclude((int)Opcode.C2M_Reload, typeof(C2M_Reload))]
	[ProtoInclude((int)Opcode.C2R_Ping, typeof(C2R_Ping))]
	[BsonKnownTypes(typeof(C2R_Login))]
	[BsonKnownTypes(typeof(C2G_LoginGate))]
	[BsonKnownTypes(typeof(C2G_EnterMap))]
	[BsonKnownTypes(typeof(C2M_Reload))]
	[BsonKnownTypes(typeof(C2R_Ping))]
	public partial class  ARequest {}

	[ProtoInclude((int)Opcode.R2C_Login, typeof(R2C_Login))]
	[ProtoInclude((int)Opcode.G2C_LoginGate, typeof(G2C_LoginGate))]
	[ProtoInclude((int)Opcode.G2C_EnterMap, typeof(G2C_EnterMap))]
	[ProtoInclude((int)Opcode.M2C_Reload, typeof(M2C_Reload))]
	[ProtoInclude((int)Opcode.R2C_Ping, typeof(R2C_Ping))]
	[BsonKnownTypes(typeof(R2C_Login))]
	[BsonKnownTypes(typeof(G2C_LoginGate))]
	[BsonKnownTypes(typeof(G2C_EnterMap))]
	[BsonKnownTypes(typeof(M2C_Reload))]
	[BsonKnownTypes(typeof(R2C_Ping))]
	public partial class  AResponse {}

}
