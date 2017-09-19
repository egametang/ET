namespace Model
{
	[ObjectEvent]
	public class NetOuterComponentEvent : ObjectEvent<NetOuterComponent>, IAwake, IAwake<string, int>, IUpdate
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Awake(string a, int b)
		{
			this.Get().Awake(a, b);
		}

		public void Update()
		{
			this.Get().Update();
		}
	}

	public class NetOuterComponent: NetworkComponent
	{
		public void Awake()
		{
			this.Awake(NetworkProtocol.TCP);
			this.MessagePacker = new MongoPacker();
			this.MessageDispatcher = new OuterMessageDispatcher();
		}

		public void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.TCP, host, port);
			this.MessagePacker = new MongoPacker();
			this.MessageDispatcher = new OuterMessageDispatcher();
		}

		public new void Update()
		{
			base.Update();
		}
	}
}