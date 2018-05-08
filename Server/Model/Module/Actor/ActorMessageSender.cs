using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ETModel
{
	public sealed class ActorMessageSender : Entity
	{
		// actor的地址
		public IPEndPoint Address;

		public long ActorId;
		
		// 还没发送的消息
		public Queue<ActorTask> WaitingTasks = new Queue<ActorTask>();
		
		// 最近发送消息的时间
		public long LastSendTime;

		public TaskCompletionSource<ActorTask> Tcs;

		public int FailTimes;

		public int MaxFailTimes;

		public int Error;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			this.Error = 0;
		}
	}
}