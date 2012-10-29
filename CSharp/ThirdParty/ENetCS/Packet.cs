using System;
using System.Runtime.InteropServices;

namespace ENet
{
	public sealed class Packet : IDisposable
	{
		private Host host;
		private IntPtr packet;

		public Packet(Host host, IntPtr packet)
		{
			this.host = host;
			this.packet = packet;
		}

		public Packet(string data, PacketFlags flags = PacketFlags.None)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.packet = Native.enet_packet_create(data, (uint) data.Length, flags);
			if (this.packet == IntPtr.Zero)
			{
				throw new ENetException(0, "Packet creation call failed.");
			}
		}

		~Packet()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.packet == IntPtr.Zero)
			{
				return;
			}

			if (disposing)
			{
				Native.enet_packet_destroy(this.packet);
			}

			this.packet = IntPtr.Zero;
		}

		private ENetPacket Struct
		{
			get
			{
				return (ENetPacket) Marshal.PtrToStructure(this.packet, typeof (ENetPacket));
			}
			set
			{
				Marshal.StructureToPtr(value, this.packet, false);
			}
		}

		public IntPtr NativePtr
		{
			get
			{
				return this.packet;
			}
		}

		public uint Length
		{
			get
			{
				if (this.packet == IntPtr.Zero)
				{
					return 0;
				}
				return this.Struct.dataLength;
			}
		}

		public string Data
		{
			get
			{
				if (this.packet == IntPtr.Zero)
				{
					return "";
				}
				ENetPacket pkt = this.Struct;
				if (pkt.data == IntPtr.Zero)
				{
					return "";
				}
				return Marshal.PtrToStringAuto(pkt.data, (int) pkt.dataLength);
			}
		}
	}
}