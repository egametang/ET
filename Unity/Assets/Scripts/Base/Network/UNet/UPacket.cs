using System;
using System.Runtime.InteropServices;

namespace Model
{
	internal sealed class UPacket: IDisposable
	{
		public UPacket(IntPtr packet)
		{
			this.PacketPtr = packet;
		}

		public UPacket(byte[] data, PacketFlags flags = PacketFlags.None)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}
			this.PacketPtr = NativeMethods.enet_packet_create(data, (uint) data.Length, flags);
			if (this.PacketPtr == IntPtr.Zero)
			{
				throw new Exception("Packet creation call failed");
			}
		}

		~UPacket()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (this.PacketPtr == IntPtr.Zero)
			{
				return;
			}

			NativeMethods.enet_packet_destroy(this.PacketPtr);
			this.PacketPtr = IntPtr.Zero;
		}

		private ENetPacket Struct
		{
			get
			{
				return (ENetPacket) Marshal.PtrToStructure(this.PacketPtr, typeof (ENetPacket));
			}
			set
			{
				Marshal.StructureToPtr(value, this.PacketPtr, false);
			}
		}

		public IntPtr PacketPtr { get; set; }

		public byte[] Bytes
		{
			get
			{
				ENetPacket enetPacket = this.Struct;
				var bytes = new byte[(long) enetPacket.DataLength];
				Marshal.Copy(enetPacket.Data, bytes, 0, (int) enetPacket.DataLength);
				return bytes;
			}
		}
	}
}