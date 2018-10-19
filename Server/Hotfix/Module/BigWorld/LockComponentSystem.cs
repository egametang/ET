using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class LockComponentAwakeSystem : AwakeSystem<LockComponent, IPEndPoint>
	{
		public override void Awake(LockComponent self, IPEndPoint a)
		{
			self.Awake(a);
		}
	}

	/// <summary>
	/// 分布式锁组件,Unit对象可能在不同进程上有镜像,访问该对象的时候需要对他加锁
	/// </summary>
	public static class LockComponentEx
	{
		public static void Awake(this LockComponent self, IPEndPoint addr)	
		{
			self.address = addr;
		}
		
		public static async ETTask Lock(this LockComponent self)
		{
			++self.lockCount;

			if (self.status == LockStatus.Locked)
			{
				return;
			}
			if (self.status == LockStatus.LockRequesting)
			{
				await self.WaitLock();
				return;
			}
			
			self.status = LockStatus.LockRequesting;

			// 真身直接本地请求锁,镜像需要调用Rpc获取锁
			MasterComponent masterComponent = self.Entity.GetComponent<MasterComponent>();
			if (masterComponent != null)
			{
				await masterComponent.Lock(self.address);
			}
			else
			{
				self.RequestLock().NoAwait();
				await self.WaitLock();
			}
		}

		private static ETTask<bool> WaitLock(this LockComponent self)
		{
			if (self.status == LockStatus.Locked)
			{
				return ETTask.FromResult(true);
			}

			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			self.queue.Enqueue(tcs);
			return tcs.Task;
		}

		private static async ETVoid RequestLock(this LockComponent self)
		{
			try
			{
				Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.address);
				string serverAddress = StartConfigComponent.Instance.StartConfig.ServerIP;
				G2G_LockRequest request = new G2G_LockRequest { Id = self.Entity.Id, Address = serverAddress };
				await session.Call(request);

				self.status = LockStatus.Locked;

				foreach (ETTaskCompletionSource<bool> taskCompletionSource in self.queue)
				{
					taskCompletionSource.SetResult(true);
				}
				self.queue.Clear();
			}
			catch (Exception e)
			{
				Log.Error($"获取锁失败: {self.address} {self.Entity.Id} {e}");
			}
		}

		public static async ETTask Release(this LockComponent self)
		{
			--self.lockCount;
			if (self.lockCount != 0)
			{
				return;
			}

			self.status = LockStatus.LockedNot;
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.address);
			G2G_LockReleaseRequest request = new G2G_LockReleaseRequest();
			await session.Call(request);
		}
	}
}