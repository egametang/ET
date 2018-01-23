﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace Hotfix
{
	[ObjectSystem]
	public class ActorComponentSystem : ObjectSystem<ActorComponent>, IAwake, IAwake<IEntityActorHandler>, ILoad
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Awake(IEntityActorHandler iEntityActorHandler)
		{
			this.Get().Awake(iEntityActorHandler);
		}

		public void Load()
		{
			this.Get().Load();
		}
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor, 它会将Entity位置注册到Location Server, 接收的消息将会队列处理
	/// </summary>
	public static class ActorComponentEx
	{
		public static void Awake(this ActorComponent self)
		{
			self.entityActorHandler = new CommonEntityActorHandler();
			self.queue = new Queue<ActorMessageInfo>();
			self.actorId = self.Parent.Id;
			Game.Scene.GetComponent<ActorManagerComponent>().Add(self.Parent);
			self.HandleAsync();
		}

		public static void Awake(this ActorComponent self, IEntityActorHandler iEntityActorHandler)
		{
			self.entityActorHandler = iEntityActorHandler;
			self.queue = new Queue<ActorMessageInfo>();
			self.actorId = self.Parent.Id;
			Game.Scene.GetComponent<ActorManagerComponent>().Add(self.Parent);
			self.HandleAsync();
		}

		// 热更新要重新创建接口,以便接口也能实现热更新
		public static void Load(this ActorComponent self)
		{
			self.entityActorHandler = (IEntityActorHandler) HotfixHelper.Create(self.entityActorHandler);
		}

		public static async Task AddLocation(this ActorComponent self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Add(self.actorId);
		}

		public static async Task RemoveLocation(this ActorComponent self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Remove(self.actorId);
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

		private static async void HandleAsync(this ActorComponent self)
		{
			while (true)
			{
				if (self.Id == 0)
				{
					return;
				}
				try
				{
					ActorMessageInfo info = await self.GetAsync();
					// 返回null表示actor已经删除,协程要终止
					if (info == null)
					{
						return;
					}
					await self.entityActorHandler.Handle(info.Session, self.Parent, info.Message);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}
	}
}