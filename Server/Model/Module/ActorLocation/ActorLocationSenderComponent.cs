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
	}
}
