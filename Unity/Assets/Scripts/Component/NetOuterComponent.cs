namespace Model
{
	[ObjectEvent]
	public class NetOuterComponentEvent : ObjectEvent<NetOuterComponent>, IAwake, IUpdate
	{
		public void Awake()
		{
			this.Get().Awake();
		}
		
		public void Update()
		{
			this.Get().Update();
		}
	}

	public class NetOuterComponent : NetworkComponent
	{
		public void Awake()
		{
			this.Awake(NetworkProtocol.KCP);
			this.MessagePacker = new ProtobufPacker();
			this.MessageDispatcher = new ClientDispatcher();
		}

		public new void Update()
		{
			base.Update();
		}
	}
}