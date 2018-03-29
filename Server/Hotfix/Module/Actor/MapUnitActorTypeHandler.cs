using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	/// <summary>
	/// 玩家收到帧同步消息交给帧同步组件处理
	/// </summary>
	[ActorTypeHandler(AppType.Map, ActorType.Unit)]
	public class MapUnitActorTypeHandler : IActorTypeHandler
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