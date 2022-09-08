

namespace ET.Server
{
	public static class SessionPlayerComponentSystem
	{
		public class SessionPlayerComponentDestroySystem: DestroySystem<SessionPlayerComponent>
		{
			protected override void Destroy(SessionPlayerComponent self)
			{
				// 发送断线消息
				ActorLocationSenderComponent.Instance?.Send(self.PlayerId, new G2M_SessionDisconnect());
				self.DomainScene().GetComponent<PlayerComponent>()?.Remove(self.PlayerId);
			}
		}

		public static Player GetMyPlayer(this SessionPlayerComponent self)
		{
			return self.DomainScene().GetComponent<PlayerComponent>().Get(self.PlayerId);
		}
	}
}
