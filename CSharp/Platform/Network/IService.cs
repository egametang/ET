using System;
using System.Threading.Tasks;

namespace Network
{
	public enum NetworkProtocol
	{
		TCP,
		UDP,
	}

	public interface IService: IDisposable
	{
		/// <summary>
		/// 将函数调用加入IService线程
		/// </summary>
		/// <param name="action"></param>
		void Add(Action action);

		Task<IChannel> GetChannel(string host, int port);

		Task<IChannel> GetChannel();

		void Remove(IChannel channel);

		void RunOnce(int timeout);

		void Run();
	}
}
