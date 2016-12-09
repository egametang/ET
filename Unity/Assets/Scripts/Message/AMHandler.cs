using System;
using Base;

namespace Model
{
	public abstract class AMHandler<Message>: IMHandler where Message: AMessage
	{
		protected abstract void Run(Session session, Message message);
		
		public void Handle(Session session, MessageInfo messageInfo)
		{
			Message message = messageInfo.Message as Message;
			if (message == null)
			{
				Log.Error($"消息类型转换错误: {messageInfo.Message.GetType().Name} to {typeof(Message).Name}");
			}
			this.Run(session, message);
		}

		public Type GetMessageType()
		{
			return typeof (Message);
		}
	}

	public abstract class AMRpcHandler<Request, Response> : IMHandler
			where Request : ARequest
			where Response: AResponse
	{
		protected static void ReplyError(Response response, Exception e, Action<Response> reply)
		{
			Log.Error(e.ToString());
			response.Error = ErrorCode.ERR_RpcFail;
			response.Message = e.ToString();
			reply(response);
		}

		protected abstract void Run(Session session, Request message, Action<Response> reply);

		public void Handle(Session session, MessageInfo messageInfo)
		{
			try
			{
				Request request = messageInfo.Message as Request;
				if (request == null)
				{
					Log.Error($"消息类型转换错误: {messageInfo.Message.GetType().Name} to {typeof(Request).Name}");
				}
				this.Run(session, request, response =>
					{
						// 等回调回来,session可以已经断开了,所以需要判断session id是否为0
						if (session.Id == 0)
						{
							return;
						}
						session.Reply(messageInfo.RpcId, response);
					}
				);
			}
			catch (Exception e)
			{
				throw new Exception($"解释消息失败: {messageInfo.Opcode}", e);
			}
		}

		public Type GetMessageType()
		{
			return typeof(Request);
		}
	}
}