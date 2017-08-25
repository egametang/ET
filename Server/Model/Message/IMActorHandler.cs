using System;
using System.Threading.Tasks;

namespace Model
{
	public interface IMActorHandler
	{
		Task<bool> Handle(Session session, Entity entity, object message);
		Type GetMessageType();
	}
}