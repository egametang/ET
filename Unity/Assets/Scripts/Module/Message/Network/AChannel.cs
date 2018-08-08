using System;
using System.IO;
using System.Net;

namespace ETModel
{
	public enum ChannelType
	{
		Connect,
		Accept,
	}

	public abstract class AChannel: ComponentWithId
	{
		public ChannelType ChannelType { get; }

		protected AService service;

		public abstract MemoryStream Stream { get; }
		
		public int Error { get; set; }

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
			this.Error = e;
			this.errorCallback?.Invoke(this, e);
		}

		protected AChannel(AService service, ChannelType channelType)
		{
			this.Id = IdGenerater.GenerateId();
			this.ChannelType = channelType;
			this.service = service;
		}

		public abstract void Start();
		
		public abstract void Send(MemoryStream stream);
		
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