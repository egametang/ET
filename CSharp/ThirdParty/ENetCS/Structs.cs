using System;
using System.Runtime.InteropServices;

namespace ENet
{
	public enum EventType
	{
		None = 0,
		Connect = 1,
		Disconnect = 2,
		Receive = 3
	}

	public enum PeerState
	{
		Uninitialized = -1,
		Disconnected = 0,
		Connecting = 1,
		AcknowledgingConnect = 2,
		ConnectionPending = 3,
		ConnectionSucceeded = 4,
		Connected = 5,
		DisconnectLater = 6,
		Disconnecting = 7,
		AcknowledgingDisconnect = 8,
		Zombie = 9
	}

	[Flags]
	public enum PacketFlags
	{
		None = 0,
		Reliable = 1 << 0,
		Unsequenced = 1 << 1,
		NoAllocate = 1 << 2
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetAddress
	{
		public uint host;
		public ushort port;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetCallbacks
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr malloc_cb(IntPtr size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void free_cb(IntPtr memory);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void no_memory_cb();

		public IntPtr malloc, free, no_memory;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetCompressor
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void compress_cb(
				IntPtr context, IntPtr inBuffers, IntPtr inBufferCount, IntPtr inLimit, IntPtr outData, IntPtr outLimit);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void decompress_cb(IntPtr context, IntPtr inData, IntPtr inLimit, IntPtr outData, IntPtr outLimit);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void destroy_cb(IntPtr context);

		public IntPtr context;
		public IntPtr compress, decompress, destroy;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ENetEvent
	{
		public EventType type;
		public IntPtr peer;
		public byte channelID;
		public uint data;
		public IntPtr packet;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetHost
	{
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ENetListNode
	{
		public ENetListNode next;
		public ENetListNode previous;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ENetPacketFreeCallback(ref ENetPacket param0);

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetPacket
	{
		public uint referenceCount;
		public uint flags;
		public IntPtr data;
		public uint dataLength;
		public ENetPacketFreeCallback freeCallback;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetPeer
	{
		public ENetListNode dispatchList;
		public readonly IntPtr host;
		public readonly ushort outgoingPeerID;
		public readonly ushort incomingPeerID;
		public readonly uint connectID;
		public readonly byte outgoingSessionID;
		public readonly byte incomingSessionID;
		public ENetAddress address;
		public IntPtr data;
		public readonly PeerState state;
	}
}