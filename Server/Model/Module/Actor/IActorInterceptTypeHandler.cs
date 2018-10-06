using System.Threading.Tasks;

namespace ETModel
{
	public interface IActorInterceptTypeHandler
	{
		Task Handle(Session session, Entity entity, object actorMessage);
	}
}