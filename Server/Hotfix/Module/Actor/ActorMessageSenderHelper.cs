using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	public static class ActorMessageSenderHelper
	{
		public static void Send(this ActorMessageSender self, IActorMessage message)
		{   
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.Address);
			message.ActorId = self.ActorId;
			session.Send(message);
		}
		
		public static async Task<IActorResponse> Call(this ActorMessageSender self, IActorRequest message)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.Address);
			message.ActorId = self.ActorId;
			return (IActorResponse)await session.Call(message);
		}
	}
}