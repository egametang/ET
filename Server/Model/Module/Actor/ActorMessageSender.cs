using System.Net;

namespace ETModel
{
	// 知道对方的instanceId，使用这个类发actor消息
	public class ActorMessageSender : ComponentWithId
	{
		// actor的地址
		public IPEndPoint Address;

		public long ActorId;
	}
}