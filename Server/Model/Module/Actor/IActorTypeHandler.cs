using System.Threading.Tasks;

namespace ETModel
{
	public interface IActorTypeHandler
	{
		Task Handle(Session session, Entity entity, IActorMessage actorMessage);
	}
}