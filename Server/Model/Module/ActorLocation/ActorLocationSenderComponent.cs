using System;
using System.Collections.Generic;

namespace ETModel
{
	public class ActorLocationSenderComponent: Component
	{
		public readonly Dictionary<long, ActorLocationSender> ActorLocationSenders = new Dictionary<long, ActorLocationSender>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();
			
			foreach (ActorLocationSender actorLocationSender in this.ActorLocationSenders.Values)
			{
				actorLocationSender.Dispose();
			}
			this.ActorLocationSenders.Clear();
		}

		public ActorLocationSender Get(long id)
		{
			if (id == 0)
			{
				throw new Exception($"actor id is 0");
			}
			if (this.ActorLocationSenders.TryGetValue(id, out ActorLocationSender actorLocationSender))
			{
				return actorLocationSender;
			}
			
			actorLocationSender = ComponentFactory.CreateWithId<ActorLocationSender>(id);
			actorLocationSender.Parent = this;
			this.ActorLocationSenders[id] = actorLocationSender;
			return actorLocationSender;
		}
		
		public void Remove(long id)
		{
			if (!this.ActorLocationSenders.TryGetValue(id, out ActorLocationSender actorMessageSender))
			{
				return;
			}
			this.ActorLocationSenders.Remove(id);
			actorMessageSender.Dispose();
		}
	}
}
