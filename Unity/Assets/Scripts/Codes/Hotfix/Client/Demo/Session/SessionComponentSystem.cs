namespace ET.Client
{
	public class SessionComponentDestroySystem: DestroySystem<SessionComponent>
	{
		protected override void Destroy(SessionComponent self)
		{
			self.Session.Dispose();
		}
	}
}
