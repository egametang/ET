using System;
using System.Threading.Tasks;

namespace Model
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

		public abstract Task<AChannel> AcceptChannel();

		public abstract AChannel ConnectChannel(string host, int port);

		public abstract void Remove(long channelId);

		public abstract void Update();

		public abstract void Dispose();
	}
}