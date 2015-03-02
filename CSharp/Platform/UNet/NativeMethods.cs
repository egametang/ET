using System;
using System.Runtime.InteropServices;
using System.Text;
using Common.Network;

namespace UNet
{
	public static class NativeMethods
	{
		private const string LIB = "ENet";

		public const int ENET_PEER_PACKET_THROTTLE_SCALE = 32;
		public const int ENET_PEER_PACKET_THROTTLE_ACCELERATION = 2;
		public const int ENET_PEER_PACKET_THROTTLE_DECELERATION = 2;
		public const int ENET_PEER_PACKET_THROTTLE_INTERVAL = 5000;
		public const int ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT = 1;
		public const int ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT = 255;
		public const int ENET_PROTOCOL_MAXIMUM_PEER_ID = 0xfff;
		public const uint ENET_HOST_ANY = 0;
		public const uint ENET_HOST_BROADCAST = 0xffffffff;

		[DllImport(LIB, EntryPoint = "enet_address_set_host")]
		internal static extern int ENetAddressSetHost(ref ENetAddress address, string hostName);

		[DllImport(LIB, EntryPoint = "enet_address_get_host")]
		internal static extern int ENetAddressGetHost(
				ref ENetAddress address, StringBuilder hostName, uint nameLength);

		[DllImport(LIB, EntryPoint = "enet_address_get_host_ip")]
		internal static extern int ENetAddressGetHostIp(
				ref ENetAddress address, StringBuilder hostIp, uint ipLength);

		[DllImport(LIB, EntryPoint = "enet_deinitialize")]
		internal static extern void ENetDeinitialize();

		[DllImport(LIB, EntryPoint = "enet_initialize")]
		internal static extern int ENetInitialize();

		[DllImport(LIB, EntryPoint = "enet_host_create")]
		internal static extern IntPtr ENetHostCreate(
				ref ENetAddress address, uint peerLimit, uint channelLimit, uint incomingBandwidth,
				uint outgoingBandwidth);

		[DllImport(LIB, EntryPoint = "enet_host_create")]
		internal static extern IntPtr ENetHostCreate(
				IntPtr address, uint peerLimit, uint channelLimit, uint incomingBandwidth,
				uint outgoingBandwidth);

		[DllImport(LIB, EntryPoint = "enet_host_destroy")]
		internal static extern void ENetHostDestroy(IntPtr host);

		[DllImport(LIB, EntryPoint = "enet_host_connect")]
		internal static extern IntPtr ENetHostConnect(
				IntPtr host, ref ENetAddress address, uint channelCount, uint data);

		[DllImport(LIB, EntryPoint = "enet_host_broadcast")]
		internal static extern void ENetHostBroadcast(IntPtr host, byte channelID, IntPtr packet);

		[DllImport(LIB, EntryPoint = "enet_host_compress")]
		internal static extern void ENetHostCompress(IntPtr host, IntPtr compressor);

		[DllImport(LIB, EntryPoint = "enet_host_compress_with_range_coder")]
		internal static extern int ENetHostCompressWithRangeCoder(IntPtr host);

		[DllImport(LIB, EntryPoint = "enet_host_channel_limit")]
		internal static extern void ENetHostChannelLimit(IntPtr host, uint channelLimit);

		[DllImport(LIB, EntryPoint = "enet_host_bandwidth_limit")]
		internal static extern void ENetHostBandwidthLimit(
				IntPtr host, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport(LIB, EntryPoint = "enet_host_flush")]
		internal static extern void ENetHostFlush(IntPtr host);

		[DllImport(LIB, EntryPoint = "enet_host_check_events")]
		internal static extern int ENetHostCheckEvents(IntPtr host, ENetEvent ev);

		[DllImport(LIB, EntryPoint = "enet_host_service")]
		internal static extern int ENetHostService(IntPtr host, ENetEvent ev, uint timeout);

		[DllImport(LIB, EntryPoint = "enet_time_get")]
		internal static extern uint ENetTimeGet();

		[DllImport(LIB, EntryPoint = "enet_time_set")]
		internal static extern void ENetTimeSet(uint newTimeBase);

		[DllImport(LIB, EntryPoint = "enet_packet_create")]
		internal static extern IntPtr ENetPacketCreate(byte[] data, uint dataLength, PacketFlags flags);

		[DllImport(LIB, EntryPoint = "enet_packet_destroy")]
		internal static extern void ENetPacketDestroy(IntPtr packet);

		[DllImport(LIB, EntryPoint = "enet_packet_resize")]
		internal static extern int ENetPacketResize(IntPtr packet, uint dataLength);

		[DllImport(LIB, EntryPoint = "enet_peer_throttle_configure")]
		internal static extern void ENetPeerThrottleConfigure(
				IntPtr peer, uint interval, uint acceleration, uint deceleration);

		[DllImport(LIB, EntryPoint = "enet_peer_send")]
		internal static extern int ENetPeerSend(IntPtr peer, byte channelID, IntPtr packet);

		[DllImport(LIB, EntryPoint = "enet_peer_receive")]
		internal static extern IntPtr ENetPeerReceive(IntPtr peer, out byte channelID);

		[DllImport(LIB, EntryPoint = "enet_peer_reset")]
		internal static extern void ENetPeerReset(IntPtr peer);

		[DllImport(LIB, EntryPoint = "enet_peer_ping")]
		internal static extern void ENetPeerPing(IntPtr peer);

		[DllImport(LIB, EntryPoint = "enet_peer_disconnect_now")]
		internal static extern void ENetPeerDisconnectNow(IntPtr peer, uint data);

		[DllImport(LIB, EntryPoint = "enet_peer_disconnect")]
		internal static extern void ENetPeerDisconnect(IntPtr peer, uint data);

		[DllImport(LIB, EntryPoint = "enet_peer_disconnect_later")]
		internal static extern void ENetPeerDisconnectLater(IntPtr peer, uint data);
	}
}