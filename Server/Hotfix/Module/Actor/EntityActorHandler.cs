using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// gate session收到的消息直接转发给客户端
    /// </summary>
    public class GateSessionEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, IActorMessage actorMessage)
        {
			ActorResponse actorResponse = new ActorResponse
			{
				RpcId = actorMessage.RpcId
			};
			try
	        {
		        // 发送给客户端
		        Session clientSession = entity as Session;
				clientSession.Send(actorMessage);

				session.Reply(actorResponse);
		        await Task.CompletedTask;
	        }
	        catch (Exception e)
	        {
		        actorResponse.Error = ErrorCode.ERR_SessionActorError;
		        actorResponse.Message = $"session actor error {e}";
				session.Reply(actorResponse);
				throw;
	        }
        }
    }

    public class CommonEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, IActorMessage actorMessage)
        {
			await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, actorMessage);
        }
    }

    /// <summary>
    /// 玩家收到帧同步消息交给帧同步组件处理
    /// </summary>
    public class MapUnitEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, IActorMessage actorMessage)
        {
			if (actorMessage is OneFrameMessage aFrameMessage)
            {
				Game.Scene.GetComponent<ServerFrameComponent>().Add(aFrameMessage);

				ActorResponse actorResponse = new ActorResponse
				{
					RpcId = actorMessage.RpcId
				};
				session.Reply(actorResponse);
				return;
            }
            await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, actorMessage);
        }
    }
}