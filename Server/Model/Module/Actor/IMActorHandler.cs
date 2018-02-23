using System;
using System.Threading.Tasks;

namespace Model
{
	public interface IMActorHandler
	{
		Task Handle(Session session, Entity entity, uint rpcId, ActorRequest message);
		Type GetMessageType();
	}
}