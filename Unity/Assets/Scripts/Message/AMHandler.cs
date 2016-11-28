using System;
using Base;

namespace Model
{
	public abstract class AMHandler<Message>: IMHandler where Message: AMessage
	{
		protected abstract void Run(Session session, Message message);
		
		public void Handle(Session session, MessageInfo messageInfo)
		{
			Message message;
			try
			{
				message = MongoHelper.FromBson<Message>(messageInfo.MessageBytes, messageInfo.Offset, messageInfo.Count);
			}
			catch (Exception e)
			{
				throw new Exception($"解释消息失败: {messageInfo.Opcode}", e);
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
				Request request = MongoHelper.FromBson<Request>(messageInfo.MessageBytes, messageInfo.Offset, messageInfo.Count);
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