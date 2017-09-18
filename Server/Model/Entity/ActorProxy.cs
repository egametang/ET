using System;
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
		public AMessage message;

		public abstract Task<AResponse> Run();

		public abstract void RunFail(int error);
	}

	/// <summary>
	/// 普通消息，不需要response
	/// </summary>
	public class ActorMessageTask: ActorTask
	{
		public ActorMessageTask(ActorProxy proxy, AMessage message)
		{
			this.proxy = proxy;
			this.message = message;
		}

		public override async Task<AResponse> Run()
		{
			ActorRequest request = new ActorRequest() { Id = this.proxy.Id, AMessage = this.message };
			ActorResponse response = await this.proxy.RealCall<ActorResponse>(request, this.proxy.CancellationTokenSource.Token);
			return response;
		}

		public override void RunFail(int error)
		{
		}
	}

	/// <summary>
	/// Rpc消息，需要等待返回
	/// </summary>
	/// <typeparam name="Response"></typeparam>
	public class ActorRpcTask<Response> : ActorTask where Response: AResponse
	{
		[BsonIgnore]
		public readonly TaskCompletionSource<Response> Tcs = new TaskCompletionSource<Response>();

		public ActorRpcTask(ActorProxy proxy, ARequest message)
		{
			this.proxy = proxy;
			this.message = message;
		}

		public override async Task<AResponse> Run()
		{
			ActorRpcRequest request = new ActorRpcRequest() { Id = this.proxy.Id, AMessage = this.message };
			ActorRpcResponse response = await this.proxy.RealCall<ActorRpcResponse>(request, this.proxy.CancellationTokenSource.Token);
			if (response.Error != ErrorCode.ERR_NotFoundActor)
			{
				this.Tcs.SetResult((Response)response.AMessage);
			}
			return response;
		}

		public override void RunFail(int error)
		{
			this.Tcs.SetException(new RpcException(error, ""));
		}
	}


	[ObjectEvent]
	public class ActorProxyEvent : ObjectEvent<ActorProxy>, IAwake, IStart
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

	public sealed class ActorProxy : Entity
	{
		// actor的地址
		public string Address;

		// 已发送等待回应的消息
		public EQueue<ActorTask> RunningTasks;

		// 还没发送的消息
		public EQueue<ActorTask> WaitingTasks;

		// 发送窗口大小
		public int WindowSize = 1;

		// 最大窗口
		public const int MaxWindowSize = 1;

		private TaskCompletionSource<ActorTask> tcs;

		public CancellationTokenSource CancellationTokenSource;

		private int failTimes;
		
		public void Awake()
		{
			this.RunningTasks = new EQueue<ActorTask>();
			this.WaitingTasks = new EQueue<ActorTask>();
			this.WindowSize = 1;
			this.CancellationTokenSource = new CancellationTokenSource();
		}
		
		public void Start()
		{
			this.UpdateAsync();
		}

		private void Add(ActorTask task)
		{
			this.WaitingTasks.Enqueue(task);
			this.AllowGet();
		}

		private void Remove()
		{
			ActorTask task = this.RunningTasks.Dequeue();
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
			if (this.Address == null)
			{
				int appId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(this.Id);
				this.Address = Game.Scene.GetComponent<StartConfigComponent>().Get(appId).GetComponent<InnerConfig>().Address;
			}
			while (true)
			{
				ActorTask actorTask = await this.GetAsync();
				this.RunTask(actorTask);
			}
		}

		private async void RunTask(ActorTask task)
		{
			try
			{
				AResponse response = await task.Run();

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
						return;
					}
					
					// 等待一会再发送
					await Game.Scene.GetComponent<TimerComponent>().WaitAsync(this.failTimes * 500);
					int appId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(this.Id);
					this.Address = Game.Scene.GetComponent<StartConfigComponent>().Get(appId).GetComponent<InnerConfig>().Address;
					this.CancellationTokenSource = new CancellationTokenSource();
					this.AllowGet();
					return;
				}

				// 发送成功
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

		public void Send(AMessage message)
		{
			ActorMessageTask task = new ActorMessageTask(this, message);
			this.Add(task);
		}

		public Task<Response> Call<Response>(ARequest request)where Response : AResponse
		{
			ActorRpcTask<Response> task = new ActorRpcTask<Response>(this, request);
			this.Add(task);
			return task.Tcs.Task;
		}

		public async Task<Response> RealCall<Response>(ActorRequest request, CancellationToken cancellationToken) where Response: AResponse
		{
			try
			{
				//Log.Debug($"realcall {MongoHelper.ToJson(request)} {this.Address}");
				request.Id = this.Id;
				Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.Address);
				Response response = await session.Call<Response>(request, cancellationToken);
				return response;
			}
			catch (RpcException e)
			{
				Log.Error($"{this.Address} {e}");
				throw;
			}
		}

		public string DebugQueue(EQueue<ActorTask> tasks)
		{
			string s = "";
			foreach (ActorTask task in tasks)
			{
				s += $" {task.message.GetType().Name}";
			}
			return s;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}