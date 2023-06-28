namespace ET.Client
{
	[ComponentOf(typeof(Fiber))]
	public class SessionComponent: Entity, IAwake, IDestroy
	{
		private EntityRef<Session> session;

		public Session Session
		{
			get
			{
				return this.session;
			}
			set
			{
				this.session = value;
			}
		}
	}
}
