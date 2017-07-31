namespace Model
{
	[EntityEvent(EntityEventId.NetOuterComponent)]
	public class NetOuterComponent: NetworkComponent, IAwake, IAwake<string, int>, IUpdate
	{
		public void Awake()
		{
			this.Awake(NetworkProtocol.TCP);
			this.messagePacker = new JsondotnetPacker();
		}

		public void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.TCP, host, port);
			this.messagePacker = new JsondotnetPacker();
		}

		public new void Update()
		{
			base.Update();
		}
	}
}