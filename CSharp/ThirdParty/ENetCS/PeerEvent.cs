using System;

namespace ENet
{
	public class PeerEvent
	{
		private Action<Event> connected;
		private Action<Event> received;
		private Action<Event> disconnect;

		public event Action<Event> Connected
		{
			add
			{
				this.connected += value;
			}
			remove
			{
				this.connected -= value;
			}
		}

		public event Action<Event> Received
		{
			add
			{
				this.received += value;
			}
			remove
			{
				this.received -= value;
			}
		}

		public event Action<Event> Disconnect
		{
			add
			{
				this.disconnect += value;
			}
			remove
			{
				this.disconnect -= value;
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