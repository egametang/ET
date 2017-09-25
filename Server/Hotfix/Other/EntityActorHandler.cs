using System.Threading.Tasks;
using Model;

namespace Hotfix
{
    /// <summary>
    /// gate session收到的消息直接转发给客户端
    /// </summary>
    public class GateSessionEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, ActorRequest message)
        {
            ((Session)entity).Send(message.AMessage);
            ActorResponse response = new ActorResponse
            {
                RpcId = message.RpcId
            };
            session.Reply(response);
        }
    }

    public class CommonEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, ActorRequest message)
        {
            await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, message);
        }
    }

    /// <summary>
    /// 玩家收到帧同步消息交给帧同步组件处理
    /// </summary>
    public class MapUnitEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, ActorRequest message)
        {
            if (message.AMessage is AFrameMessage aFrameMessage)
            {
				// 客户端发送不需要设置Frame消息的id，在这里统一设置，防止客户端被破解发个假的id过来
	            aFrameMessage.Id = entity.Id;
				Game.Scene.GetComponent<ServerFrameComponent>().Add(aFrameMessage);
	            ActorResponse response = new ActorResponse
	            {
		            RpcId = message.RpcId
	            };
	            session.Reply(response);
				return;
            }
            await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, message);
        }
    }
}