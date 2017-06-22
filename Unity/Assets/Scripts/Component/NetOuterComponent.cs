namespace Model
{
	[EntityEvent(EntityEventId.NetOuterComponent)]
	public class NetOuterComponent: NetworkComponent, IAwake, IAwake<string, int>, IUpdate
	{
		public void Awake()
		{
			this.Awake(NetworkProtocol.UDP);
		}

		public void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.UDP, host, port);
		}

		public new void Update()
		{
			base.Update();
		}
	}
}