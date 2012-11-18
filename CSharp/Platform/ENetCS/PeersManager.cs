using System;
using System.Collections.Generic;

namespace ENet
{
	public class PeersManager
	{
		private readonly Dictionary<IntPtr, Peer> peersManager = new Dictionary<IntPtr, Peer>();

		public void Add(IntPtr peerPtr, Peer peer)
		{
			this.peersManager.Add(peerPtr, peer);
		}

		public void Remove(IntPtr peerPtr)
		{
			this.peersManager.Remove(peerPtr);
		}

		public Peer this[IntPtr peerPtr]
		{
			get
			{
				if (!this.peersManager.ContainsKey(peerPtr))
				{
					throw new KeyNotFoundException("No Peer Key");
				}
				return this.peersManager[peerPtr];
			}
		}
	}
}