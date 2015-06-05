using Model;
using MongoDB.Bson;

namespace Controller
{
	[Event(EventType.BuffTimeout, ServerType.City)]
	public class BuffTimeoutEvent: IEvent
	{
		public void Run(Env env)
		{
			Unit owner = World.Instance.GetComponent<UnitComponent>().Get(env.Get<ObjectId>(EnvKey.OwnerId));
			if (owner == null)
			{
				return;
			}
			owner.GetComponent<BuffComponent>().RemoveById(env.Get<ObjectId>(EnvKey.BuffId));
		}
	}
}