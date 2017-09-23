using System;
using Model;

namespace Hotfix
{
	public class InnerMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, int offset, byte[] messageBytes, AMessage message)
		{
			// 收到actor rpc request
			if (message is ActorRpcRequest actorRpcRequest)
			{
				Entity entity = Game.Scene.GetComponent<ActorManagerComponent>().Get(actorRpcRequest.Id);
				if (entity == null)
				{
					Log.Warning($"not found actor: {actorRpcRequest.Id}");
					ActorRpcResponse response = new ActorRpcResponse
					{
						RpcId = actorRpcRequest.RpcId,
						Error = ErrorCode.ERR_NotFoundActor
					};
					session.Reply(response);
					return;
				}
				entity.GetComponent<ActorComponent>().Add(new ActorMessageInfo() { Session = session, Message = actorRpcRequest });
				return;
			}

			// 收到actor消息分发给actor自己去处理
			if (message is ActorRequest actorRequest)
			{
				Entity entity = Game.Scene.GetComponent<ActorManagerComponent>().Get(actorRequest.Id);
				if (entity == null)
				{
					Log.Warning($"not found actor: {actorRequest.Id}");
					ActorResponse response = new ActorResponse
					{
						RpcId = actorRequest.RpcId,
						Error = ErrorCode.ERR_NotFoundActor
					};
					session.Reply(response);
					return;
				}
				entity.GetComponent<ActorComponent>().Add(new ActorMessageInfo() { Session = session, Message = actorRequest });
				return;
			}

			if (message is AMessage || message is ARequest)
			{
				Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, message);
				return;
			}

			throw new Exception($"message type error: {message.GetType().FullName}");
		}
	}
}
