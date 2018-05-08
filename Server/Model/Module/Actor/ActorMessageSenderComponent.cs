using System.Collections.Generic;

namespace ETModel
{
	public class ActorMessageSenderComponent: Component
	{
		public readonly Dictionary<long, ActorMessageSender> ActorMessageSenders = new Dictionary<long, ActorMessageSender>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();
			foreach (ActorMessageSender actorMessageSender in this.ActorMessageSenders.Values)
			{
				actorMessageSender.Dispose();
			}
			this.ActorMessageSenders.Clear();
		}

		public ActorMessageSender Get(long id)
		{
			if (this.ActorMessageSenders.TryGetValue(id, out ActorMessageSender actorMessageSender))
			{
				return actorMessageSender;
			}
			
			actorMessageSender = ComponentFactory.CreateWithId<ActorMessageSender>(id);
			actorMessageSender.Parent = this;
			this.ActorMessageSenders[id] = actorMessageSender;
			return actorMessageSender;
		}
		
		public ActorMessageSender GetWithActorId(long actorId)
		{
			if (this.ActorMessageSenders.TryGetValue(actorId, out ActorMessageSender actorMessageSender))
			{
				return actorMessageSender;
			}
			
			actorMessageSender = ComponentFactory.CreateWithId<ActorMessageSender, long>(actorId, actorId);
			actorMessageSender.Parent = this;
			this.ActorMessageSenders[actorId] = actorMessageSender;
			return actorMessageSender;
		}

		public void Remove(long id)
		{
			ActorMessageSender actorMessageSender;
			if (!this.ActorMessageSenders.TryGetValue(id, out actorMessageSender))
			{
				return;
			}
			this.ActorMessageSenders.Remove(id);
			actorMessageSender.Dispose();
		}
	}
}
