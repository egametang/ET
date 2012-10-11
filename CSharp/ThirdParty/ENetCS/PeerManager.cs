using System;
using System.Collections.Generic;

namespace ENet
{
	public class PeerManager
	{
		private int num = 0;
		private readonly Dictionary<int, Peer> peers = new Dictionary<int, Peer>();

		public void Add(Peer peer)
		{
			++num;
			unsafe
			{
				peer.NativeData->data = (IntPtr)num;
			}
			peers[num] = peer;
		}

		public void Remove(int key)
		{
			peers.Remove(key);
		}

		public bool ContainsKey(int key)
		{
			if (peers.ContainsKey(key))
			{
				return true;
			}
			return false;
		}

		public Peer this[int key]
		{
			get
			{
				return this.peers[key];
			}
			set
			{
				this.peers[key] = value;
			}
		}
	}
}
