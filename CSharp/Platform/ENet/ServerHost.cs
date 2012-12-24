using System;
using System.Threading.Tasks;
using Log;

namespace ENet
{
	public sealed class ServerHost : Host
	{
		private Action<Peer> acceptEvent;

		public ServerHost(Address address, 
				uint peerLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID,
				uint channelLimit = 0, uint incomingBandwidth = 0,
				uint outgoingBandwidth = 0, bool enableCrc = true)
		{
			if (peerLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}
			CheckChannelLimit(channelLimit);

			ENetAddress nativeAddress = address.Struct;
			this.host = NativeMethods.enet_host_create(
					ref nativeAddress, peerLimit,
					channelLimit, incomingBandwidth, outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new ENetException(0, "Host creation call failed.");
			}

			if (enableCrc)
			{
				NativeMethods.enet_enable_crc(this.host);
			}
		}

		public Action<Peer> AcceptEvent
		{
			set
			{
				this.acceptEvent = value;
			}
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
						if (this.acceptEvent == null)
						{
							throw new ENetException(4, "No Accept Event!");
						}
						var peer = new Peer(ev.PeerPtr);
						this.PeersManager.Add(peer.PeerPtr, peer);
						this.acceptEvent(peer);
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
			while (isRunning)
			{
				RunOnce(timeout);
			}
		}
	}
}