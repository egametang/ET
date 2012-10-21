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
				connected += value;
			}
			remove
			{
				connected -= value;
			}
		}

		public event Action<Event> Received
		{
			add
			{
				received += value;
			}
			remove
			{
				received -= value;
			}
		}

		public event Action<Event> Disconnect
		{
			add
			{
				disconnect += value;
			}
			remove
			{
				disconnect -= value;
			}
		}

		internal void OnConnected(Event e)
		{
			if (connected == null)
			{
				return;
			}
			connected(e);
		}

		internal void OnReceived(Event e)
		{
			if (received == null)
			{
				return;
			}
			received(e);
		}

		internal void OnDisconnect(Event e)
		{
			if (disconnect == null)
			{
				return;
			}
			disconnect(e);
		}
	}
}
