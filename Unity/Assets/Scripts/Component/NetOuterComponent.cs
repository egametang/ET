using Base;

namespace Model
{
	[DisposerEvent(typeof(NetOuterComponent))]
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