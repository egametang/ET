namespace ET.Server
{
	[ComponentOf(typeof(Session))]
	public class SessionPlayerComponent : Entity, IAwake, IDestroy
	{
		private long playerInstanceId;

		public Player Player
		{
			get
			{
				return Root.Instance.Get(this.playerInstanceId) as Player;
			}
			set
			{
				this.playerInstanceId = value.InstanceId;
			}
		}
	}
}