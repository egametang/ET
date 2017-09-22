using System.Threading.Tasks;

namespace Model
{
	public interface IEntityActorHandler
	{
		Task Handle(Session session, Entity entity, ActorRequest message);
	}

	/// <summary>
	/// gate session 收到的消息直接转发给客户端
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

	public class MapUnitEntityActorHandler : IEntityActorHandler
	{
		public async Task Handle(Session session, Entity entity, ActorRequest message)
		{
			if (message.AMessage is AFrameMessage aFrameMessage)
			{
				Game.Scene.GetComponent<ServerFrameComponent>().Add(aFrameMessage);
			}
			await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, message);
		}
	}
}