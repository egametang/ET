using System;
using System.Net;
using System.Net.Sockets;

namespace Base
{
	/// <summary>
	/// 封装Socket,将回调push到主线程处理
	/// </summary>
	public class TSocket: IDisposable
	{
		private readonly TPoller poller;
		private Socket socket;
		private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
		private readonly SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();

		public Action<SocketError> OnConn;
		public Action<int, SocketError> OnRecv;
		public Action<int, SocketError> OnSend;
		public Action<SocketError> OnDisconnect;
		private string remoteAddress;

		public TSocket(TPoller poller)
		{
			this.poller = poller;
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.innArgs.Completed += this.OnComplete;
			this.outArgs.Completed += this.OnComplete;
		}

		public string RemoteAddress
		{
			get
			{
				return remoteAddress;
			}
		}

		public Socket Socket
		{
			get
			{
				return this.socket;
			}
		}

		protected void Dispose(bool disposing)
		{
			if (this.socket == null)
			{
				return;
			}

			if (disposing)
			{
				this.socket.Close();
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
				default:
					throw new Exception($"socket error: {e.LastOperation}");
			}

			// 回调到主线程处理
			this.poller.Add(action);
		}

		public bool ConnectAsync(string host, int port)
		{
			remoteAddress = $"{host}:{port}";
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
			if (this.OnSend == null)
			{
				return;
			}
			this.OnSend(e.BytesTransferred, e.SocketError);
		}

		private void OnDisconnectComplete(SocketAsyncEventArgs e)
		{
			if (this.OnDisconnect == null)
			{
				return;
			}
			this.OnDisconnect(e.SocketError);
		}
	}
}