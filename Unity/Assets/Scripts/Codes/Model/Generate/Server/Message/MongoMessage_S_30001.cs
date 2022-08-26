using ET;
using ProtoBuf;
using System.Collections.Generic;
namespace ET
{
	[Message(MongoMessage.ObjectQueryResponse)]
	[ProtoContract]
	public partial class ObjectQueryResponse: Object, IActorResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public Entity entity { get; set; }

	}

	[ResponseType(nameof(M2M_UnitTransferResponse))]
	[Message(MongoMessage.M2M_UnitTransferRequest)]
	[ProtoContract]
	public partial class M2M_UnitTransferRequest: Object, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public Unit Unit { get; set; }

		[ProtoMember(3)]
		public List<Entity> Entitys = new List<Entity>();

	}

	public static class MongoMessage
	{
		 public const ushort ObjectQueryResponse = 30002;
		 public const ushort M2M_UnitTransferRequest = 30003;
	}
}
