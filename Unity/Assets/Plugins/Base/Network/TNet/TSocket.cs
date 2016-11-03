using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Base
{
	/// <summary>
	/// 封装Socket,将回调push到主线程处理
	/// </summary>
	public sealed class TSocket: IDisposable
	{
		private readonly TPoller poller;
		private Socket socket;
		private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
		private readonly SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();

		public Action<SocketError> OnConn;
		public Action<int, SocketError> OnRecv;
		public Action<int, SocketError> OnSend;
		public Action<SocketError> OnDisconnect;

		public TSocket(TPoller poller)
		{
			this.poller = poller;
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.innArgs.Completed += this.OnComplete;
			this.outArgs.Completed += this.OnComplete;
		}

		public TSocket(TPoller poller, string host, int port): this(poller)
		{
			try
			{
				this.Bind(host, port);
				this.Listen(100);
			}
			catch (Exception e)
			{
				throw new Exception($"socket bind error: {host} {port}", e);
			}
		}
		
		public Socket Socket
		{
			get
			{
				return this.socket;
			}
		}

		public string RemoteAddress
		{
			get
			{
				IPEndPoint ipEndPoint = (IPEndPoint)this.socket.RemoteEndPoint;
				return ipEndPoint.Address + ":" + ipEndPoint.Port;
			}
		}

		public void Dispose()
		{
			if (this.socket == null)
			{
				return;
			}
			
			this.socket.Close();
			this.innArgs.Dispose();
			this.outArgs.Dispose();
			this.socket = null;
		}

		private void Bind(string host, int port)
		{
			this.socket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
		}

		private void Listen(int backlog)
		{
			this.socket.Listen(backlog);
		}

		public Task<bool> AcceptAsync(TSocket accpetSocket)
		{
			if (this.socket == null)
			{
				throw new Exception($"TSocket已经被Dispose,不能接收连接!");
			}
			var tcs = new TaskCompletionSource<bool>();
			this.innArgs.UserToken = tcs;
			this.innArgs.AcceptSocket = accpetSocket.socket;
			if (!this.socket.AcceptAsync(this.innArgs))
			{
				OnAcceptComplete(this.innArgs);
			}
			return tcs.Task;
		}

		private void OnAcceptComplete(SocketAsyncEventArgs e)
		{
			if (this.socket == null)
			{
				return;
			}
			var tcs = (TaskCompletionSource<bool>)e.UserToken;
			e.UserToken = null;
			if (e.SocketError != SocketError.Success)
			{
				tcs.SetException(new Exception($"socket error: {e.SocketError}"));
				return;
			}
			tcs.SetResult(true);
		}

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			Action action;
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Connect:
					action = () => OnConnectComplete(e);
					break;
				case SocketAsyncOperation.Receive:
					action = () => OnRecvComplete(e);
					break;
				case SocketAsyncOperation.Send:
					action = () => OnSendComplete(e);
					break;
				case SocketAsyncOperation.Disconnect:
					action = () => OnDisconnectComplete(e);
					break;
				case SocketAsyncOperation.Accept:
					action = () => OnAcceptComplete(e);
					break;
				default:
					throw new Exception($"socket error: {e.LastOperation}");
			}

			// 回调到主线程处理
			this.poller.Add(action);
		}

		public bool ConnectAsync(string host, int port)
		{
			this.outArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
			if (this.socket.ConnectAsync(this.outArgs))
			{
				return true;
			}
			OnConnectComplete(this.outArgs);
			return false;
		}

		private void OnConnectComplete(SocketAsyncEventArgs e)
		{
			if (this.socket == null)
			{
				return;
			}
			if (this.OnConn == null)
			{
				return;
			}
			this.OnConn(e.SocketError);
		}

		public bool RecvAsync(byte[] buffer, int offset, int count)
		{
			try
			{
				this.innArgs.SetBuffer(buffer, offset, count);
			}
			catch (Exception e)
			{
				throw new Exception($"socket set buffer error: {buffer.Length}, {offset}, {count}", e);
			}
			if (this.socket.ReceiveAsync(this.innArgs))
			{
				return true;
			}
			OnRecvComplete(this.innArgs);
			return false;
		}

		private void OnRecvComplete(SocketAsyncEventArgs e)
		{
			if (this.socket == null)
			{
				return;
			}
			if (this.OnRecv == null)
			{
				return;
			}
			this.OnRecv(e.BytesTransferred, e.SocketError);
		}

		public bool SendAsync(byte[] buffer, int offset, int count)
		{
			try
			{
				this.outArgs.SetBuffer(buffer, offset, count);
			}
			catch (Exception e)
			{
				throw new Exception($"socket set buffer error: {buffer.Length}, {offset}, {count}", e);
			}
			if (this.socket.SendAsync(this.outArgs))
			{
				return true;
			}
			OnSendComplete(this.outArgs);
			return false;
		}

		private void OnSendComplete(SocketAsyncEventArgs e)
		{
			if (this.socket == null)
			{
				return;
			}
			if (this.OnSend == null)
			{
				return;
			}
			this.OnSend(e.BytesTransferred, e.SocketError);
		}

		private void OnDisconnectComplete(SocketAsyncEventArgs e)
		{
			if (this.socket == null)
			{
				return;
			}
			if (this.OnDisconnect == null)
			{
				return;
			}
			this.OnDisconnect(e.SocketError);
		}
	}
}