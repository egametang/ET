using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class SessionPlayerComponentDestroySystem : DestroySystem<SessionPlayerComponent>
	{
		public override void Destroy(SessionPlayerComponent self)
		{
			DestroyAsync(self).Coroutine();
		}

		private static async ETVoid DestroyAsync(SessionPlayerComponent self)
        {
			// 发送断线消息
			ActorLocationSender actorLocationSender = await Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(self.Player.UnitId);
			actorLocationSender.Send(new G2M_SessionDisconnect()).Coroutine();
			Game.Scene.GetComponent<PlayerComponent>()?.Remove(self.Player.Id);
		}
	}
}