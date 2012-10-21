using System;
using System.Runtime.InteropServices;

namespace ENet
{
	public sealed class Packet : IDisposable
	{
		private IntPtr packet;

		public Packet(IntPtr packet)
		{
			if (packet == IntPtr.Zero)
			{
				throw new InvalidOperationException("No native packet.");
			}
			this.packet = packet;
		}

		public Packet(string data, PacketFlags flags = PacketFlags.None)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.packet = Native.enet_packet_create(data, (uint)data.Length, flags);
			if (this.packet == IntPtr.Zero)
			{
				throw new ENetException(0, "Packet creation call failed.");
			}
		}

		~Packet()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.packet == IntPtr.Zero)
			{
				return;
			}
			Native.enet_packet_destroy(this.packet);
			this.packet = IntPtr.Zero;
		}

		public ENetPacket Struct
		{
			get
			{
				return (ENetPacket)Marshal.PtrToStructure(this.packet, typeof(ENetPacket));
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
	}
}