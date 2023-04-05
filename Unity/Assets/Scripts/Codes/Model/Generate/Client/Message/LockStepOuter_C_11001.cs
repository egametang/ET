using ET;
using ProtoBuf;
using System.Collections.Generic;
namespace ET
{
	[ResponseType(nameof(G2C_Match))]
	[Message(LockStepOuter.C2G_Match)]
	[ProtoContract]
	public partial class C2G_Match: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(LockStepOuter.G2C_Match)]
	[ProtoContract]
	public partial class G2C_Match: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	public static class LockStepOuter
	{
		 public const ushort C2G_Match = 11002;
		 public const ushort G2C_Match = 11003;
	}
}
