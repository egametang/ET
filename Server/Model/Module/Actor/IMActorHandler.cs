using System;

namespace ETModel
{
	public interface IMActorHandler
	{
		ETTask Handle(Session session, Entity entity, object actorMessage);
		Type GetMessageType();
	}
}