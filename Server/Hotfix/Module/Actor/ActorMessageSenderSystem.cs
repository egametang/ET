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
			self.Tcs = null;
			self.FailTimes = 0;
			self.MaxFailTimes = 5;
			self.ActorId = 0;
			self.Error = 0;
		}
	}
	
	[ObjectSystem]
	public class ActorMessageSenderAwake2System : AwakeSystem<ActorMessageSender, long>
	{
		public override void Awake(ActorMessageSender self, long actorId)
		{
			self.LastSendTime = TimeHelper.Now();
			self.Tcs = null;
			self.FailTimes = 0;
			self.MaxFailTimes = 0;
			self.ActorId = actorId;
			self.Error = 0;
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
			while (self.WaitingTasks.Count > 0)
			{
				ActorTask actorTask = self.WaitingTasks.Dequeue();
				actorTask.Tcs?.SetException(new RpcException(self.Error, ""));
			}
			
			self.LastSendTime = 0;
			self.Address = null;
			self.ActorId = 0;
			self.FailTimes = 0;
			var t = self.Tcs;
			self.Tcs = null;
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
			if (self.Tcs == null || self.WaitingTasks.Count <= 0)
			{
				return;
			}

			ActorTask task = self.WaitingTasks.Peek();

			var t = self.Tcs;
			self.Tcs = null;
			t.SetResult(task);
		}

		private static Task<ActorTask> GetAsync(this ActorMessageSender self)
		{
			if (self.WaitingTasks.Count > 0)
			{
				ActorTask task = self.WaitingTasks.Peek();
				return Task.FromResult(task);
			}

			self.Tcs = new TaskCompletionSource<ActorTask>();
			return self.Tcs.Task;
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

					if (actorTask.ActorMessage == null)
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
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.Address);

			task.ActorMessage.ActorId = self.ActorId;
			IResponse response = await session.Call(task.ActorMessage);
			

			// 发送成功
			switch (response.Error)
			{
				case ErrorCode.ERR_NotFoundActor:
					// 如果没找到Actor,重试
					++self.FailTimes;

					// 失败MaxFailTimes次则清空actor发送队列，返回失败
					if (self.FailTimes > self.MaxFailTimes)
					{
						// 失败直接删除actorproxy
						Log.Info($"actor send message fail, actorid: {self.Id}");
						self.Error = response.Error;
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
				
				case ErrorCode.ERR_ActorNoMailBoxComponent:
					self.Error = response.Error;
					self.GetParent<ActorMessageSenderComponent>().Remove(self.Id);
					return;
				
				default:
					self.LastSendTime = TimeHelper.Now();
					self.FailTimes = 0;

					self.WaitingTasks.Dequeue();
					
					task.Tcs?.SetResult(response);
					
					return;
			}
		}

		public static void Send(this ActorMessageSender self, IActorMessage message)
		{
			ActorTask task = new ActorTask(message);
			self.Add(task);
		}

		public static Task<IResponse> Call(this ActorMessageSender self, IActorRequest request)
		{
			TaskCompletionSource<IResponse> tcs = new TaskCompletionSource<IResponse>();
			ActorTask task = new ActorTask(request, tcs);
			self.Add(task);
			return task.Tcs.Task;
		}

		public static string DebugQueue(this ActorMessageSender self, Queue<ActorTask> tasks)
		{
			string s = "";
			foreach (ActorTask task in tasks)
			{
				s += $" {task.ActorMessage.GetType().Name}";
			}

			return s;
		}
	}
}