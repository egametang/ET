using System;

namespace ETModel
{
	public abstract class AMRpcHandler<Request, Response>: IMHandler where Request : class, IRequest where Response : class, IResponse 
	{
		protected abstract ETTask Run(Session session, Request request, Response response, Action reply);

		public async ETVoid Handle(Session session, object message)
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
				Response response = Activator.CreateInstance<Response>();

				void Reply()
				{
					// 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
					if (session.InstanceId != instanceId)
					{
						return;
					}

					response.RpcId = rpcId;
					session.Reply(response);
				}

				try
				{
					await this.Run(session, request, response, Reply);
				}
				catch (Exception e)
				{
					Log.Error(e);
					response.Error = ErrorCode.ERR_RpcFail;
					response.Message = e.ToString();
					Reply();
				}
				
			}
			catch (Exception e)
			{
				Log.Error($"解释消息失败: {message.GetType().FullName}\n{e}");
			}
		}

		public Type GetMessageType()
		{
			return typeof (Request);
		}
	}
}