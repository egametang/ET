using System.Threading.Tasks;

namespace Model
{
	public interface IEntityActorHandler
	{
		Task Handle(Session session, Entity entity, ActorRequest message);
	}

	/// <summary>
	/// gate session �յ�����Ϣֱ��ת�����ͻ���
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
}