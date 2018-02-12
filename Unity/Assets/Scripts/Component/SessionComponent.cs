namespace Model
{
	[ObjectSystem]
	public class SessionComponentSystem : ObjectSystem<SessionComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}

	public class SessionComponent: Component
	{
		public static SessionComponent Instance;

		public Session Session;

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
