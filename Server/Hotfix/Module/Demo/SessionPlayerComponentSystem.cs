using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class SessionPlayerComponentDestroySystem : DestroySystem<SessionPlayerComponent>
	{
		public override void Destroy(SessionPlayerComponent self)
		{
			// 发送断线消息
			ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(self.Player.UnitId);
			actorLocationSender.Send(new G2M_SessionDisconnect());
			Game.Scene.GetComponent<PlayerComponent>()?.Remove(self.Player.Id);
		}
	}
}