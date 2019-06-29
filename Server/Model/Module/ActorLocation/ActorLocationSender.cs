using System.Collections.Generic;
using System.Net;

namespace ETModel
{
	// 知道对方的Id，使用这个类发actor消息
	public class ActorLocationSender : ComponentWithId
	{
		public long ActorId;
		
		// 还没发送的消息
		public Queue<ActorTask> WaitingTasks = new Queue<ActorTask>();

		// 最近接收消息的时间
		public long LastRecvTime;
		
		public int FailTimes;

		public const int MaxFailTimes = 5;
		
		public ETTaskCompletionSource<ActorTask> Tcs;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();
		}
	}
}