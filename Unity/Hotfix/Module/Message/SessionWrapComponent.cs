using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class SessionComponentAwakeSystem : AwakeSystem<SessionWrapComponent>
	{
		public override void Awake(SessionWrapComponent self)
		{
			self.Awake();
		}
	}

	public class SessionWrapComponent: Component
	{
		public static SessionWrapComponent Instance;

		public SessionWrap Session;

		public void Awake()
		{
			Instance = this;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.Session.Dispose();
			this.Session = null;
			Instance = null;
		}
	}
}
