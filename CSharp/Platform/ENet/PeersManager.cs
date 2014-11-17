using System;
using System.Collections.Generic;

namespace ENet
{
	internal class PeersManager
	{
		private readonly Dictionary<IntPtr, ESocket> peersManager = new Dictionary<IntPtr, ESocket>();

		public void Add(IntPtr peerPtr, ESocket eSocket)
		{
			this.peersManager.Add(peerPtr, eSocket);
		}

		public void Remove(IntPtr peerPtr)
		{
			this.peersManager.Remove(peerPtr);
		}

		public bool ContainsKey(IntPtr peerPtr)
		{
			if (this.peersManager.ContainsKey(peerPtr))
			{
				return true;
			}
			return false;
		}

		public ESocket this[IntPtr peerPtr]
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