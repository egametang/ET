using System;
using System.Collections.Generic;

namespace Base
{
	[Flags]
	public enum PacketFlags
	{
		None = 0,
		Reliable = 1 << 0,
		Unsequenced = 1 << 1,
		NoAllocate = 1 << 2
	}

	public abstract class AChannel: Entity
	{
		protected AService service;

		protected AChannel(AService service)
		{
			this.service = service;
		}

		public abstract void ConnectAsync();

		/// <summary>
		/// 发送消息
		/// </summary>
		public abstract void Send(byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable);

		public abstract void Send(List<byte[]> buffers, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable);

		/// <summary>
		/// 接收消息
		/// </summary>
		public abstract byte[] Recv();

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			long id = this.Id;

			base.Dispose();

			this.service.Remove(id);
		}
	}
}