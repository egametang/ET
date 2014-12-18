using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TNet
{
	public class TSocket
	{
		private IPoller poller;
		private readonly Socket socket;
		private readonly SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();

		public TSocket(IPoller poller)
		{
			this.poller = poller;
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.socketAsyncEventArgs.Completed += this.OnComplete;
		}

		public Socket Socket
		{
			get
			{
				return this.socket;
			}
		}

		public void Dispose()
		{
			if (this.poller == null)
			{
				return;
			}
			this.socket.Dispose();
			this.poller = null;
		}

		public void Bind(string host, int port)
		{
			this.socket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
		}

		public void Listen(int backlog)
		{
			this.socket.Listen(backlog);
		}

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			Action action;
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					action = () => OnAcceptComplete(e);
					e.AcceptSocket = null;
					break;
				case SocketAsyncOperation.Connect:
					action = () => OnConnectComplete(e);
					break;
				case SocketAsyncOperation.Disconnect:
					action = () => OnDisconnectComplete(e);
					break;
				case SocketAsyncOperation.Receive:
					action = () => OnRecvComplete(e);
					break;
				case SocketAsyncOperation.Send:
					action = () => OnSendComplete(e);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			this.poller.Add(action);
		}

		public Task<bool> ConnectAsync(string host, int port)
		{
			var tcs = new TaskCompletionSource<bool>();
			this.socketAsyncEventArgs.UserToken = tcs;
			this.socketAsyncEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
			if (!this.socket.ConnectAsync(this.socketAsyncEventArgs))
			{
				this.poller.Add(() => { OnConnectComplete(this.socketAsyncEventArgs); });
			}
			return tcs.Task;
		}

		private static void OnConnectComplete(SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<bool>)e.UserToken;
			tcs.SetResult(true);
		}

		public Task<bool> AcceptAsync(TSocket accpetSocket)
		{
			var tcs = new TaskCompletionSource<bool>();
			this.socketAsyncEventArgs.UserToken = tcs;
			this.socketAsyncEventArgs.AcceptSocket = accpetSocket.socket;
			if (!this.socket.AcceptAsync(this.socketAsyncEventArgs))
			{
				Action action = () => OnAcceptComplete(this.socketAsyncEventArgs);
				this.poller.Add(action);
			}
			return tcs.Task;
		}

		private static void OnAcceptComplete(SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<bool>)e.UserToken;
			tcs.SetResult(true);
		}

		public Task<int> RecvAsync(byte[] buffer, int offset, int count)
		{
			var tcs = new TaskCompletionSource<int>();
			this.socketAsyncEventArgs.UserToken = tcs;
			this.socketAsyncEventArgs.SetBuffer(buffer, offset, count);
			if (!this.socket.ReceiveAsync(this.socketAsyncEventArgs))
			{
				Action action = () => OnRecvComplete(this.socketAsyncEventArgs);
				this.poller.Add(action);
			}
			return tcs.Task;
		}

		private static void OnRecvComplete(SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<int>)e.UserToken;
			tcs.SetResult(e.BytesTransferred);
		}

		public Task<int> SendAsync(byte[] buffer, int offset, int count)
		{
			var tcs = new TaskCompletionSource<int>();
			this.socketAsyncEventArgs.UserToken = tcs;
			this.socketAsyncEventArgs.SetBuffer(buffer, offset, count);
			if (!this.socket.SendAsync(this.socketAsyncEventArgs))
			{
				Action action = () => OnSendComplete(this.socketAsyncEventArgs);
				this.poller.Add(action);
			}
			return tcs.Task;
		}

		private static void OnSendComplete(SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<int>)e.UserToken;
			tcs.SetResult(e.BytesTransferred);
		}

		public Task<bool> DisconnectAsync()
		{
			var tcs = new TaskCompletionSource<bool>();
			this.socketAsyncEventArgs.UserToken = tcs;
			if (!this.socket.DisconnectAsync(this.socketAsyncEventArgs))
			{
				Action action = () => OnDisconnectComplete(this.socketAsyncEventArgs);
				this.poller.Add(action);
			}
			return tcs.Task;
		}

		private static void OnDisconnectComplete(SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<bool>)e.UserToken;
			tcs.SetResult(true);
		}
	}
}
