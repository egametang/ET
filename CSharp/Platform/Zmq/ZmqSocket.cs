using System;
using System.Threading.Tasks;
using NetMQ;

namespace Zmq
{
	public class ZmqSocket: IDisposable
	{
		private ZmqPoller poller;
		private readonly NetMQSocket socket;

		public ZmqSocket(ZmqPoller poller, NetMQSocket socket)
		{
			this.poller = poller;
			this.socket = socket;
			poller.AddSocket(this.socket);
		}

		public void Dispose()
		{
			this.poller.RemoveSocket(this.socket);
			this.socket.Dispose();
		}

		private EventHandler<NetMQSocketEventArgs> SendHandler { get; set; }

		private EventHandler<NetMQSocketEventArgs> RecvHandler { get; set; }

	    public Task<byte[]> RecvAsync()
	    {
			var tcs = new TaskCompletionSource<byte[]>();

			this.RecvHandler = (sender, args) =>
		    {
				bool hasMore = false;
				args.Socket.ReceiveReady -= this.RecvHandler;
				tcs.TrySetResult(args.Socket.Receive(true, out hasMore));
		    };

			this.socket.ReceiveReady += this.RecvHandler;
		    return tcs.Task;
	    }

		public Task<bool> SendAsync(byte[] bytes)
		{
			var tcs = new TaskCompletionSource<bool>();

			this.SendHandler = (sender, args) =>
			{
				args.Socket.SendReady -= this.SendHandler;
				this.socket.Send(bytes, bytes.Length, true);
				tcs.TrySetResult(true);
			};
			this.socket.SendReady += this.SendHandler;
			return tcs.Task;
		}

		public void Connect(string address)
		{
			this.socket.Connect(address);
		}

		public void Bind(string address)
		{
			this.socket.Bind(address);
		}
	}
}
