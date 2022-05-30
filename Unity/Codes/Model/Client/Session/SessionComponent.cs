namespace ET.Client
{
	[ComponentOf(typeof(Scene))]
	public class SessionComponent: Entity, IAwake, IDestroy
	{
		public Session Session { get; set; }
	}
}
