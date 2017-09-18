using System;
using System.Threading.Tasks;

namespace Model
{
	public interface IMActorHandler
	{
		Task Handle(Session session, Entity entity, ActorRequest message);
		Type GetMessageType();
	}
}