namespace ET.Server
{
	[ComponentOf(typeof(Player))]
	public class SessionInfoComponent : Entity, IAwake
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