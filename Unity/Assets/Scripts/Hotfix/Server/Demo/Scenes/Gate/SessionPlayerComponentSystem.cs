

namespace ET.Server
{
	public static class SessionPlayerComponentSystem
	{
		public class SessionPlayerComponentDestroySystem: DestroySystem<SessionPlayerComponent>
		{
			protected override void Destroy(SessionPlayerComponent self)
			{
				// 发送断线消息
				ActorLocationSenderComponent.Instance?.Get(LocationType.Unit)?.Send(self.Player.Id, new G2M_SessionDisconnect());
				self.DomainScene().GetComponent<PlayerComponent>()?.RemoveChild(self.Player.Id);
			}
		}
	}
}
