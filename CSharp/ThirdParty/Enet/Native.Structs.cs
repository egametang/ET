#region License

/*
ENet for C#
Copyright (c) 2011 James F. Bellinger <jfb@zer7.com>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

#endregion

using System;
using System.Runtime.InteropServices;

namespace ENet
{
	public static unsafe partial class Native
	{
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
		public struct ENetEvent
		{
			public readonly EventType type;
			public readonly ENetPeer* peer;
			public readonly byte channelID;
			public readonly uint data;
			public readonly ENetPacket* packet;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ENetHost
		{
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ENetListNode
		{
			public readonly ENetListNode* next;
			public readonly ENetListNode* previous;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ENetPacket
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void freeCallback_cb(IntPtr packet);

			public IntPtr referenceCount;
			public readonly PacketFlags flags;
			public readonly void* data;
			public IntPtr dataLength;
			public IntPtr freeCallback;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ENetPeer
		{
			public ENetListNode dispatchList;
			public readonly ENetHost* host;
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
}