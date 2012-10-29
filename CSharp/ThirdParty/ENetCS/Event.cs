namespace ENet
{
	public class Event
	{
		private readonly Host host;
		private readonly ENetEvent ev;

		public Event(Host host, ENetEvent ev)
		{
			this.host = host;
			this.ev = ev;
		}

		public ENetEvent Ev
		{
			get
			{
				return this.ev;
			}
		}

		public Packet Packet
		{
			get
			{
				return new Packet(this.host, this.Ev.packet);
			}
		}

		public Peer Peer
		{
			get
			{
				Peer peer = this.host.PeersManager[this.Ev.peer];
				return peer;
			}
		}

		public EventType Type
		{
			get
			{
				return this.Ev.type;
			}
		}
	}
}