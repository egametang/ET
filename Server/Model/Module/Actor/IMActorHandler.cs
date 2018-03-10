using System;
using System.Threading.Tasks;

namespace ETModel
{
	public interface IMActorHandler
	{
		Task Handle(Session session, Entity entity, ActorRequest message);
		Type GetMessageType();
	}
}