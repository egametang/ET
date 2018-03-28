using System.Threading.Tasks;

namespace ETModel
{
	public interface IEntityActorHandler
	{
		Task Handle(Session session, Entity entity, IActorMessage actorMessage);
	}
}