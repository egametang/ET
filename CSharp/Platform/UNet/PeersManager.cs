using System;
using System.Collections.Generic;

namespace UNet
{
	internal class PeersManager
	{
		private readonly Dictionary<IntPtr, USocket> peersManager = new Dictionary<IntPtr, USocket>();

		public void Add(IntPtr peerPtr, USocket uSocket)
		{
			this.peersManager.Add(peerPtr, uSocket);
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

		public USocket this[IntPtr peerPtr]
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