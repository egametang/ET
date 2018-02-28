using ProtoBuf;
using Model;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
namespace Model
{
	[Message(OuterOpcode.Actor_Test)]
	[ProtoContract]
	public partial class Actor_Test: MessageObject, IActorMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public string Info;

	}

	[Message(OuterOpcode.Actor_TestRequest)]
	[ProtoContract]
	public partial class Actor_TestRequest: MessageObject, IActorRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string request;

	}

	[Message(OuterOpcode.Actor_TestResponse)]
	[ProtoContract]
	public partial class Actor_TestResponse: MessageObject, IActorResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }
		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
		[ProtoMember(1, IsRequired = true)]
		public string response;

	}

	[Message(OuterOpcode.Actor_TransferRequest)]
	[ProtoContract]
	public partial class Actor_TransferRequest: MessageObject, IActorRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public int MapIndex;

	}

	[Message(OuterOpcode.Actor_TransferResponse)]
	[ProtoContract]
	public partial class Actor_TransferResponse: MessageObject, IActorResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }
		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(OuterOpcode.C2G_EnterMap)]
	[ProtoContract]
	public partial class C2G_EnterMap: IRequest
	{
	}

	[Message(OuterOpcode.G2C_EnterMap)]
	[ProtoContract]
	public partial class G2C_EnterMap: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }
		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
		[ProtoMember(1, IsRequired = true)]
		public long UnitId;

		[ProtoMember(2, IsRequired = true)]
		public int Count;

	}

	[Message(OuterOpcode.UnitInfo)]
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

	[Message(OuterOpcode.Actor_CreateUnits)]
	[ProtoContract]
	public partial class Actor_CreateUnits: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		public List<UnitInfo> Units = new List<UnitInfo>();

	}

	[Message(OuterOpcode.FrameMessageInfo)]
	[ProtoContract]
	public partial class FrameMessageInfo
	{
		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public MessageObject Message;

	}

	[Message(OuterOpcode.Frame_ClickMap)]
	[ProtoContract]
	public partial class Frame_ClickMap: MessageObject, IFrameMessage
	{
		[ProtoMember(92, IsRequired = true)]
		public long Id { get; set; }
		[ProtoMember(1, IsRequired = true)]
		public int X;

		[ProtoMember(2, IsRequired = true)]
		public int Z;

	}

	[Message(OuterOpcode.C2M_Reload)]
	[ProtoContract]
	public partial class C2M_Reload: IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public AppType AppType;

	}

	[Message(OuterOpcode.M2C_Reload)]
	[ProtoContract]
	public partial class M2C_Reload: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }
		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(OuterOpcode.C2R_Ping)]
	[ProtoContract]
	public partial class C2R_Ping: IRequest
	{
	}

	[Message(OuterOpcode.R2C_Ping)]
	[ProtoContract]
	public partial class R2C_Ping: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }
		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

}
#if SERVER
namespace Model
{
	[BsonKnownTypes(typeof(Actor_Test))]
	[BsonKnownTypes(typeof(Actor_TestRequest))]
	[BsonKnownTypes(typeof(Actor_TestResponse))]
	[BsonKnownTypes(typeof(Actor_TransferRequest))]
	[BsonKnownTypes(typeof(Actor_TransferResponse))]
	[BsonKnownTypes(typeof(Actor_CreateUnits))]
	[BsonKnownTypes(typeof(Frame_ClickMap))]
	public partial class MessageObject {}

}
#endif
namespace Model
{
	[ProtoInclude(OuterOpcode.Actor_Test, typeof(Actor_Test))]
	[ProtoInclude(OuterOpcode.Actor_TestRequest, typeof(Actor_TestRequest))]
	[ProtoInclude(OuterOpcode.Actor_TestResponse, typeof(Actor_TestResponse))]
	[ProtoInclude(OuterOpcode.Actor_TransferRequest, typeof(Actor_TransferRequest))]
	[ProtoInclude(OuterOpcode.Actor_TransferResponse, typeof(Actor_TransferResponse))]
	[ProtoInclude(OuterOpcode.Actor_CreateUnits, typeof(Actor_CreateUnits))]
	[ProtoInclude(OuterOpcode.Frame_ClickMap, typeof(Frame_ClickMap))]
	public partial class MessageObject {}

}
