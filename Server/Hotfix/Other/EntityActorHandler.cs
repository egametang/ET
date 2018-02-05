using System;
using System.Threading.Tasks;
using Model;

namespace Hotfix
{
    /// <summary>
    /// gate session收到的消息直接转发给客户端
    /// </summary>
    public class GateSessionEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, uint rpcId, ActorRequest message)
        {
	        ActorResponse response = new ActorResponse();

			try
	        {
		        ((Session)entity).Send((IMessage)message.AMessage);
		        session.Reply(rpcId, response);
		        await Task.CompletedTask;
	        }
	        catch (Exception e)
	        {
		        response.Error = ErrorCode.ERR_SessionActorError;
		        response.Message = $"session actor error {e}";
				session.Reply(rpcId, response);
				throw;
	        }
        }
    }

    public class CommonEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, uint rpcId, ActorRequest message)
        {
            await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, rpcId, message);
        }
    }

    /// <summary>
    /// 玩家收到帧同步消息交给帧同步组件处理
    /// </summary>
    public class MapUnitEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, uint rpcId, ActorRequest message)
        {
            if (message.AMessage is IFrameMessage aFrameMessage)
            {
				// 客户端发送不需要设置Frame消息的id，在这里统一设置，防止客户端被破解发个假的id过来
	            aFrameMessage.Id = entity.Id;
				Game.Scene.GetComponent<ServerFrameComponent>().Add(aFrameMessage);
	            ActorResponse response = new ActorResponse();
	            session.Reply(rpcId, response);
				return;
            }
            await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, rpcId, message);
        }
    }
}