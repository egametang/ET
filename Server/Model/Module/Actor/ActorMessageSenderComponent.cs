using System.Collections.Generic;

namespace ET
{
	public class ActorMessageSenderComponent: Entity
	{
		public static long TIMEOUT_TIME = 30 * 1000;
		
		public static ActorMessageSenderComponent Instance { get; set; }
		
		public int RpcId;
		
		public readonly Dictionary<int, ActorMessageSender> requestCallback = new Dictionary<int, ActorMessageSender>();

		public long TimeoutCheckTimer;
		
		public List<int> TimeoutActorMessageSenders = new List<int>();
	}
}
