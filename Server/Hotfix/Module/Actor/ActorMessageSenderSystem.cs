using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class ActorMessageSenderAwakeSystem : AwakeSystem<ActorMessageSender>
	{
		public override void Awake(ActorMessageSender self)
		{
			self.LastSendTime = TimeHelper.Now();
			self.tcs = null;
			self.FailTimes = 0;
			self.MaxFailTimes = 5;
			self.ActorId = 0;
		}
	}
	
	[ObjectSystem]
	public class ActorMessageSenderAwake2System : AwakeSystem<ActorMessageSender, long>
	{
		public override void Awake(ActorMessageSender self, long actorId)
		{
			self.LastSendTime = TimeHelper.Now();
			self.tcs = null;
			self.FailTimes = 0;
			self.MaxFailTimes = 0;
			self.ActorId = actorId;
		}
	}

	[ObjectSystem]
	public class ActorMessageSenderStartSystem : StartSystem<ActorMessageSender>
	{
		public override async void Start(ActorMessageSender self)
		{
			if (self.ActorId == 0)
			{
				self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
			}

			self.Address = Game.Scene.GetComponent<StartConfigComponent>()
					.Get(IdGenerater.GetAppIdFromId(self.ActorId))
					.GetComponent<InnerConfig>().IPEndPoint;

			self.UpdateAsync();
		}
	}
	
	[ObjectSystem]
	public class ActorMessageSenderDestroySystem : DestroySystem<ActorMessageSender>
	{
		public override void Destroy(ActorMessageSender self)
		{
			self.LastSendTime = 0;
			self.Address = null;

			while (self.WaitingTasks.Count > 0)
			{
				ActorTask actorTask = self.WaitingTasks.Dequeue();
				actorTask.RunFail(ErrorCode.ERR_NotFoundActor);
			}

			self.ActorId = 0;
			self.FailTimes = 0;
			var t = self.tcs;
			self.tcs = null;
			t?.SetResult(new ActorTask());
		}
	}

	public static class ActorMessageSenderEx
	{
		private static void Add(this ActorMessageSender self, ActorTask task)
		{
			if (self.IsDisposed)
			{
				throw new Exception("ActorProxy Disposed! dont hold actorproxy");
			}

			self.WaitingTasks.Enqueue(task);
			// failtimes > 0表示正在重试，这时候不能加到正在发送队列
			if (self.FailTimes == 0)
			{
				self.AllowGet();
			}
		}

		private static void AllowGet(this ActorMessageSender self)
		{
			if (self.tcs == null || self.WaitingTasks.Count <= 0)
			{
				return;
			}

			ActorTask task = self.WaitingTasks.Peek();

			var t = self.tcs;
			self.tcs = null;
			t.SetResult(task);
		}

		private static Task<ActorTask> GetAsync(this ActorMessageSender self)
		{
			if (self.WaitingTasks.Count > 0)
			{
				ActorTask task = self.WaitingTasks.Peek();
				return Task.FromResult(task);
			}

			self.tcs = new TaskCompletionSource<ActorTask>();
			return self.tcs.Task;
		}

		public static async void UpdateAsync(this ActorMessageSender self)
		{
			try
			{
				while (true)
				{
					ActorTask actorTask = await self.GetAsync();
					if (self.IsDisposed)
					{
						return;
					}

					await self.RunTask(actorTask);
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private static async Task RunTask(this ActorMessageSender self, ActorTask task)
		{
			IResponse response = await task.Run();

			// 如果没找到Actor,重试
			if (response.Error == ErrorCode.ERR_NotFoundActor)
			{
				++self.FailTimes;

				// 失败MaxFailTimes次则清空actor发送队列，返回失败
				if (self.FailTimes > self.MaxFailTimes)
				{
					// 失败直接删除actorproxy
					Log.Info($"actor send message fail, actorid: {self.Id}");
					self.GetParent<ActorMessageSenderComponent>().Remove(self.Id);
					return;
				}

				// 等待1s再发送
				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(1000);
				self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
				self.Address = Game.Scene.GetComponent<StartConfigComponent>()
						.Get(IdGenerater.GetAppIdFromId(self.ActorId))
						.GetComponent<InnerConfig>().IPEndPoint;
				self.AllowGet();
				return;
			}

			// 发送成功
			self.LastSendTime = TimeHelper.Now();
			self.FailTimes = 0;

			self.WaitingTasks.Dequeue();
		}

		public static void Send(this ActorMessageSender self, IActorMessage message)
		{
			ActorTask task = new ActorTask { message = message, MessageSender = self };
			self.Add(task);
		}

		public static Task<IResponse> Call(this ActorMessageSender self, IActorRequest request)
		{
			ActorTask task = new ActorTask { message = request, MessageSender = self, Tcs = new TaskCompletionSource<IResponse>() };
			self.Add(task);
			return task.Tcs.Task;
		}

		public static string DebugQueue(this ActorMessageSender self, Queue<ActorTask> tasks)
		{
			string s = "";
			foreach (ActorTask task in tasks)
			{
				s += $" {task.message.GetType().Name}";
			}

			return s;
		}
	}
}