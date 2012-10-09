using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENet
{
	public class ENetClient
	{
		private readonly Host host;

		private readonly Dictionary<uint, TaskCompletionSource<Peer>> peerTcs = new Dictionary<uint, TaskCompletionSource<Peer>>();

		public ENetClient(uint peerLimit)
		{
			host = new Host(null, peerLimit);
		}

		public void Poll(int timeout)
		{
			while (host.Service(timeout) >= 0)
			{
				Event e;
				while (host.CheckEvents(out e) > 0)
				{
					switch (e.Type)
					{
						case EventType.Connect:
						{
							var tcs = peerTcs[e.Peer.ConnectID];
							tcs.TrySetResult(e.Peer);
							peerTcs.Remove(e.Peer.ConnectID);
							break;
						}
						case EventType.Receive:
						{
							break;
						}
						case EventType.Disconnect:
						{
							break;
						}
					}
				}
			}
		}

		public Task<Peer> ConnectAsync(string hostName, ushort port)
		{
			var tcs = new TaskCompletionSource<Peer>();
			var address = new Address { Host = "192.168.10.246", Port = 8901 };
			var peer = this.host.Connect(address, 2, 1);
			uint id = peer.ConnectID;
			peerTcs[id] = tcs;
			var t = tcs.Task;
			return t;
		}
	}
}
