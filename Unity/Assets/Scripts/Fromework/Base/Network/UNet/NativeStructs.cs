using System;
using System.Runtime.InteropServices;

namespace Model
{
	internal enum EventType
	{
		None = 0,
		Connect = 1,
		Disconnect = 2,
		Receive = 3
	}

	internal enum PeerState
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
	internal struct ENetAddress
	{
		public uint Host;
		public ushort Port;
	}

	// ENetEvent
	[StructLayout(LayoutKind.Sequential)]
	internal struct ENetEvent
	{
		public EventType Type;
		public IntPtr Peer;
		public byte ChannelID;
		public uint Data;
		public IntPtr Packet;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class ENetListNode
	{
		public IntPtr Next;
		public IntPtr Previous;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct ENetPacket
	{
		public IntPtr ReferenceCount; // size_t
		public uint Flags;
		public IntPtr Data;
		public IntPtr DataLength; // size_t
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct ENetPeer
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
	}
}