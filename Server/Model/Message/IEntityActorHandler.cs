namespace Model
{
	public interface IEntityActorHandler
	{
		void Handle(Session session, Entity entity, IActorMessage message);
	}

	/// <summary>
	/// gate session 收到的消息直接转发给客户端
	/// </summary>
	public class GateSessionEntityActorHandler : IEntityActorHandler
	{
		public void Handle(Session session, Entity entity, IActorMessage message)
		{
			message.Id = 0;
			((Session)entity).Send((AMessage)message);
		}
	}

	public class CommonEntityActorHandler : IEntityActorHandler
	{
		public void Handle(Session session, Entity entity, IActorMessage message)
		{
			Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, message);
		}
	}
}