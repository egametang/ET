using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class ActorComponentAwakeSystem : AwakeSystem<ActorComponent>
	{
		public override void Awake(ActorComponent self)
		{
			self.entityActorHandler = new CommonEntityActorHandler();
			self.queue = new Queue<ActorMessageInfo>();
			Game.Scene.GetComponent<ActorManagerComponent>().Add(self.Entity);

			self.HandleAsync();
		}
	}

	[ObjectSystem]
	public class ActorComponentAwake1System : AwakeSystem<ActorComponent, IEntityActorHandler>
	{
		public override void Awake(ActorComponent self, IEntityActorHandler iEntityActorHandler)
		{
			self.entityActorHandler = iEntityActorHandler;
			self.queue = new Queue<ActorMessageInfo>();
			Game.Scene.GetComponent<ActorManagerComponent>().Add(self.Entity);

			self.HandleAsync();
		}
	}

	[ObjectSystem]
	public class ActorComponentLoadSystem : LoadSystem<ActorComponent>
	{
		public override void Load(ActorComponent self)
		{
			self.entityActorHandler = (IEntityActorHandler)HotfixHelper.Create(self.entityActorHandler);
		}
	}

	[ObjectSystem]
	public class ActorComponentDestroySystem : DestroySystem<ActorComponent>
	{
		public override void Destroy(ActorComponent self)
		{
			Game.Scene.GetComponent<ActorManagerComponent>().Remove(self.Entity.Id);
		}
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor, 它会将Entity位置注册到Location Server, 接收的消息将会队列处理
	/// </summary>
	public static class ActorComponentEx
	{
		public static async Task AddLocation(this ActorComponent self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Add(self.Entity.Id);
		}

		public static async Task RemoveLocation(this ActorComponent self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Remove(self.Entity.Id);
		}

		public static void Add(this ActorComponent self, ActorMessageInfo info)
		{
			self.queue.Enqueue(info);

			if (self.tcs == null)
			{
				return;
			}

			var t = self.tcs;
			self.tcs = null;
			t.SetResult(self.queue.Dequeue());
		}

		private static Task<ActorMessageInfo> GetAsync(this ActorComponent self)
		{
			if (self.queue.Count > 0)
			{
				return Task.FromResult(self.queue.Dequeue());
			}

			self.tcs = new TaskCompletionSource<ActorMessageInfo>();
			return self.tcs.Task;
		}

		public static async void HandleAsync(this ActorComponent self)
		{
			while (true)
			{
				if (self.IsDisposed)
				{
					return;
				}
				try
				{
					ActorMessageInfo info = await self.GetAsync();
					// 返回null表示actor已经删除,协程要终止
					if (info.Message == null)
					{
						return;
					}
					await self.entityActorHandler.Handle(info.Session, (Entity)self.Parent, info.Message);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}