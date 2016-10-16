using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Base
{
	public static class NativeMethods
	{
		private const string LIB = "ENet";

		public const int ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT = 255;
		public const int ENET_PROTOCOL_MAXIMUM_PEER_ID = 0xfff;

		[DllImport(LIB, EntryPoint = "enet_address_set_host", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int ENetAddressSetHost(ref ENetAddress address, string hostName);

		[DllImport(LIB, EntryPoint = "enet_address_get_host", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int ENetAddressGetHost(ref ENetAddress address, StringBuilder hostName, uint nameLength);

		[DllImport(LIB, EntryPoint = "enet_address_get_host_ip", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int ENetAddressGetHostIp(ref ENetAddress address, StringBuilder hostIp, uint ipLength);

		[DllImport(LIB, EntryPoint = "enet_deinitialize", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetDeinitialize();

		[DllImport(LIB, EntryPoint = "enet_initialize", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int ENetInitialize();

		[DllImport(LIB, EntryPoint = "enet_host_create", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr ENetHostCreate(
				ref ENetAddress address, uint peerLimit, uint channelLimit, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport(LIB, EntryPoint = "enet_host_create", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr ENetHostCreate(IntPtr address, uint peerLimit, uint channelLimit, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport(LIB, EntryPoint = "enet_host_destroy", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetHostDestroy(IntPtr host);

		[DllImport(LIB, EntryPoint = "enet_host_connect", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr ENetHostConnect(IntPtr host, ref ENetAddress address, uint channelCount, uint data);

		[DllImport(LIB, EntryPoint = "enet_host_broadcast", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetHostBroadcast(IntPtr host, byte channelID, IntPtr packet);

		[DllImport(LIB, EntryPoint = "enet_host_compress", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetHostCompress(IntPtr host, IntPtr compressor);

		[DllImport(LIB, EntryPoint = "enet_host_compress_with_range_coder", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int ENetHostCompressWithRangeCoder(IntPtr host);

		[DllImport(LIB, EntryPoint = "enet_host_channel_limit", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetHostChannelLimit(IntPtr host, uint channelLimit);

		[DllImport(LIB, EntryPoint = "enet_host_bandwidth_limit", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetHostBandwidthLimit(IntPtr host, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport(LIB, EntryPoint = "enet_host_flush", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetHostFlush(IntPtr host);

		[DllImport(LIB, EntryPoint = "enet_host_check_events", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int ENetHostCheckEvents(IntPtr host, ENetEvent ev);

		[DllImport(LIB, EntryPoint = "enet_host_service", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int ENetHostService(IntPtr host, ENetEvent ev, uint timeout);

		[DllImport(LIB, EntryPoint = "enet_time_get", CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint ENetTimeGet();

		[DllImport(LIB, EntryPoint = "enet_time_set", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetTimeSet(uint newTimeBase);

		[DllImport(LIB, EntryPoint = "enet_packet_create", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr ENetPacketCreate(byte[] data, uint dataLength, PacketFlags flags);

		[DllImport(LIB, EntryPoint = "enet_packet_destroy", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetPacketDestroy(IntPtr packet);

		[DllImport(LIB, EntryPoint = "enet_packet_resize", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int ENetPacketResize(IntPtr packet, uint dataLength);

		[DllImport(LIB, EntryPoint = "enet_peer_throttle_configure", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetPeerThrottleConfigure(IntPtr peer, uint interval, uint acceleration, uint deceleration);

		[DllImport(LIB, EntryPoint = "enet_peer_send", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int ENetPeerSend(IntPtr peer, byte channelID, IntPtr packet);

		[DllImport(LIB, EntryPoint = "enet_peer_receive", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr ENetPeerReceive(IntPtr peer, out byte channelID);

		[DllImport(LIB, EntryPoint = "enet_peer_reset", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetPeerReset(IntPtr peer);

		[DllImport(LIB, EntryPoint = "enet_peer_ping", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetPeerPing(IntPtr peer);

		[DllImport(LIB, EntryPoint = "enet_peer_disconnect_now", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetPeerDisconnectNow(IntPtr peer, uint data);

		[DllImport(LIB, EntryPoint = "enet_peer_disconnect", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetPeerDisconnect(IntPtr peer, uint data);

		[DllImport(LIB, EntryPoint = "enet_peer_disconnect_later", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ENetPeerDisconnectLater(IntPtr peer, uint data);
	}
}