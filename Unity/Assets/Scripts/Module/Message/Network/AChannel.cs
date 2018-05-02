using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ETModel
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

	public abstract class AChannel: ComponentWithId
	{
		public ChannelType ChannelType { get; }

		protected AService service;

		public IPEndPoint RemoteAddress { get; protected set; }

		private Action<AChannel, SocketError> errorCallback;

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

		protected void OnError(SocketError e)
		{
			if (this.IsDisposed)
			{
				return;
			}
			this.errorCallback?.Invoke(this, e);
		}


		protected AChannel(AService service, ChannelType channelType)
		{
			this.ChannelType = channelType;
			this.service = service;
		}
		
		/// <summary>
		/// 发送消息
		/// </summary>
		public abstract void Send(byte[] buffer, int index, int length);

		public abstract void Send(List<byte[]> buffers);

		/// <summary>
		/// 接收消息
		/// </summary>
		public abstract Task<Packet> Recv();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.service.Remove(this.Id);
		}
	}
}