using System.Net;

namespace ETModel
{
	// 知道对方的instanceId，使用这个类发actor消息
	public struct ActorMessageSender
	{
		// actor的地址
		public IPEndPoint Address;

		public long ActorId;

		public ActorMessageSender(long actorId, IPEndPoint address)
		{
			this.ActorId = actorId;
			this.Address = address;
		}
	}
}