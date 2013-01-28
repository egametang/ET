using System;
using System.Threading.Tasks;

namespace ENet
{
	public sealed class ClientHost: Host
	{
		public ClientHost(
			uint peerLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, uint channelLimit = 0,
			uint incomingBandwidth = 0, uint outgoingBandwidth = 0, bool enableCrc = true)
		{
			if (peerLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}
			CheckChannelLimit(channelLimit);

			this.host = NativeMethods.enet_host_create(
				IntPtr.Zero, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new ENetException(0, "Host creation call failed.");
			}

			if (enableCrc)
			{
				this.EnableCrc();
			}
		}

		public Task<Peer> ConnectAsync(Address address, 
				uint channelLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT,
				uint data = 0)
		{
			CheckChannelLimit(channelLimit);

			var tcs = new TaskCompletionSource<Peer>();
			ENetAddress nativeAddress = address.Struct;
			IntPtr peerPtr = NativeMethods.enet_host_connect(
				this.host, ref nativeAddress, channelLimit, data);
			if (peerPtr == IntPtr.Zero)
			{
				throw new ENetException(0, "Host connect call failed.");
			}
			var peer = new Peer(peerPtr);
			this.PeersManager.Add(peerPtr, peer);
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

			Event ev;
			while (this.CheckEvents(out ev) > 0)
			{
				switch (ev.Type)
				{
					case EventType.Connect:
					{
						var peer = this.PeersManager[ev.PeerPtr];
						peer.PeerEvent.OnConnected(ev);
						peer.PeerEvent.Connected = null;
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

						this.PeersManager.Remove(peer.PeerPtr);
						// enet_peer_disconnect会reset Peer,这里设置为0,防止再次Dispose
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