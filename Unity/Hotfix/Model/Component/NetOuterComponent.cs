using Model;

namespace Model
{
	[EntityEvent(EntityEventId.NetOuterComponent)]
	public class NetOuterComponent: NetworkComponent
	{
		private void Awake()
		{
			this.Awake(NetworkProtocol.UDP);
		}

		private void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.UDP, host, port);
		}

		private new void Update()
		{
			base.Update();
		}
	}
}