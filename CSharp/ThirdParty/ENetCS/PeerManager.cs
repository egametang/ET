using System;
using System.Collections.Generic;

namespace ENet
{
	public class PeerManager
	{
		private int num = 0;
		private readonly Dictionary<int, ENetPeer> peers = new Dictionary<int, ENetPeer>();

		public void Add(ENetPeer peer)
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

		public ENetPeer this[int key]
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
