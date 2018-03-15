using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ETModel
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

	public sealed class ActorProxy : Component
	{
		// actor的地址
		public IPEndPoint Address;

		// 已发送等待回应的消息
		public Queue<ActorTask> RunningTasks = new Queue<ActorTask>();

		// 还没发送的消息
		public Queue<ActorTask> WaitingTasks = new Queue<ActorTask>();

		// 发送窗口大小
		public int WindowSize = 1;

		// 最大窗口
		public const int MaxWindowSize = 1;

		// 最近发送消息的时间
		public long LastSendTime;

		private TaskCompletionSource<ActorTask> tcs;

		public CancellationTokenSource CancellationTokenSource;

		private int failTimes;
		
		public void Awake()
		{
			this.LastSendTime = TimeHelper.Now();
			this.WindowSize = 1;
			this.tcs = null;
			this.CancellationTokenSource = new CancellationTokenSource();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
			this.LastSendTime = 0;
			this.Address = null;
			this.RunningTasks.Clear();
			this.WaitingTasks.Clear();
			this.failTimes = 0;
			var t = this.tcs;
			this.tcs = null;
			t?.SetResult(new ActorTask());
		}
		
		private void Add(ActorTask task)
		{
			if (this.IsDisposed)
			{
				throw new Exception("ActorProxy Disposed! dont hold actorproxy");
			}
			this.WaitingTasks.Enqueue(task);
			// failtimes > 0表示正在重试，这时候不能加到正在发送队列
			if (this.failTimes == 0)
			{
				this.AllowGet();
			}
		}
		
		private void AllowGet()
		{
			if (this.tcs == null || this.WaitingTasks.Count <= 0 || this.RunningTasks.Count >= this.WindowSize)
			{
				return;
			}
			
			ActorTask task = this.WaitingTasks.Dequeue();
			this.RunningTasks.Enqueue(task);

			var t = this.tcs;
			this.tcs = null;
			t.SetResult(task);
		}

		private Task<ActorTask> GetAsync()
		{
			if (this.WaitingTasks.Count > 0)
			{
				ActorTask task = this.WaitingTasks.Dequeue();
				this.RunningTasks.Enqueue(task);
				return Task.FromResult(task);
			}
			
			this.tcs = new TaskCompletionSource<ActorTask>();
			return this.tcs.Task;
		}

		public async void UpdateAsync()
		{
			while (true)
			{
				ActorTask actorTask = await this.GetAsync();
				if (this.IsDisposed)
				{
					return;
				}
				try
				{
					this.RunTask(actorTask);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
					return;
				}
			}
		}

		private async void RunTask(ActorTask task)
		{
			try
			{
				IResponse response = await task.Run();

				// 如果没找到Actor,发送窗口减少为1,重试
				if (response.Error == ErrorCode.ERR_NotFoundActor)
				{
					this.CancellationTokenSource.Cancel();
					this.WindowSize = 1;
					++this.failTimes;

					while (this.WaitingTasks.Count > 0)
					{
						ActorTask actorTask = this.WaitingTasks.Dequeue();
						this.RunningTasks.Enqueue(actorTask);
					}
					ObjectHelper.Swap(ref this.RunningTasks, ref this.WaitingTasks);
					
					// 失败3次则清空actor发送队列，返回失败
					if (this.failTimes > 3)
					{
						while (this.WaitingTasks.Count > 0)
						{
							ActorTask actorTask = this.WaitingTasks.Dequeue();
							actorTask.RunFail(response.Error);
						}

						// 失败直接删除actorproxy
						Game.Scene.GetComponent<ActorProxyComponent>().Remove(this.Id);
						return;
					}
					// 等待一会再发送
					await Game.Scene.GetComponent<TimerComponent>().WaitAsync(this.failTimes * 500);
					int appId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(this.Id);
					this.Address = Game.Scene.GetComponent<StartConfigComponent>().Get(appId).GetComponent<InnerConfig>().IPEndPoint;
					this.CancellationTokenSource = new CancellationTokenSource();
					this.AllowGet();
					return;
				}

				// 发送成功
				this.LastSendTime = TimeHelper.Now();
				this.failTimes = 0;
				if (this.WindowSize < MaxWindowSize)
				{
					++this.WindowSize;
				}

				this.RunningTasks.Dequeue();
				this.AllowGet();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public void Send(IActorMessage message)
		{
			ActorTask task = new ActorTask
			{
				message = message,
				proxy = this
			};
			this.Add(task);
		}

		public Task<IResponse> Call(IActorRequest request)
		{
			ActorTask task = new ActorTask
			{
				message = request,
				proxy = this,
				Tcs = new TaskCompletionSource<IResponse>()
			};
			this.Add(task);
			return task.Tcs.Task;
		}

		public string DebugQueue(Queue<ActorTask> tasks)
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