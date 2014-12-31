using System;
using System.Runtime.InteropServices;

namespace UNet
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
	
	[StructLayout(LayoutKind.Sequential)]
	public struct ENetAddress
	{
		public uint Host;
		public ushort Port;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetCallbacks
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr MallocCb(IntPtr size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void FreeCb(IntPtr memory);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void NoMemoryCb();

		private IntPtr malloc;
		private IntPtr free;
		private IntPtr no_memory;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ENetEvent
	{
		public EventType Type;
		public IntPtr Peer;
		public byte ChannelID;
		public uint Data;
		public IntPtr Packet;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetHost
	{
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ENetListNode
	{
		public IntPtr Next;
		public IntPtr Previous;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ENetPacketFreeCallback(ref ENetPacket param0);

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetPacket
	{
		public uint ReferenceCount;
		public uint Flags;
		public IntPtr Data;
		public uint DataLength;
		public ENetPacketFreeCallback FreeCallback;
		public IntPtr UserData;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ENetPeer
	{
		public ENetListNode DispatchList;
		public IntPtr Host;
		public ushort OutgoingPeerID;
		public ushort IncomingPeerID;
		public uint ConnectID;
		public byte OutgoingSessionID;
		public byte IncomingSessionID;
		public ENetAddress Address;
		public IntPtr Data;
		public PeerState State;
	}
}