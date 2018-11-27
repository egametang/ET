using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class SessionComponentAwakeSystem : AwakeSystem<SessionComponent>
	{
		public override void Awake(SessionComponent self)
		{
			self.Awake();
		}
	}

	public class SessionComponent: Component
	{
		public static SessionComponent Instance;

		private Session session;

		public Session Session
		{
			get
			{
				return this.session;
			}
			set
			{
				this.session = value;
				
				if (this.session != null)
				{
					this.session.Parent = this;
				}
			}
		}

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
