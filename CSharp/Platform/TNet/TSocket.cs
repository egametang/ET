using System;
using System.Net;
using System.Net.Sockets;

namespace TNet
{
	public class TSocket: IDisposable
	{
		private Socket socket;
		private readonly TPoller poller;
		private readonly SocketAsyncEventArgs innSocketAsyncEventArgs = new SocketAsyncEventArgs();
		private readonly SocketAsyncEventArgs outSocketAsyncEventArgs = new SocketAsyncEventArgs();
		private readonly TBuffer recvBuffer = new TBuffer();
		private readonly TBuffer sendBuffer = new TBuffer();
		public Action RecvAction { get; set; }
		public Action<TSocket> AcceptAction { get; set; }

		public TSocket(TPoller poller)
		{
			this.poller = poller;
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.outSocketAsyncEventArgs.Completed += this.OnComplete;
			this.innSocketAsyncEventArgs.Completed += this.OnComplete;
		}

		public TSocket(TPoller poller, Socket socket)
		{
			this.poller = poller;
			this.socket = socket;
			this.outSocketAsyncEventArgs.Completed += this.OnComplete;
			this.innSocketAsyncEventArgs.Completed += this.OnComplete;
		}

		public void Dispose()
		{
			if (this.socket == null)
			{
				return;
			}
			socket.Dispose();
			this.socket = null;
		}

		public void Connect(string host, int port)
		{
			if (socket.ConnectAsync(this.innSocketAsyncEventArgs))
			{
				return;
			}

			this.poller.Add(this.OnConnComplete);
		}

		public void Accept(int port)
		{
			this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
			this.socket.Listen(100);
			this.BeginAccept();
		}

		public bool Recv(byte[] buffer)
		{
			if (buffer.Length > this.RecvSize)
			{
				return false;
			}
			this.recvBuffer.RecvFrom(buffer);
			return true;
		}

		public void Send(byte[] buffer)
		{
			bool needBeginSend = this.sendBuffer.Count == 0;
			this.sendBuffer.SendTo(buffer);
			if (needBeginSend)
			{
				this.BeginSend();
			}
		}

		public int RecvSize
		{
			get
			{
				return this.recvBuffer.Count;
			}
		}

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			Action action;
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					action = () => this.OnAcceptComplete(e.AcceptSocket);
					e.AcceptSocket = null;
					break;
				case SocketAsyncOperation.Connect:
					action = this.OnConnComplete;
					break;
				case SocketAsyncOperation.Disconnect:
					action = this.OnDisconnect;
					break;
				case SocketAsyncOperation.Receive:
					action = () => this.OnRecvComplete(e.BytesTransferred);
					break;
				case SocketAsyncOperation.Send:
					action = () => this.OnSendComplete(e.BytesTransferred);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			this.poller.Add(action);
		}

		private void OnDisconnect()
		{
			this.Dispose();
		}

		private void OnAcceptComplete(Socket sock)
		{
			if (this.socket == null)
			{
				return;
			}

			TSocket newSocket = new TSocket(poller, sock);
			if (this.AcceptAction != null)
			{
				this.AcceptAction(newSocket);
			}
			this.BeginAccept();
		}

		private void OnConnComplete()
		{
			if (this.socket == null)
			{
				return;
			}
			this.BeginRecv();
		}

		private void OnRecvComplete(int bytesTransferred)
		{
			if (this.socket == null)
			{
				return;
			}
			this.recvBuffer.LastIndex += bytesTransferred;
			if (this.recvBuffer.LastIndex == TBuffer.ChunkSize)
			{
				this.recvBuffer.LastIndex = 0;
				this.recvBuffer.AddLast();
			}

			this.BeginRecv();

			if (this.RecvAction != null)
			{
				this.RecvAction();
			}
		}

		private void OnSendComplete(int bytesTransferred)
		{
			if (this.socket == null)
			{
				return;
			}

			this.sendBuffer.FirstIndex += bytesTransferred;
			if (this.sendBuffer.FirstIndex == TBuffer.ChunkSize)
			{
				this.sendBuffer.FirstIndex = 0;
				this.sendBuffer.RemoveFirst();
			}

			// 如果没有数据可以发送,则返回
			if (this.sendBuffer.Count == 0)
			{
				return;
			}

			// 继续发送数据
			this.BeginSend();
		}

		private void BeginAccept()
		{
			if (this.socket == null)
			{
				return;
			}

			if (this.socket.AcceptAsync(this.innSocketAsyncEventArgs))
			{
				return;
			}
			Action action = () => this.OnAcceptComplete(this.innSocketAsyncEventArgs.AcceptSocket);
			this.poller.Add(action);
		}

		private void BeginRecv()
		{
			if (this.socket == null)
			{
				return;
			}

			this.innSocketAsyncEventArgs.SetBuffer(this.recvBuffer.Last, this.recvBuffer.LastIndex, TBuffer.ChunkSize - this.recvBuffer.LastIndex);
			if (this.socket.ReceiveAsync(this.innSocketAsyncEventArgs))
			{
				return;
			}

			Action action = () => this.OnRecvComplete(this.innSocketAsyncEventArgs.BytesTransferred);
			this.poller.Add(action);
		}

		private void BeginSend()
		{
			if (this.socket == null)
			{
				return;
			}

			int count = 0;
			if (TBuffer.ChunkSize - this.sendBuffer.FirstIndex < this.sendBuffer.Count)
			{
				count = TBuffer.ChunkSize - this.sendBuffer.FirstIndex;
			}
			else
			{
				count = this.sendBuffer.Count;
			}
			this.outSocketAsyncEventArgs.SetBuffer(this.sendBuffer.First, this.sendBuffer.FirstIndex, count);
			if (this.socket.SendAsync(outSocketAsyncEventArgs))
			{
				return;
			}
			Action action = () => this.OnSendComplete(this.outSocketAsyncEventArgs.BytesTransferred);
			this.poller.Add(action);
		}
	}
}
