using Base;

namespace Model
{
	[DisposerEvent]
	public class NetOuterComponentEvent : DisposerEvent<NetOuterComponent>, IUpdate, IAwake, IAwake<string, int>
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

	public class NetOuterComponent : NetworkComponent
	{
		public void Awake()
		{
			this.Awake(NetworkProtocol.UDP);
		}

		public void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.UDP, host, port);
		}
	}
}