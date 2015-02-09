using Common.Event;
using Model;
using MongoDB.Bson;

namespace Controller
{
	[Action(ActionType.MessageAction)]
	public class MessageAction : IEventSync
	{
		public void Run(Env env)
		{
			Unit unit = World.Instance.GetComponent<UnitComponent>().Get(ObjectId.Empty);
			if (unit == null)
			{
				return;
			}
			unit.GetComponent<ActorComponent>().Add(env);
		}
	}
}