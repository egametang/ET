using System.Net;
using ETModel;

namespace ETHotfix
{
	public static class MasterComponentEx
	{
		public static void AddGhost(this MasterComponent self, IPEndPoint address)
		{
			self.ghostsAddress.Add(address);
		}

		public static void RemoveGhost(this MasterComponent self, IPEndPoint address)
		{
			self.ghostsAddress.Remove(address);
		}

		public static ETTask Lock(this MasterComponent self, IPEndPoint address)
		{
			if (self.lockedAddress == null)
			{
				self.lockedAddress = address;
				return ETTask.FromResult(true);
			}

			ETTaskCompletionSource tcs = new ETTaskCompletionSource();
			LockInfo lockInfo = new LockInfo(address, tcs);
			self.queue.Enqueue(lockInfo);
			return tcs.Task;
		}

		public static void Release(this MasterComponent self, IPEndPoint address)
		{
			if (!self.lockedAddress.Equals(address))
			{
				Log.Error($"解锁地址与锁地址不匹配! {self.lockedAddress} {address}");
				return;
			}
			if (self.queue.Count == 0)
			{
				self.lockedAddress = null;
				return;
			}
			LockInfo lockInfo = self.queue.Dequeue();
			self.lockedAddress = lockInfo.Address;
			lockInfo.Tcs.SetResult();
		}
	}
}