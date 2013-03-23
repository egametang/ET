using System;
using System.Threading.Tasks;

namespace ENet
{
	public sealed class ServerHost: Host
	{
		public ServerHost(string hostName, ushort port, 
			uint peerLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID,
			uint channelLimit = 0, uint incomingBandwidth = 0, uint outgoingBandwidth = 0)
		{
			if (peerLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}
			CheckChannelLimit(channelLimit);

			var address = new Address { HostName = hostName, Port = port };
			ENetAddress nativeAddress = address.Struct;
			this.host = NativeMethods.enet_host_create(
				ref nativeAddress, peerLimit, channelLimit, incomingBandwidth, 
				outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new ENetException("Host creation call failed.");
			}
		}

		public Task<Peer> AcceptAsync()
		{
			if (this.PeersManager.ContainsKey(IntPtr.Zero))
			{
				throw new ENetException("Do Not Accept Twice!");
			}
			var tcs = new TaskCompletionSource<Peer>();
			var peer = new Peer(IntPtr.Zero);
			this.PeersManager.Add(peer.PeerPtr, peer);
			peer.PeerEvent.Connected += e => tcs.TrySetResult(peer);
			return tcs.Task;
		}

		public void RunOnce(int timeout = 0)
		{
			this.OnEvents();

			if (this.Service(timeout) < 0)
			{
				return;
			}

			while (true)
			{
				Event ev = this.GetEvent();
				if (ev == null)
				{
					return;
				}

				switch (ev.Type)
				{
					case EventType.Connect:
					{
						var peer = this.PeersManager[IntPtr.Zero];

						this.PeersManager.Remove(IntPtr.Zero);

						peer.PeerPtr = ev.PeerPtr;
						this.PeersManager.Add(peer.PeerPtr, peer);

						PeerEvent peerEvent = peer.PeerEvent;
						peerEvent.OnConnected(ev);
						break;
					}
					case EventType.Receive:
					{
						var peer = this.PeersManager[ev.PeerPtr];
						peer.PeerEvent.OnReceived(ev);
						peer.PeerEvent.Received = null;
						break;
					}
					case EventType.Disconnect:
					{
						ev.EventState = EventState.DISCONNECTED;

						var peer = this.PeersManager[ev.PeerPtr];
						PeerEvent peerEvent = peer.PeerEvent;

						this.PeersManager.Remove(ev.PeerPtr);
						// enet_peer_disconnect 会 reset Peer,这里设置为0,防止再次Dispose
						peer.PeerPtr = IntPtr.Zero;

						if (peerEvent.Received != null)
						{
							peerEvent.OnReceived(ev);
						}
						else
						{
							peerEvent.OnDisconnect(ev);
						}
						break;
					}
				}
			}
		}

		public void Start(int timeout = 0)
		{
			while (this.isRunning)
			{
				this.RunOnce(timeout);
			}
		}
	}
}