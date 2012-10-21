using System;
using System.Collections.Generic;

namespace ENet
{
	public class PeerEventsManager
	{
		private readonly Dictionary<IntPtr, PeerEvent> peerEvents = new Dictionary<IntPtr, PeerEvent>();

		public void Add(IntPtr peer)
		{
			this.peerEvents.Add(peer, new PeerEvent());
		}

		public void Remove(IntPtr peer)
		{
			this.peerEvents.Remove(peer);
		}

		public PeerEvent this[IntPtr peer]
		{
			get
			{
				if (!peerEvents.ContainsKey(peer))
				{
					throw new KeyNotFoundException("No Peer Key");
				}
				return peerEvents[peer];
			}
		}

		internal void OnConnected(IntPtr peer, Event e)
		{
			var peerEvent = peerEvents[peer];
			if (peerEvent == null)
			{
				return;
			}
			peerEvent.OnConnected(e);
		}

		internal void OnReceived(IntPtr peer, Event e)
		{
			var peerEvent = peerEvents[peer];
			if (peerEvent == null)
			{
				return;
			}
			peerEvent.OnReceived(e);
		}

		internal void OnDisconnect(IntPtr peer, Event e)
		{
			var peerEvent = peerEvents[peer];
			if (peerEvent == null)
			{
				return;
			}
			peerEvent.OnDisconnect(e);
		}
	}
}
