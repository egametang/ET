using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Model
{
	public enum LockStatus
	{
		LockedNot,
		LockRequesting,
		Locked,
	}

	[ObjectSystem]
	public class LockComponentSystem : ObjectSystem<LockComponent>, IAwake<IPEndPoint>
	{
		public void Awake(IPEndPoint a)
		{
			this.Get().Awake(a);
		}
	}

	/// <summary>
	/// 分布式锁组件,Unit对象可能在不同进程上有镜像,访问该对象的时候需要对他加锁
	/// </summary>
	public class LockComponent: Component
	{
		private LockStatus status = LockStatus.LockedNot;
		private IPEndPoint address;
		private int lockCount;
		private readonly Queue<TaskCompletionSource<bool>> queue = new Queue<TaskCompletionSource<bool>>();

		public void Awake(IPEndPoint addr)	
		{
			this.address = addr;
		}
		
		public async Task Lock()
		{
			++this.lockCount;

			if (this.status == LockStatus.Locked)
			{
				return;
			}
			if (this.status == LockStatus.LockRequesting)
			{
				await WaitLock();
				return;
			}
			
			this.status = LockStatus.LockRequesting;

			// 真身直接本地请求锁,镜像需要调用Rpc获取锁
			MasterComponent masterComponent = this.Entity.GetComponent<MasterComponent>();
			if (masterComponent != null)
			{
				await masterComponent.Lock(this.address);
			}
			else
			{
				RequestLock();
				await WaitLock();
			}
		}

		private Task<bool> WaitLock()
		{
			if (this.status == LockStatus.Locked)
			{
				return Task.FromResult(true);
			}

			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			this.queue.Enqueue(tcs);
			return tcs.Task;
		}

		private async void RequestLock()
		{
			try
			{
				Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.address);
				string serverAddress = Game.Scene.GetComponent<StartConfigComponent>().StartConfig.ServerIP;
				G2G_LockRequest request = new G2G_LockRequest { Id = this.Parent.Id, Address = serverAddress };
				await session.Call(request);

				this.status = LockStatus.Locked;

				foreach (TaskCompletionSource<bool> taskCompletionSource in this.queue)
				{
					taskCompletionSource.SetResult(true);
				}
				this.queue.Clear();
			}
			catch (Exception e)
			{
				Log.Error($"获取锁失败: {this.address} {this.Parent.Id} {e}");
			}
		}

		public async Task Release()
		{
			--this.lockCount;
			if (this.lockCount != 0)
			{
				return;
			}

			this.status = LockStatus.LockedNot;
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.address);
			G2G_LockReleaseRequest request = new G2G_LockReleaseRequest();
			await session.Call(request);
		}
	}
}