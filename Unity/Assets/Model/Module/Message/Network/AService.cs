using System;
using System.Net;

namespace ETModel
{
	public enum NetworkProtocol
	{
		KCP,
		TCP,
		WebSocket,
	}

	public abstract class AService: Component
	{
		public abstract AChannel GetChannel(long id);

		private Action<AChannel> acceptCallback;

		public event Action<AChannel> AcceptCallback
		{
			add
			{
				this.acceptCallback += value;
			}
			remove
			{
				this.acceptCallback -= value;
			}
		}
		
		protected void OnAccept(AChannel channel)
		{
			this.acceptCallback.Invoke(channel);
		}

		public abstract AChannel ConnectChannel(IPEndPoint ipEndPoint);
		
		public abstract AChannel ConnectChannel(string address);

		public abstract void Remove(long channelId);

		public abstract void Update();
	}
}