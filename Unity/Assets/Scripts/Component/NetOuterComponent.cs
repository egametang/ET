namespace Model
{
	public class NetOuterComponent : NetworkComponent, IAwake, IAwake<string, int>
	{
		public void Awake()
		{
			this.Awake(NetworkProtocol.TCP);
			this.MessagePacker = new JsondotnetPacker();
			this.MessageDispatcher = new ClientDispatcher();
		}

		public void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.TCP, host, port);
			this.MessagePacker = new JsondotnetPacker();
			this.MessageDispatcher = new ClientDispatcher();
		}

		public new void Update()
		{
			base.Update();
		}
	}
}