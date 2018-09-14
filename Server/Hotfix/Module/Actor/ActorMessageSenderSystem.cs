using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class ActorMessageSenderAwakeSystem : AwakeSystem<ActorMessageSender, long>
	{
		public override void Awake(ActorMessageSender self, long actorId)
		{
			self.Id = actorId;
			self.ActorId = actorId;
			self.Address = StartConfigComponent.Instance.Get(IdGenerater.GetAppIdFromId(self.ActorId)).GetComponent<InnerConfig>().IPEndPoint;
		}
	}
	
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