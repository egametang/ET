using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common.Logger;

namespace TNet
{
	public class TSocket
	{
		private readonly IPoller poller;
		private Socket socket;
		private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
		private readonly SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();

		public TSocket(IPoller poller)
		{
			this.poller = poller;
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.innArgs.Completed += this.OnComplete;
			this.outArgs.Completed += this.OnComplete;
		}

		public IPoller Poller
		{
			get
			{
				return this.poller;
			}
		}

		public string RemoteAddress
		{
			get
			{
				return ((IPEndPoint) this.socket.RemoteEndPoint).Address + ":" +
				       ((IPEndPoint) this.socket.RemoteEndPoint).Port;
			}
		}

		public Socket Socket
		{
			get
			{
				return this.socket;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.socket == null)
			{
				return;
			}

			if (disposing)
			{
				this.socket.Dispose();
			}

			this.socket = null;
		}

		~TSocket()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
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
			this.outArgs.UserToken = tcs;
			this.outArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
			if (!this.socket.ConnectAsync(this.outArgs))
			{
				OnConnectComplete(this.outArgs);
			}
			return tcs.Task;
		}

		private static void OnConnectComplete(SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<bool>) e.UserToken;
			e.UserToken = null;
			if (e.SocketError != SocketError.Success)
			{
				tcs.SetException(new Exception(string.Format("socket error: {0}", e.SocketError)));
				return;
			}
			tcs.SetResult(true);
		}

		public Task<bool> AcceptAsync(TSocket accpetSocket)
		{
			var tcs = new TaskCompletionSource<bool>();
			this.innArgs.UserToken = tcs;
			this.innArgs.AcceptSocket = accpetSocket.socket;
			if (!this.socket.AcceptAsync(this.innArgs))
			{
				OnAcceptComplete(this.innArgs);
			}
			return tcs.Task;
		}

		private static void OnAcceptComplete(SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<bool>) e.UserToken;
			e.UserToken = null;
			if (e.SocketError != SocketError.Success)
			{
				tcs.SetException(new Exception(string.Format("socket error: {0}", e.SocketError)));
				return;
			}
			tcs.SetResult(true);
		}

		public Task<int> RecvAsync(byte[] buffer, int offset, int count)
		{
			var tcs = new TaskCompletionSource<int>();
			this.innArgs.UserToken = tcs;
			this.innArgs.SetBuffer(buffer, offset, count);
			if (!this.socket.ReceiveAsync(this.innArgs))
			{
				OnRecvComplete(this.innArgs);
			}
			return tcs.Task;
		}

		private static void OnRecvComplete(SocketAsyncEventArgs e)
		{
			Log.Debug("OnRecvComplete: " + e.BytesTransferred);
			var tcs = (TaskCompletionSource<int>) e.UserToken;
			e.UserToken = null;
			if (e.SocketError != SocketError.Success)
			{
				tcs.SetException(new Exception(string.Format("socket error: {0}", e.SocketError)));
				return;
			}
			tcs.SetResult(e.BytesTransferred);
		}

		public Task<int> SendAsync(byte[] buffer, int offset, int count)
		{
			var tcs = new TaskCompletionSource<int>();
			this.outArgs.UserToken = tcs;
			this.outArgs.SetBuffer(buffer, offset, count);
			if (!this.socket.SendAsync(this.outArgs))
			{
				OnSendComplete(this.outArgs);
			}
			return tcs.Task;
		}

		private static void OnSendComplete(SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<int>) e.UserToken;
			e.UserToken = null;
			if (e.SocketError != SocketError.Success)
			{
				tcs.SetException(new Exception(string.Format("socket error: {0}", e.SocketError)));
				return;
			}
			tcs.SetResult(e.BytesTransferred);
		}

		public Task<bool> DisconnectAsync()
		{
			var tcs = new TaskCompletionSource<bool>();
			this.outArgs.UserToken = tcs;
			if (!this.socket.DisconnectAsync(this.outArgs))
			{
				OnDisconnectComplete(this.outArgs);
			}
			return tcs.Task;
		}

		private static void OnDisconnectComplete(SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<bool>) e.UserToken;
			e.UserToken = null;
			if (e.SocketError != SocketError.Success)
			{
				tcs.SetException(new Exception(string.Format("socket error: {0}", e.SocketError)));
				return;
			}
			tcs.SetResult(true);
		}
	}
}