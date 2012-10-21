using System;
using System.Runtime.InteropServices;

namespace ENet
{
	public struct Event
	{
		private readonly IntPtr e;

		public Event(IntPtr e)
		{
			this.e = e;
		}

		public ENetEvent Struct
		{
			get
			{
				return (ENetEvent)Marshal.PtrToStructure(this.e, typeof(ENetEvent));
			}
			set
			{
				Marshal.StructureToPtr(value, this.e, false);
			}
		}

		public IntPtr NativePtr
		{
			get
			{
				return e;
			}
		}

		public Packet Packet
		{
			get
			{
				return new Packet(this.Struct.packet);
			}
		}

		public Peer Peer
		{
			get
			{
				return new Peer(this.Struct.peer);
			}
		}

		public EventType Type
		{
			get
			{
				return this.Struct.type;
			}
		}
	}
}