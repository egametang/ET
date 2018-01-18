using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Model
{
	public class LockInfo
	{
		public IPEndPoint Address;
		public TaskCompletionSource<bool> Tcs;

		public LockInfo(IPEndPoint address, TaskCompletionSource<bool> tcs)
		{
			this.Address = address;
			this.Tcs = tcs;
		}
	}
	
	public class MasterComponent : Component
	{
		/// 镜像的地址
		private readonly List<IPEndPoint> ghostsAddress = new List<IPEndPoint>();

		/// 当前获取锁的进程地址
		private IPEndPoint lockedAddress;

		/// 请求锁的队列
		private readonly Queue<LockInfo> queue = new Queue<LockInfo>();

		public void AddGhost(IPEndPoint address)
		{
			this.ghostsAddress.Add(address);
		}

		public void RemoveGhost(IPEndPoint address)
		{
			this.ghostsAddress.Remove(address);
		}

		public Task<bool> Lock(IPEndPoint address)
		{
			if (this.lockedAddress == null)
			{
				this.lockedAddress = address;
				return Task.FromResult(true);
			}

			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			LockInfo lockInfo = new LockInfo(address, tcs);
			this.queue.Enqueue(lockInfo);
			return tcs.Task;
		}

		public void Release(IPEndPoint address)
		{
			if (!this.lockedAddress.Equals(address))
			{
				Log.Error($"解锁地址与锁地址不匹配! {this.lockedAddress} {address}");
				return;
			}
			if (this.queue.Count == 0)
			{
				this.lockedAddress = null;
				return;
			}
			LockInfo lockInfo = this.queue.Dequeue();
			this.lockedAddress = lockInfo.Address;
			lockInfo.Tcs.SetResult(true);
		}
	}
}