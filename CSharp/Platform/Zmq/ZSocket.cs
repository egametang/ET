using System;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace Zmq
{
	public class ZSocket: IDisposable
	{
		private readonly ZmqSocket socket;

		public ZSocket(ZmqSocket socket)
		{
			this.socket = socket;
			this.socket.ReceiveReady += delegate { };
		}

		public void Dispose()
		{
			this.socket.Dispose();
		}

		public ZmqSocket ZmqSocket
		{
			get
			{
				return this.socket;
			}
		}

		private EventHandler<SocketEventArgs> SendHandler { get; set; }

		private EventHandler<SocketEventArgs> RecvHandler { get; set; }

	    public Task<string> RecvAsync()
	    {
			var tcs = new TaskCompletionSource<string>();

			this.RecvHandler = (sender, args) =>
		    {
				args.Socket.ReceiveReady -= this.RecvHandler;
				tcs.TrySetResult(args.Socket.Receive(Encoding.Unicode));
		    };

			this.socket.ReceiveReady += this.RecvHandler;
		    return tcs.Task;
	    }

		public Task<bool> SendAsync(string str)
		{
			var tcs = new TaskCompletionSource<bool>();

			this.SendHandler = (sender, args) =>
			{
				args.Socket.SendReady -= this.SendHandler;
				this.socket.Send(str, Encoding.Unicode, TimeSpan.FromMilliseconds(0));
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
