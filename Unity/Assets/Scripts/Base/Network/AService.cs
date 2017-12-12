using System;
using System.Net;
using System.Threading.Tasks;

namespace Model
{
	public enum NetworkProtocol
	{
		TCP,
		KCP,
	}

	public abstract class AService: IDisposable
	{
		public abstract AChannel GetChannel(long id);

		public abstract Task<AChannel> AcceptChannel();

		public abstract AChannel ConnectChannel(IPEndPoint ipEndPoint);

		public abstract void Remove(long channelId);

		public abstract void Update();

		public abstract void Dispose();
	}
}