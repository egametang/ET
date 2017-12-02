using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Model
{
	[Flags]
	public enum PacketFlags
	{
		None = 0,
		Reliable = 1 << 0,
		Unsequenced = 1 << 1,
		NoAllocate = 1 << 2
	}

	public enum ChannelType
	{
		Connect,
		Accept,
	}

	public abstract class AChannel: IDisposable
	{
		public long Id { get; set; }

		public ChannelType ChannelType { get; }

		protected AService service;

		public IPEndPoint RemoteAddress { get; protected set; }

		private event Action<AChannel, SocketError> errorCallback;

		public event Action<AChannel, SocketError> ErrorCallback
		{
			add
			{
				this.errorCallback += value;
			}
			remove
			{
				this.errorCallback -= value;
			}
		}

		protected void OnError(AChannel channel, SocketError e)
		{
			this.errorCallback?.Invoke(channel, e);
		}


		protected AChannel(AService service, ChannelType channelType)
		{
			this.Id = IdGenerater.GenerateId();
			this.ChannelType = channelType;
			this.service = service;
		}
		
		/// <summary>
		/// 发送消息
		/// </summary>
		public abstract void Send(byte[] buffer);

		public abstract void Send(List<byte[]> buffers);

		/// <summary>
		/// 接收消息
		/// </summary>
		public abstract Task<Packet> Recv();

		public virtual void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			long id = this.Id;
			this.Id = 0;

			this.service.Remove(id);
		}
	}
}