namespace Model
{
	[ObjectSystem]
	public class NetOuterComponentSystem : ObjectSystem<NetOuterComponent>, IAwake, IUpdate
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
			this.Awake(NetworkProtocol.TCP);
			this.MessagePacker = new ProtobufPacker();
			// 由hotfix中设置
			//this.MessageDispatcher = new ClientDispatcher();
		}

		public new void Update()
		{
			base.Update();
		}
	}
}