using System;
using System.Threading.Tasks;

namespace Model
{
	public struct ActorMessageInfo
	{
		public Session Session;
		public ActorRequest Message;
	}

	[ObjectEvent]
	public class ActorComponentEvent : ObjectEvent<ActorComponent>, IAwake, IAwake<IEntityActorHandler>
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Awake(IEntityActorHandler iEntityActorHandler)
		{
			this.Get().Awake(iEntityActorHandler);
		}
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor, 它会将Entity位置注册到Location Server, 接收的消息将会队列处理
	/// </summary>
	public class ActorComponent: Component
	{
		private IEntityActorHandler entityActorHandler;

		private long actorId;

		// 队列处理消息
		private readonly EQueue<ActorMessageInfo> queue = new EQueue<ActorMessageInfo>();

		private TaskCompletionSource<ActorMessageInfo> tcs;

		public void Awake()
		{
			this.entityActorHandler = new CommonEntityActorHandler();

			this.actorId = this.Entity.Id;
			Game.Scene.GetComponent<ActorManagerComponent>().Add(this.Entity);
			this.HandleAsync();
		}

		public void Awake(IEntityActorHandler iEntityActorHandler)
		{
			this.entityActorHandler = iEntityActorHandler;

			this.actorId = this.Entity.Id;
			Game.Scene.GetComponent<ActorManagerComponent>().Add(this.Entity);
			this.HandleAsync();
		}

		public async Task AddLocation()
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Add(this.actorId);
		}

		public async Task RemoveLocation()
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Remove(this.actorId);
		}

		public void Add(ActorMessageInfo info)
		{
			this.queue.Enqueue(info);
			
			if (this.tcs == null)
			{
				return;
			}

			var t = this.tcs;
			this.tcs = null;
			t.SetResult(this.queue.Dequeue());
		}

		private Task<ActorMessageInfo> GetAsync()
		{
			if (this.queue.Count > 0)
			{
				return Task.FromResult(this.queue.Dequeue());
			}

			this.tcs = new TaskCompletionSource<ActorMessageInfo>();
			return this.tcs.Task;
		}

		private async void HandleAsync()
		{
			while (true)
			{
				try
				{
					ActorMessageInfo info = await this.GetAsync();
					await this.entityActorHandler.Handle(info.Session, this.Entity, info.Message);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public override void Dispose()
		{
			try
			{
				if (this.Id == 0)
				{
					return;
				}

				base.Dispose();

				Game.Scene.GetComponent<ActorManagerComponent>().Remove(actorId);
			}
			catch (Exception)
			{
				Log.Error($"unregister actor fail: {this.actorId}");
			}
		}
	}
}