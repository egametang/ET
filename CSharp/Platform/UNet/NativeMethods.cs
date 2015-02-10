using System;
using System.Runtime.InteropServices;
using System.Text;
using Common.Network;

namespace UNet
{
	public static class NativeMethods
	{
		private const string LIB = "ENet.dll";

		public const int ENET_PEER_PACKET_THROTTLE_SCALE = 32;
		public const int ENET_PEER_PACKET_THROTTLE_ACCELERATION = 2;
		public const int ENET_PEER_PACKET_THROTTLE_DECELERATION = 2;
		public const int ENET_PEER_PACKET_THROTTLE_INTERVAL = 5000;
		public const int ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT = 1;
		public const int ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT = 255;
		public const int ENET_PROTOCOL_MAXIMUM_PEER_ID = 0xfff;
		public const uint ENET_VERSION = (1 << 16) | (3 << 8) | (10);
		public const uint ENET_HOST_ANY = 0;
		public const uint ENET_HOST_BROADCAST = 0xffffffff;

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_address_set_host")
		]
		internal static extern int EnetAddressSetHost(ref ENetAddress address, string hostName);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_address_get_host")
		]
		internal static extern int EnetAddressGetHost(
				ref ENetAddress address, StringBuilder hostName, uint nameLength);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl,
				EntryPoint = "enet_address_get_host_ip")]
		internal static extern int EnetAddressGetHostIp(
				ref ENetAddress address, StringBuilder hostIp, uint ipLength);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_deinitialize")]
		internal static extern void EnetDeinitialize();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_initialize")]
		internal static extern int EnetInitialize();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_host_create")]
		internal static extern IntPtr EnetHostCreate(
				ref ENetAddress address, uint peerLimit, uint channelLimit, uint incomingBandwidth,
				uint outgoingBandwidth);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_host_create")]
		internal static extern IntPtr EnetHostCreate(
				IntPtr address, uint peerLimit, uint channelLimit, uint incomingBandwidth,
				uint outgoingBandwidth);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_host_destroy")]
		internal static extern void EnetHostDestroy(IntPtr host);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_host_connect")]
		internal static extern IntPtr EnetHostConnect(
				IntPtr host, ref ENetAddress address, uint channelCount, uint data);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_host_broadcast")]
		internal static extern void EnetHostBroadcast(IntPtr host, byte channelID, IntPtr packet);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_host_compress")]
		internal static extern void EnetHostCompress(IntPtr host, IntPtr compressor);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl,
				EntryPoint = "enet_host_compress_with_range_coder")]
		internal static extern int EnetHostCompressWithRangeCoder(IntPtr host);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl,
				EntryPoint = "enet_host_channel_limit")]
		internal static extern void EnetHostChannelLimit(IntPtr host, uint channelLimit);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl,
				EntryPoint = "enet_host_bandwidth_limit")]
		internal static extern void EnetHostBandwidthLimit(
				IntPtr host, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_host_flush")]
		internal static extern void EnetHostFlush(IntPtr host);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_host_check_events"
				)]
		internal static extern int EnetHostCheckEvents(IntPtr host, ENetEvent ev);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_host_service")]
		internal static extern int EnetHostService(IntPtr host, ENetEvent ev, uint timeout);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_time_get")]
		internal static extern uint EnetTimeGet();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_time_set")]
		internal static extern void EnetTimeSet(uint newTimeBase);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_packet_create")]
		internal static extern IntPtr EnetPacketCreate(byte[] data, uint dataLength, PacketFlags flags);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_packet_destroy")]
		internal static extern void EnetPacketDestroy(IntPtr packet);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_packet_resize")]
		internal static extern int EnetPacketResize(IntPtr packet, uint dataLength);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl,
				EntryPoint = "enet_peer_throttle_configure")]
		internal static extern void EnetPeerThrottleConfigure(
				IntPtr peer, uint interval, uint acceleration, uint deceleration);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_peer_send")]
		internal static extern int EnetPeerSend(IntPtr peer, byte channelID, IntPtr packet);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_peer_receive")]
		internal static extern IntPtr EnetPeerReceive(IntPtr peer, out byte channelID);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_peer_reset")]
		internal static extern void EnetPeerReset(IntPtr peer);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_peer_ping")]
		internal static extern void EnetPeerPing(IntPtr peer);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl,
				EntryPoint = "enet_peer_disconnect_now")]
		internal static extern void EnetPeerDisconnectNow(IntPtr peer, uint data);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enet_peer_disconnect")]
		internal static extern void EnetPeerDisconnect(IntPtr peer, uint data);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl,
				EntryPoint = "enet_peer_disconnect_later")]
		internal static extern void EnetPeerDisconnectLater(IntPtr peer, uint data);
	}
}