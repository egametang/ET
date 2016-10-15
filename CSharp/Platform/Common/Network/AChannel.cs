using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;

namespace Common.Network
{
	[Flags]
	public enum PacketFlags
	{
		None = 0,
		Reliable = 1 << 0,
		Unsequenced = 1 << 1,
		NoAllocate = 1 << 2
	}

	public abstract class AChannel: Entity<AChannel>, IDisposable
	{
		protected IService service;
		protected Action<AChannel> onDispose = channel => { };

		protected AChannel(IService service)
		{
			this.service = service;
		}

		public abstract Task<bool> ConnectAsync();

		/// <summary>
		/// 发送消息
		/// </summary>
		public abstract void SendAsync(
				byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable);

		public abstract void SendAsync(
				List<byte[]> buffers, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable);

		/// <summary>
		/// 接收消息
		/// </summary>
		public abstract Task<byte[]> RecvAsync();

		public abstract Task<bool> DisconnnectAsync();

		public abstract string RemoteAddress { get; }

		public event Action<AChannel> OnDispose
		{
			add
			{
				this.onDispose += value;
			}
			remove
			{
				this.onDispose -= value;
			}
		}

		public abstract void Dispose();
	}
}