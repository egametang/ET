using System.Threading.Tasks;

namespace Model
{
	public interface IEntityActorHandler
	{
		Task Handle(Session session, Entity entity, ActorRequest message);
	}
}