using System;
using Base;

namespace Model
{
	public abstract class AMActorHandler<E, Message>: IMActorHandler where E: Entity where Message : AActorMessage
	{
		protected abstract void Run(E entity, Message message);

		public void Handle(Session session, Entity entity, MessageInfo messageInfo)
		{
			Message message = messageInfo.Message as Message;
			if (message == null)
			{
				Log.Error($"消息类型转换错误: {messageInfo.Message.GetType().Name} to {typeof (Message).Name}");
			}
			E e = entity as E;
			if (e == null)
			{
				Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof(E).Name}");
			}
			this.Run(e, message);
		}

		public Type GetMessageType()
		{
			return typeof (Message);
		}
	}

	public abstract class AMActorRpcHandler<Request, Response>: IMActorHandler where Request : AActorRequest where Response : AActorResponse
	{
		protected static void ReplyError(Response response, Exception e, Action<Response> reply)
		{
			Log.Error(e.ToString());
			response.Error = ErrorCode.ERR_RpcFail;
			response.Message = e.ToString();
			reply(response);
		}

		protected abstract void Run(Entity entity, Request message, Action<Response> reply);

		public void Handle(Session session, Entity entity,  MessageInfo messageInfo)
		{
			try
			{
				Request request = messageInfo.Message as Request;
				if (request == null)
				{
					Log.Error($"消息类型转换错误: {messageInfo.Message.GetType().Name} to {typeof (Request).Name}");
				}
				this.Run(session, request, response =>
				{
					// 等回调回来,session可以已经断开了,所以需要判断session id是否为0
					if (session.Id == 0)
					{
						return;
					}
					session.Reply(messageInfo.RpcId, response);
				});
			}
			catch (Exception e)
			{
				throw new Exception($"解释消息失败: {messageInfo.Opcode}", e);
			}
		}

		public Type GetMessageType()
		{
			return typeof (Request);
		}
	}
}