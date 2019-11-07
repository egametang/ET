using System;

namespace ETModel
{
	public abstract class AMActorRpcHandler<E, Request, Response>: IMActorHandler where E: Entity where Request: class, IActorRequest where Response : class, IActorResponse
	{
		protected abstract ETTask Run(E unit, Request request, Response response, Action reply);

		public async ETTask Handle(Session session, Entity entity, object actorMessage)
		{
			try
			{
				Request request = actorMessage as Request;
				if (request == null)
				{
					Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Request).Name}");
					return;
				}
				E e = entity as E;
				if (e == null)
				{
					Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof(E).Name}");
					return;
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
					await this.Run(e, request, response, Reply);
				}
				catch (Exception exception)
				{
					Log.Error(exception);
					response.Error = ErrorCode.ERR_RpcFail;
					response.Message = e.ToString();
					Reply();
				}
			}
			catch (Exception e)
			{
				Log.Error($"解释消息失败: {actorMessage.GetType().FullName}\n{e}");
			}
		}

		public Type GetMessageType()
		{
			return typeof (Request);
		}
	}
}