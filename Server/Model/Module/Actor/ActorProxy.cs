using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public abstract class ActorTask
	{
		[BsonIgnore]
		public ActorProxy proxy;

		[BsonElement]
		public MessageObject message;

		public abstract Task<IResponse> Run();

		public abstract void RunFail(int error);
	}

	/// <summary>
	/// 普通消息，不需要response
	/// </summary>
	public class ActorMessageTask: ActorTask
	{
		public ActorMessageTask(ActorProxy proxy, IMessage message)
		{
			this.proxy = proxy;
			this.message = (MessageObject)message;
		}

		public override async Task<IResponse> Run()
		{
			ActorRequest request = new ActorRequest() { Id = this.proxy.Id, AMessage = this.message };
			ActorResponse response = (ActorResponse)await this.proxy.RealCall(request, this.proxy.CancellationTokenSource.Token);
			return response;
		}

		public override void RunFail(int error)
		{
		}
	}

	/// <summary>
	/// Rpc消息，需要等待返回
	/// </summary>
	public class ActorRpcTask : ActorTask
	{
		[BsonIgnore]
		public readonly TaskCompletionSource<IResponse> Tcs = new TaskCompletionSource<IResponse>();

		public ActorRpcTask(ActorProxy proxy, IMessage message)
		{
			this.proxy = proxy;
			this.message = (MessageObject)message;
		}

		public override async Task<IResponse> Run()
		{
			ActorRequest request = new ActorRequest() { Id = this.proxy.Id, AMessage = this.message };
			ActorResponse response = (ActorResponse)await this.proxy.RealCall(request, this.proxy.CancellationTokenSource.Token);
			if (response.Error != ErrorCode.ERR_NotFoundActor)
			{
				this.Tcs.SetResult((IResponse)response.AMessage);
			}
			return response;
		}

		public override void RunFail(int error)
		{
			this.Tcs.SetException(new RpcException(error, ""));
		}
	}


	[ObjectSystem]
	public class ActorProxySystem : ObjectSystem<ActorProxy>, IAwake, IStart
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Start()
		{
			this.Get().Start();
		}
	}

	public sealed class ActorProxy : Disposer
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
			this.RunningTasks.Clear();
			this.WaitingTasks.Clear();
			this.WindowSize = 1;
			this.tcs = null;
			this.CancellationTokenSource = new CancellationTokenSource();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
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
			t?.SetResult(null);
		}

		public async void Start()
		{
			int appId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(this.Id);
			this.Address = Game.Scene.GetComponent<StartConfigComponent>().Get(appId).GetComponent<InnerConfig>().IPEndPoint;

			this.UpdateAsync();
		}

		private void Add(ActorTask task)
		{
			if (this.Id == 0)
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

		private void Remove()
		{
			this.RunningTasks.Dequeue();
			this.AllowGet();
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

		private async void UpdateAsync()
		{
			while (true)
			{
				ActorTask actorTask = await this.GetAsync();
				if (this.Id == 0)
				{
					return;
				}
				if (actorTask == null)
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
				this.Remove();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public void Send(IMessage message)
		{
			ActorMessageTask task = new ActorMessageTask(this, message);
			this.Add(task);
		}

		public Task<IResponse> Call(IRequest request)
		{
			ActorRpcTask task = new ActorRpcTask(this, (IMessage)request);
			this.Add(task);
			return task.Tcs.Task;
		}

		public async Task<IResponse> RealCall(ActorRequest request, CancellationToken cancellationToken)
		{
			try
			{
				//Log.Debug($"realcall {MongoHelper.ToJson(request)} {this.Address}");
				request.Id = this.Id;
				Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.Address);
				IResponse response = await session.Call(request, cancellationToken);
				return response;
			}
			catch (RpcException e)
			{
				Log.Error($"{this.Address} {e}");
				throw;
			}
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