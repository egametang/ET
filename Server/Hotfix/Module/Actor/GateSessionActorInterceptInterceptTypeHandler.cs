using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	/// <summary>
	/// gate session 拦截器，收到的actor消息直接转发给客户端
	/// </summary>
	[ActorInterceptTypeHandler(AppType.Gate, ActorInterceptType.GateSession)]
	public class GateSessionActorInterceptInterceptTypeHandler : IActorInterceptTypeHandler
	{
		public async ETTask Handle(Session session, Entity entity, object actorMessage)
		{
			try
			{
				IActorMessage iActorMessage = actorMessage as IActorMessage;
				// 发送给客户端
				Session clientSession = entity as Session;
				iActorMessage.ActorId = 0;
				clientSession.Send(iActorMessage);
				await ETTask.CompletedTask;
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
