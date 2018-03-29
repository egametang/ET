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
			self.ActorType = ActorType.Common;
			self.Queue = new Queue<ActorMessageInfo>();
			Game.Scene.GetComponent<ActorManagerComponent>().Add(self.Entity);

			self.HandleAsync();
		}
	}

	[ObjectSystem]
	public class ActorComponentAwake1System : AwakeSystem<ActorComponent, string>
	{
		public override void Awake(ActorComponent self, string actorType)
		{
			self.ActorType = actorType;
			self.Queue = new Queue<ActorMessageInfo>();
			Game.Scene.GetComponent<ActorManagerComponent>().Add(self.Entity);

			self.HandleAsync();
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
			self.Queue.Enqueue(info);

			if (self.Tcs == null)
			{
				return;
			}

			var t = self.Tcs;
			self.Tcs = null;
			t.SetResult(self.Queue.Dequeue());
		}

		private static Task<ActorMessageInfo> GetAsync(this ActorComponent self)
		{
			if (self.Queue.Count > 0)
			{
				return Task.FromResult(self.Queue.Dequeue());
			}

			self.Tcs = new TaskCompletionSource<ActorMessageInfo>();
			return self.Tcs.Task;
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

					// 根据这个actor的类型分发给相应的ActorHandler处理
					await Game.Scene.GetComponent<ActorMessageDispatherComponent>().ActorTypeHandle(
							self.ActorType, info.Session, (Entity)self.Parent, info.Message);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}