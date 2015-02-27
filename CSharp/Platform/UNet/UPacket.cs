using System;
using System.Runtime.InteropServices;
using Common.Logger;
using Common.Network;

namespace UNet
{
	internal sealed class UPacket: IDisposable
	{
		private IntPtr packet;

		public UPacket(IntPtr packet)
		{
			this.packet = packet;
		}

		public UPacket(byte[] data, PacketFlags flags = PacketFlags.None)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.packet = NativeMethods.EnetPacketCreate(data, (uint) data.Length, flags);
			if (this.packet == IntPtr.Zero)
			{
				throw new UException("Packet creation call failed");
			}
		}

		~UPacket()
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

		public byte[] Bytes
		{
			get
			{
				ENetPacket enetPacket = this.Struct;
				var bytes = new byte[(long)enetPacket.DataLength];
				Marshal.Copy(enetPacket.Data, bytes, 0, (int) enetPacket.DataLength);
				return bytes;
			}
		}
	}
}