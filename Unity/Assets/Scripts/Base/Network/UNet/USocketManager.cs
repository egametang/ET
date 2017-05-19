using System;
using System.Collections.Generic;

namespace Model
{
	internal class USocketManager
	{
		private readonly Dictionary<IntPtr, USocket> sockets = new Dictionary<IntPtr, USocket>();

		public void Add(IntPtr peerPtr, USocket uSocket)
		{
			this.sockets.Add(peerPtr, uSocket);
		}

		public void Remove(IntPtr peerPtr)
		{
			this.sockets.Remove(peerPtr);
		}

		public bool ContainsKey(IntPtr peerPtr)
		{
			if (this.sockets.ContainsKey(peerPtr))
			{
				return true;
			}
			return false;
		}

		public USocket this[IntPtr peerPtr]
		{
			get
			{
				if (!this.sockets.ContainsKey(peerPtr))
				{
					throw new KeyNotFoundException("No Peer Key");
				}
				return this.sockets[peerPtr];
			}
		}
	}
}