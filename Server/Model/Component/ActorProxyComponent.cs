using System.Collections.Generic;

namespace Model
{
	public class ActorProxyComponent: Component
	{
		private readonly Dictionary<long, ActorProxy> dictionary = new Dictionary<long, ActorProxy>();

		public ActorProxy Get(long id)
		{
			if (this.dictionary.TryGetValue(id, out ActorProxy actorProxy))
			{
				return actorProxy;
			}
			
			actorProxy = EntityFactory.CreateWithId<ActorProxy>(id);
			this.dictionary[id] = actorProxy;
			return actorProxy;
		}
	}
}
