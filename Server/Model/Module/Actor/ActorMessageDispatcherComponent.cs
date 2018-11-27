using System;
using System.Collections.Generic;

namespace ETModel
{
	/// <summary>
	/// Actor消息分发组件
	/// </summary>
	public class ActorMessageDispatcherComponent : Component
	{
		public readonly Dictionary<Type, IMActorHandler> ActorMessageHandlers = new Dictionary<Type, IMActorHandler>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			this.ActorMessageHandlers.Clear();
		}
	}
}