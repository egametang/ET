using ProtoBuf;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
namespace Model
{
	[Message(Opcode.Actor_Test)]
	[ProtoContract]
	public partial class Actor_Test : MessageObject, IActorMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public string Info;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.Actor_TestRequest)]
	[ProtoContract]
	public partial class Actor_TestRequest : MessageObject, IActorRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string request;
	}

	[Message(Opcode.Actor_TestResponse)]
	[ProtoContract]
	public partial class Actor_TestResponse : MessageObject, IActorResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public string response;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.Actor_TransferRequest)]
	[ProtoContract]
	public partial class Actor_TransferRequest : MessageObject, IActorRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public int MapIndex;
	}

	[Message(Opcode.Actor_TransferResponse)]
	[ProtoContract]
	public partial class Actor_TransferResponse : MessageObject, IActorResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.C2G_EnterMap)]
	[ProtoContract]
	public partial class C2G_EnterMap : MessageObject, IRequest
	{
	}

	[Message(Opcode.G2C_EnterMap)]
	[ProtoContract]
	public partial class G2C_EnterMap : MessageObject, IResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public long UnitId;

		[ProtoMember(2, IsRequired = true)]
		public int Count;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
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
	public partial class Actor_CreateUnits : MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		public List<UnitInfo> Units = new List<UnitInfo>();

	}
	
	[Message(Opcode.FrameMessage)]
	[ProtoContract]
	public partial class FrameMessage : MessageObject, IActorMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public int Frame;

		[ProtoMember(2)]
		public List<MessageObject> Messages = new List<MessageObject>();
	}

	[Message(Opcode.Frame_ClickMap)]
	[ProtoContract]
	public partial class Frame_ClickMap : MessageObject, IFrameMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public int X;

		[ProtoMember(2, IsRequired = true)]
		public int Z;

		[ProtoMember(90)]
		public long Id { get; set; }
	}

	[Message(Opcode.C2M_Reload)]
	[ProtoContract]
	public partial class C2M_Reload : MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public AppType AppType;

	}

	[Message(Opcode.M2C_Reload)]
	[ProtoContract]
	public partial class M2C_Reload : MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.C2R_Ping)]
	[ProtoContract]
	public partial class C2R_Ping : MessageObject, IRequest
	{
	}

	[Message(Opcode.R2C_Ping)]
	[ProtoContract]
	public partial class R2C_Ping : MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}
	
	[ProtoInclude(Opcode.Frame_ClickMap, typeof(Frame_ClickMap))]
	[BsonKnownTypes(typeof(Frame_ClickMap))]
	public partial class MessageObject
	{
	}
}