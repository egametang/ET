using Base;

namespace Model
{
	[ObjectEvent]
	public class NetInnerComponentEvent : ObjectEvent<NetInnerComponent>, IUpdate, IAwake, IAwake<string, int>
	{
		public void Update()
		{
			NetworkComponent component = this.GetValue();
			component.Update();
		}

		public void Awake()
		{
			this.GetValue().Awake();
		}

		public void Awake(string host, int port)
		{
			this.GetValue().Awake(host, port);
		}
	}

	public class NetInnerComponent : NetworkComponent
	{
		public void Awake()
		{
			this.Awake(NetworkProtocol.TCP);
		}

		public void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.TCP, host, port);
		}
	}
}