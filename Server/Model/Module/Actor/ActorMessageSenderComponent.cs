using System;
using System.Net;

namespace ETModel
{
	public class ActorMessageSenderComponent: Component
	{
		public ActorMessageSender Get(long actorId)
		{
			if (actorId == 0)
			{
				throw new Exception($"actor id is 0");
			}
			IPEndPoint ipEndPoint = StartConfigComponent.Instance.GetInnerAddress(IdGenerater.GetAppIdFromId(actorId));
			ActorMessageSender actorMessageSender = new ActorMessageSender(actorId, ipEndPoint);
			return actorMessageSender;
		}
	}
}
