namespace ET
{
	public class SessionComponentDestroySystem: DestroySystem<SessionComponent>
	{
		public override void Destroy(SessionComponent self)
		{
			self.Session.Dispose();
		}
	}
}
