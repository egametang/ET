using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class MailBoxComponentAwakeSystem : AwakeSystem<MailBoxComponent>
	{
		public override void Awake(MailBoxComponent self)
		{
			self.ActorInterceptType = ActorInterceptType.None;
			self.Queue.Clear();
		}
	}

	[ObjectSystem]
	public class MailBoxComponentAwake1System : AwakeSystem<MailBoxComponent, string>
	{
		public override void Awake(MailBoxComponent self, string actorInterceptType)
		{
			self.ActorInterceptType = actorInterceptType;
			self.Queue.Clear();
		}
	}

	[ObjectSystem]
	public class MailBoxComponentStartSystem : StartSystem<MailBoxComponent>
	{
		public override void Start(MailBoxComponent self)
		{
			self.HandleAsync().NoAwait();
		}
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor, 接收的消息将会队列处理
	/// </summary>
	public static class MailBoxComponentHelper
	{
		public static async ETTask AddLocation(this MailBoxComponent self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Add(self.Entity.Id, self.Entity.InstanceId);
		}

		public static async ETTask RemoveLocation(this MailBoxComponent self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Remove(self.Entity.Id);
		}

		public static void Add(this MailBoxComponent self, ActorMessageInfo info)
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

		private static ETTask<ActorMessageInfo> GetAsync(this MailBoxComponent self)
		{
			if (self.Queue.Count > 0)
			{
				return ETTask.FromResult(self.Queue.Dequeue());
			}

			self.Tcs = new ETTaskCompletionSource<ActorMessageInfo>();
			return self.Tcs.Task;
		}

		public static async ETVoid HandleAsync(this MailBoxComponent self)
		{
			ActorMessageDispatherComponent actorMessageDispatherComponent = Game.Scene.GetComponent<ActorMessageDispatherComponent>();
			
			long instanceId = self.InstanceId;
			
			while (true)
			{
				if (self.InstanceId != instanceId)
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
					await actorMessageDispatherComponent.Handle(self, info);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}