using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Base
{
	public static class NativeMethods
	{
#if UNITY_IOS && !UNITY_EDITOR
		private const string LIB = "__Internal";
#else
		private const string LIB = "ENet";
#endif

		public const int ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT = 255;
		public const int ENET_PROTOCOL_MAXIMUM_PEER_ID = 0xfff;

		[DllImport(LIB)]
		internal static extern int enet_address_set_host(ref ENetAddress address, string hostName);

		[DllImport(LIB)]
		internal static extern int enet_address_get_host(ref ENetAddress address, StringBuilder hostName, uint nameLength);

		[DllImport(LIB)]
		internal static extern int enet_address_get_host_ip(ref ENetAddress address, StringBuilder hostIp, uint ipLength);

		[DllImport(LIB)]
		internal static extern void enet_deinitialize();

		[DllImport(LIB)]
		internal static extern int enet_initialize();

		[DllImport(LIB)]
		internal static extern IntPtr enet_host_create(
				ref ENetAddress address, uint peerLimit, uint channelLimit, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport(LIB)]
		internal static extern IntPtr enet_host_create(IntPtr address, uint peerLimit, uint channelLimit, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport(LIB)]
		internal static extern void enet_host_destroy(IntPtr host);

		[DllImport(LIB)]
		internal static extern IntPtr enet_host_connect(IntPtr host, ref ENetAddress address, uint channelCount, uint data);

		[DllImport(LIB)]
		internal static extern void enet_host_broadcast(IntPtr host, byte channelID, IntPtr packet);

		[DllImport(LIB)]
		internal static extern void enet_host_compress(IntPtr host, IntPtr compressor);

		[DllImport(LIB)]
		internal static extern int enet_host_compress_with_range_coder(IntPtr host);

		[DllImport(LIB)]
		internal static extern void enet_host_channel_limit(IntPtr host, uint channelLimit);

		[DllImport(LIB)]
		internal static extern void enet_host_bandwidth_limit(IntPtr host, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport(LIB)]
		internal static extern void enet_host_flush(IntPtr host);

		[DllImport(LIB)]
		internal static extern int enet_host_check_events(IntPtr host, ref ENetEvent ev);

		[DllImport(LIB)]
		internal static extern int enet_host_service(IntPtr host, IntPtr ev, uint timeout);

		[DllImport(LIB)]
		internal static extern uint enet_time_get();

		[DllImport(LIB)]
		internal static extern void enet_time_set(uint newTimeBase);

		[DllImport(LIB)]
		internal static extern IntPtr enet_packet_create(byte[] data, uint dataLength, PacketFlags flags);

		[DllImport(LIB)]
		internal static extern void enet_packet_destroy(IntPtr packet);

		[DllImport(LIB)]
		internal static extern int enet_packet_resize(IntPtr packet, uint dataLength);

		[DllImport(LIB)]
		internal static extern void enet_peer_throttle_configure(IntPtr peer, uint interval, uint acceleration, uint deceleration);

		[DllImport(LIB)]
		internal static extern int enet_peer_send(IntPtr peer, byte channelID, IntPtr packet);

		[DllImport(LIB)]
		internal static extern IntPtr enet_peer_receive(IntPtr peer, out byte channelID);

		[DllImport(LIB)]
		internal static extern void enet_peer_reset(IntPtr peer);

		[DllImport(LIB)]
		internal static extern void enet_peer_ping(IntPtr peer);

		[DllImport(LIB)]
		internal static extern void enet_peer_disconnect_now(IntPtr peer, uint data);

		[DllImport(LIB)]
		internal static extern void enet_peer_disconnect(IntPtr peer, uint data);

		[DllImport(LIB)]
		internal static extern void enet_peer_disconnect_later(IntPtr peer, uint data);
	}
}