using System;
using System.Threading.Tasks;
using NetMQ;

namespace Zmq
{
	public class ZmqSocket
	{
		private readonly NetMQSocket socket;

		public ZmqSocket(NetMQSocket socket)
		{
			this.socket = socket;
		}

		private EventHandler<NetMQSocketEventArgs> SendHandler { get; set; }

		private EventHandler<NetMQSocketEventArgs> RecvHandler { get; set; }

	    public Task<byte[]> Recv()
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

		public Task<bool> Send(byte[] bytes)
		{
			var tcs = new TaskCompletionSource<bool>();

			this.SendHandler = (sender, args) =>
			{
				args.Socket.SendReady -= this.SendHandler;
				tcs.TrySetResult(true);
			};
			this.socket.SendReady += this.SendHandler;
			this.socket.Send(bytes, bytes.Length, true);
			return tcs.Task;
		}
    }
}
