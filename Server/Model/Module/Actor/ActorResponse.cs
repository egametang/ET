namespace ETModel
{
	/// <summary>
	/// actor RPC消息响应
	/// </summary>
	[Message(Opcode.ActorResponse)]
	public class ActorResponse : IActorLocationResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }
	}
}
