using ProtoBuf;
using Model;
using Hotfix;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
namespace Hotfix
{
	[Message(HotfixOpcode.C2R_Login)]
	[ProtoContract]
	public partial class C2R_Login: IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string Account;

		[ProtoMember(2, IsRequired = true)]
		public string Password;

	}

	[Message(HotfixOpcode.R2C_Login)]
	[ProtoContract]
	public partial class R2C_Login: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }
		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
		[ProtoMember(1, IsRequired = true)]
		public string Address;

		[ProtoMember(2, IsRequired = true)]
		public long Key;

	}

	[Message(HotfixOpcode.C2G_LoginGate)]
	[ProtoContract]
	public partial class C2G_LoginGate: IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(HotfixOpcode.G2C_LoginGate)]
	[ProtoContract]
	public partial class G2C_LoginGate: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }
		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
		[ProtoMember(1, IsRequired = true)]
		public long PlayerId;

	}

	[Message(HotfixOpcode.G2C_TestHotfixMessage)]
	[ProtoContract]
	public partial class G2C_TestHotfixMessage: IMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public string Info;

	}

	[Message(HotfixOpcode.C2M_TestActorRequest)]
	[ProtoContract]
	public partial class C2M_TestActorRequest: MessageObject, IActorRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string Info;

	}

	[Message(HotfixOpcode.M2C_TestActorResponse)]
	[ProtoContract]
	public partial class M2C_TestActorResponse: MessageObject, IActorResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }
		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
		[ProtoMember(1, IsRequired = true)]
		public string Info;

	}

}
#if SERVER
namespace Model
{
	[BsonKnownTypes(typeof(C2M_TestActorRequest))]
	[BsonKnownTypes(typeof(M2C_TestActorResponse))]
	public partial class MessageObject {}

}
#endif
namespace Model
{
	public partial class MessageObject {}

}
