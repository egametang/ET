using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ETModel
{
	/// <summary>
	/// actor RPC消息响应
	/// </summary>
	[Message(Opcode.ActorResponse)]
	[ProtoContract]
	public class ActorResponse : IActorResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }
	}
}
