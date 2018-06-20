using System;
using System.Collections.Generic;
using System.Net;

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

	public class UserTokenInfo
	{
		public long InstanceId;
	}

	public abstract class AChannel: ComponentWithId
	{
		public ChannelType ChannelType { get; }

		protected AService service;

		public IPEndPoint RemoteAddress { get; protected set; }

		private Action<AChannel, int> errorCallback;

		public event Action<AChannel, int> ErrorCallback
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
		
		private Action<Packet> readCallback;

		public event Action<Packet> ReadCallback
		{
			add
			{
				this.readCallback += value;
			}
			remove
			{
				this.readCallback -= value;
			}
		}
		
		protected void OnRead(Packet packet)
		{
			this.readCallback.Invoke(packet);
		}

		protected void OnError(int e)
		{
			this.errorCallback?.Invoke(this, e);
		}

		protected AChannel(AService service, ChannelType channelType)
		{
			this.Id = IdGenerater.GenerateId();
			this.ChannelType = channelType;
			this.service = service;
		}

		public abstract void Start();

		/// <summary>
		/// 发送消息
		/// </summary>
		public abstract void Send(byte[] buffer, int index, int length);

		public abstract void Send(List<byte[]> buffers);
		
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