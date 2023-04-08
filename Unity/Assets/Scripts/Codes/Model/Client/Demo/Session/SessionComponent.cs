namespace ET.Client
{
	[ComponentOf(typeof(Scene))]
	public class SessionComponent: Entity, IAwake, IDestroy
	{
		private long sessionInstanceId;

		public Session Session
		{
			get
			{
				return Root.Instance.Get(this.sessionInstanceId) as Session;
			}
			set
			{
				this.sessionInstanceId = value.InstanceId;
			}
		}
	}
}
