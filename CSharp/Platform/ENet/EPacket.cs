using System;
using System.Runtime.InteropServices;

namespace ENet
{
	internal sealed class EPacket: IDisposable
	{
		private IntPtr packet;

		public EPacket(IntPtr packet)
		{
			this.packet = packet;
		}

		public EPacket(byte[] data, PacketFlags flags = PacketFlags.None)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.packet = NativeMethods.EnetPacketCreate(data, (uint) data.Length, flags);
			if (this.packet == IntPtr.Zero)
			{
				throw new EException("Packet creation call failed");
			}
		}

		~EPacket()
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

			NativeMethods.EnetPacketDestroy(this.packet);
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

		public IntPtr PacketPtr
		{
			get
			{
				return this.packet;
			}
			set
			{
				this.packet = value;
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
				return this.Struct.DataLength;
			}
		}

		public byte[] Bytes
		{
			get
			{
				var enetPacket = this.Struct;
				var bytes = new byte[enetPacket.DataLength];
				Marshal.Copy(enetPacket.Data, bytes, 0, (int) enetPacket.DataLength);
				return bytes;
			}
		}
	}
}