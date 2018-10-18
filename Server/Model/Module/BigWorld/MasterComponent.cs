using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ETModel
{
	public class LockInfo
	{
		public IPEndPoint Address;
		public ETTaskCompletionSource<bool> Tcs;

		public LockInfo(IPEndPoint address, ETTaskCompletionSource<bool> tcs)
		{
			this.Address = address;
			this.Tcs = tcs;
		}
	}
	
	public class MasterComponent : Component
	{
		/// 镜像的地址
		public readonly List<IPEndPoint> ghostsAddress = new List<IPEndPoint>();

		/// 当前获取锁的进程地址
		public IPEndPoint lockedAddress;

		/// 请求锁的队列
		public readonly Queue<LockInfo> queue = new Queue<LockInfo>();
	}
}