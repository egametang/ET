using System.Collections.Generic;
using Common.Base;
using MongoDB.Bson;

namespace Model
{
	public class ActorComponent : Component<World>
	{
		private readonly Dictionary<ObjectId, Actor> actors = new Dictionary<ObjectId, Actor>();

		public Actor Get(ObjectId id)
		{
			return this.actors[id];
		}

		public void Add(Actor actor)
		{
			this.actors[actor.Id] = actor;
		}

		public void Remove(ObjectId id)
		{
			Actor actor = this.Get(id);
			this.actors.Remove(id);
			actor.Dispose();
		}
	}
}