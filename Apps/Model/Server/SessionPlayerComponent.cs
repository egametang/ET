namespace ET.Server
{
	public class SessionPlayerComponent : Entity, IAwake, IDestroy
	{
		public long PlayerId { get; set; }
	}
}