using System;
using System.Net.Sockets;

namespace Base
{
	public enum NetworkProtocol
	{
		TCP,
		UDP
	}

	public abstract class AService: IDisposable
	{
		/// <summary>
		/// 将函数调用加入IService线程
		/// </summary>
		/// <param name="action"></param>
		public abstract void Add(Action action);

		public abstract AChannel GetChannel(long id);

		public abstract AChannel GetChannel(string host, int port);

		public abstract AChannel GetChannel(string address);
		
		public abstract void Remove(long channelId);

		public abstract void Update();

		public Action<long, SocketError> OnError;

		public void OnChannelError(long channelId, SocketError error)
		{
			this.OnError?.Invoke(channelId, error);
			this.Remove(channelId);
		}

		public abstract void Dispose();
	}
}