namespace Model
{
	[ObjectEvent]
	public class NetOuterComponentEvent : ObjectEvent<NetOuterComponent>, IAwake, IAwake<string, int>
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Awake(string host, int port)
		{
			this.Get().Awake();
		}
	}

	public class NetOuterComponent : NetworkComponent
	{
		public void Awake()
		{
			this.Awake(NetworkProtocol.TCP);
			this.MessagePacker = new MongoPacker();
			this.MessageDispatcher = new ClientDispatcher();
		}

		public void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.TCP, host, port);
			this.MessagePacker = new MongoPacker();
			this.MessageDispatcher = new ClientDispatcher();
		}

		public new void Update()
		{
			base.Update();
		}
	}
}