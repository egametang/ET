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
				var peerPtr = this.Ev.peer;
				if (!this.host.PeersManager.ContainsKey(this.Ev.peer))
				{
					var peer = new Peer(this.host, peerPtr);
					this.host.PeersManager.Add(peerPtr, peer);
					return peer;
				}
				else
				{
					Peer peer = this.host.PeersManager[peerPtr];
					return peer;
				}
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