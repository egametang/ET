using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ETModel
{
	// 知道对方的Id，使用这个类发actor消息
	public class ActorLocationSender : ComponentWithId
	{
		// actor的地址
		public IPEndPoint Address;

		public long ActorId;
		
		// 还没发送的消息
		public Queue<ActorTask> WaitingTasks = new Queue<ActorTask>();

		// 最近发送消息的时间
		public long LastSendTime;
		
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