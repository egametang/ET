using System;
using System.Threading.Tasks;

namespace ETModel
{
	public abstract class AMActorRpcHandler<E, Request, Response>: IMActorHandler where E: Entity where Request: class, IActorRequest where Response : class, IActorResponse
	{
		protected static void ReplyError(Response response, Exception e, Action<Response> reply)
		{
			Log.Error(e);
			response.Error = ErrorCode.ERR_RpcFail;
			response.Message = e.ToString();
			reply(response);
		}

		protected abstract ETTask Run(E unit, Request message, Action<Response> reply);

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
				
				await this.Run(e, request, response =>
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
				throw new Exception($"解释消息失败: {actorMessage.GetType().FullName}", e);
			}
		}

		public Type GetMessageType()
		{
			return typeof (Request);
		}
	}
}