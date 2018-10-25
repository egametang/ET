namespace ETModel
{
	public interface IMailboxHandler
	{
		ETTask Handle(Session session, Entity entity, object actorMessage);
	}
}