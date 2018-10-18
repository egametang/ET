namespace ETModel
{
	public interface IActorInterceptTypeHandler
	{
		ETTask Handle(Session session, Entity entity, object actorMessage);
	}
}