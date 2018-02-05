using Model;
using ProtoBuf;

namespace Hotfix
{
	[Message(HotfixOpcode.C2R_Login)]
	[ProtoContract]
	public class C2R_Login : MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string Account;

		[ProtoMember(2, IsRequired = true)]
		public string Password;
	}

	[Message(HotfixOpcode.R2C_Login)]
	[ProtoContract]
	public class R2C_Login : MessageObject, IResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public string Address;

		[ProtoMember(2, IsRequired = true)]
		public long Key;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(HotfixOpcode.C2G_LoginGate)]
	[ProtoContract]
	public class C2G_LoginGate : MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(HotfixOpcode.G2C_LoginGate)]
	[ProtoContract]
	public class G2C_LoginGate : MessageObject, IResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public long PlayerId;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}
}