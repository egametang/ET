using System;

namespace Model
{
	public interface IMActorHandler
	{
		void Handle(Session session, Entity entity, object message);
		Type GetMessageType();
	}
}