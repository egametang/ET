using System;

namespace ET
{
	public interface IMActorHandler
	{
		ETTask Handle(Session session, Entity entity, object actorMessage);
		Type GetMessageType();
	}
}