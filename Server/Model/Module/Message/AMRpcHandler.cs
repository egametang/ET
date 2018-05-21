using System;

namespace ETModel
{
	public abstract class AMRpcHandler<Request, Response>: IMHandler where Request : class, IRequest where Response : class, IResponse 
	{
		protected static void ReplyError(Response response, Exception e, Action<Response> reply)
		{
			Log.Error(e);
			response.Error = ErrorCode.ERR_RpcFail;
			response.Message = e.ToString();
			reply(response);
		}

		protected abstract void Run(Session session, Request message, Action<Response> reply);

		public void Handle(Session session, object message)
		{
			try
			{
				Request request = message as Request;
				if (request == null)
				{
					Log.Error($"消息类型转换错误: {message.GetType().Name} to {typeof (Request).Name}");
				}

				int rpcId = request.RpcId;

				long instanceId = session.InstanceId;
				
				this.Run(session, request, response =>
				{
					// 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
					if (session.InstanceId != instanceId)
					{
						return;
					}

					response.RpcId = rpcId;
					session.Reply(response);
				});
			}
			catch (Exception e)
			{
				throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
			}
		}

		public Type GetMessageType()
		{
			return typeof (Request);
		}
	}
}