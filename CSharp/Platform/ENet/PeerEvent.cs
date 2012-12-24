using System;

namespace ENet
{
	public class PeerEvent
	{
		private Action<Event> connected;
		private Action<Event> received;
		private Action<Event> disconnect;

		public Action<Event> Connected
		{
			get
			{
				return this.connected;
			}
			set
			{
				this.connected = value;
			}
		}

		public Action<Event> Received
		{
			get
			{
				return this.received;
			}
			set
			{
				this.received = value;
			}
		}

		public Action<Event> Disconnect
		{
			get
			{
				return this.disconnect;
			}
			set
			{
				this.disconnect = value;
			}
		}

		internal void OnConnected(Event e)
		{
			if (this.connected == null)
			{
				return;
			}
			this.connected(e);
		}

		internal void OnReceived(Event e)
		{
			if (this.received == null)
			{
				return;
			}
			this.received(e);
		}

		internal void OnDisconnect(Event e)
		{
			if (this.disconnect == null)
			{
				return;
			}
			this.disconnect(e);
		}
	}
}