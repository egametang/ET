using System;

namespace ETModel
{
	// 知道对方的instanceId，使用这个类发actor消息
	public struct ActorMessageSender
	{
		// 最近接收或者发送消息的时间
		public long CreateTime { get; }
		// actor的地址
		public Action<IActorResponse> Callback { get; }

		public ActorMessageSender(Action<IActorResponse> callback)
		{
			this.CreateTime = TimeHelper.Now();
			this.Callback = callback;
		}
	}
}