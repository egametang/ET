using System.Collections.Generic;

namespace Model
{
	[ObjectSystem]
	public class ActorProxyComponentSystem : ObjectSystem<ActorProxyComponent>, IStart
	{
		// 每10s扫描一次过期的actorproxy进行回收,过期时间是1分钟
		public async void Start()
		{
			ActorProxyComponent self = this.Get();

			List<long> timeoutActorProxyIds = new List<long>();

			while (true)
			{
				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(10000);

				if (self.Id == 0)
				{
					return;
				}

				timeoutActorProxyIds.Clear();

				long timeNow = TimeHelper.Now();
				foreach (long id in self.ActorProxys.Keys)
				{
					ActorProxy actorProxy = self.Get(id);
					if (actorProxy == null)
					{
						continue;
					}

					if (timeNow < actorProxy.LastSendTime + 60 * 1000)
					{
						continue;
					}
					
					timeoutActorProxyIds.Add(id);
				}

				foreach (long id in timeoutActorProxyIds)
				{
					self.Remove(id);
				}
			}
		}
	}

	public class ActorProxyComponent: Component
	{
		public readonly Dictionary<long, ActorProxy> ActorProxys = new Dictionary<long, ActorProxy>();

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();
			foreach (ActorProxy actorProxy in this.ActorProxys.Values)
			{
				actorProxy.Dispose();
			}
			this.ActorProxys.Clear();
		}

		public ActorProxy Get(long id)
		{
			if (this.ActorProxys.TryGetValue(id, out ActorProxy actorProxy))
			{
				return actorProxy;
			}
			
			actorProxy = EntityFactory.CreateWithId<ActorProxy>(id);
			this.ActorProxys[id] = actorProxy;
			return actorProxy;
		}

		public void Remove(long id)
		{
			ActorProxy actorProxy;
			if (!this.ActorProxys.TryGetValue(id, out actorProxy))
			{
				return;
			}
			this.ActorProxys.Remove(id);
			actorProxy.Dispose();
		}
	}
}
