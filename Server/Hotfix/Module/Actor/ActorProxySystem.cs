using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class ActorProxyAwakeSystem : AwakeSystem<ActorProxy>
	{
		public override void Awake(ActorProxy self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class ActorProxyStartSystem : StartSystem<ActorProxy>
	{
		public override async void Start(ActorProxy self)
		{
			int appId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
			self.Address = Game.Scene.GetComponent<StartConfigComponent>().Get(appId).GetComponent<InnerConfig>().IPEndPoint;

			self.UpdateAsync();
		}
	}
	
	[ObjectSystem]
	public class ActorProxyDestroySystem : DestroySystem<ActorProxy>
	{
		public override void Destroy(ActorProxy self)
		{
			self.LastSendTime = 0;
			self.Address = null;

			while (self.WaitingTasks.Count > 0)
			{
				ActorTask actorTask = self.WaitingTasks.Dequeue();
				actorTask.RunFail(ErrorCode.ERR_NotFoundActor);
			}

			self.failTimes = 0;
			var t = self.tcs;
			self.tcs = null;
			t?.SetResult(new ActorTask());
		}
	}

	public static class ActorProxyEx
	{
		public static void Awake(this ActorProxy self)
		{
			self.LastSendTime = TimeHelper.Now();
			self.tcs = null;
			self.failTimes = 0;
			self.CancellationTokenSource = new CancellationTokenSource();
		}

		private static void Add(this ActorProxy self, ActorTask task)
		{
			if (self.IsDisposed)
			{
				throw new Exception("ActorProxy Disposed! dont hold actorproxy");
			}

			self.WaitingTasks.Enqueue(task);
			// failtimes > 0表示正在重试，这时候不能加到正在发送队列
			if (self.failTimes == 0)
			{
				self.AllowGet();
			}
		}

		private static void AllowGet(this ActorProxy self)
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

		private static Task<ActorTask> GetAsync(this ActorProxy self)
		{
			if (self.WaitingTasks.Count > 0)
			{
				ActorTask task = self.WaitingTasks.Peek();
				return Task.FromResult(task);
			}

			self.tcs = new TaskCompletionSource<ActorTask>();
			return self.tcs.Task;
		}

		public static async void UpdateAsync(this ActorProxy self)
		{
			while (true)
			{
				ActorTask actorTask = await self.GetAsync();
				if (self.IsDisposed)
				{
					return;
				}

				try
				{
					await self.RunTask(actorTask);
				}
				catch (Exception e)
				{
					Log.Error(e);
					return;
				}
			}
		}

		private static async Task RunTask(this ActorProxy self, ActorTask task)
		{
			try
			{
				IResponse response = await task.Run();

				// 如果没找到Actor,重试
				if (response.Error == ErrorCode.ERR_NotFoundActor)
				{
					self.CancellationTokenSource.Cancel();
					++self.failTimes;

					// 失败10次则清空actor发送队列，返回失败
					if (self.failTimes > 10)
					{
						// 失败直接删除actorproxy
						Log.Info($"actor send message fail, actorid: {self.Id}");
						Game.Scene.GetComponent<ActorProxyComponent>().Remove(self.Id);
						return;
					}

					// 等待1s再发送
					await Game.Scene.GetComponent<TimerComponent>().WaitAsync(1000);
					int appId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
					self.Address = Game.Scene.GetComponent<StartConfigComponent>().Get(appId).GetComponent<InnerConfig>().IPEndPoint;
					self.CancellationTokenSource = new CancellationTokenSource();
					self.AllowGet();
					return;
				}

				// 发送成功
				self.LastSendTime = TimeHelper.Now();
				self.failTimes = 0;

				self.WaitingTasks.Dequeue();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void Send(this ActorProxy self, IActorMessage message)
		{
			ActorTask task = new ActorTask { message = message, proxy = self };
			self.Add(task);
		}

		public static Task<IResponse> Call(this ActorProxy self, IActorRequest request)
		{
			ActorTask task = new ActorTask { message = request, proxy = self, Tcs = new TaskCompletionSource<IResponse>() };
			self.Add(task);
			return task.Tcs.Task;
		}

		public static string DebugQueue(this ActorProxy self, Queue<ActorTask> tasks)
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