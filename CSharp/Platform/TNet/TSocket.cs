using System;
using System.Net.Sockets;

namespace TNet
{
	public class TSocketState
	{
		public Action Action { get; set; }

		public void Run()
		{
			this.Action();
		}
	}

	public class TSocket: IDisposable
	{
		private Socket socket;
		private readonly TPoller poller;
		private readonly SocketAsyncEventArgs innSocketAsyncEventArgs = new SocketAsyncEventArgs();
		private readonly SocketAsyncEventArgs outSocketAsyncEventArgs = new SocketAsyncEventArgs();
		private readonly TBuffer recvBuffer = new TBuffer();
		private readonly TBuffer sendBuffer = new TBuffer();
		public bool IsSending { get; private set; }

		public TSocket(TPoller poller)
		{
			this.poller = poller;
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.outSocketAsyncEventArgs.Completed += this.OnComplete;
			this.innSocketAsyncEventArgs.Completed += this.OnComplete;
			this.IsSending = false;
		}

		public void Dispose()
		{
			if (this.socket == null)
			{
				return;
			}
			socket.Dispose();
			this.socket = null;
			poller.CanWriteSocket.Remove(this);
		}

		public void Connect(string host, int port)
		{
			socket.ConnectAsync(this.innSocketAsyncEventArgs);
		}

		public int CanRecvSize
		{
			get
			{
				return this.recvBuffer.Count;
			}
		}

		public void Recv(byte[] buffer)
		{
			this.recvBuffer.RecvFrom(buffer);
		}

		public void Send(byte[] buffer)
		{
			this.sendBuffer.SendTo(buffer);
			// 如果正在发送,则不做可发送标记
			if (this.IsSending)
			{
				return;
			}
			if (this.poller.CanWriteSocket.Contains(this))
			{
				return;
			}
			this.poller.CanWriteSocket.Add(this);
		}

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			Action action = () => { };
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					break;
				case SocketAsyncOperation.Connect:
					action = this.OnConnComplete;
					break;
				case SocketAsyncOperation.Disconnect:
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
			TSocketState socketState = new TSocketState
			{
				Action = action,
			};
			
			this.poller.Add(socketState);
		}

		private void OnConnComplete()
		{
			this.BeginRecv();
		}

		private void OnRecvComplete(int bytesTransferred)
		{
			this.recvBuffer.LastIndex += bytesTransferred;
			if (this.recvBuffer.LastIndex == TBuffer.ChunkSize)
			{
				this.recvBuffer.LastIndex = 0;
				this.recvBuffer.AddLast();
			}
			this.BeginRecv();
		}

		private void OnSendComplete(int bytesTransferred)
		{
			this.sendBuffer.FirstIndex += bytesTransferred;
			if (this.sendBuffer.FirstIndex == TBuffer.ChunkSize)
			{
				this.sendBuffer.FirstIndex = 0;
				this.sendBuffer.RemoveFirst();
			}

			// 如果没有数据可以发送,则返回
			if (this.sendBuffer.Count == 0)
			{
				this.IsSending = false;
				return;
			}

			// 继续发送数据
			this.BeginSend();
		}

		private void BeginRecv()
		{
			this.innSocketAsyncEventArgs.SetBuffer(this.recvBuffer.Last, this.recvBuffer.LastIndex, TBuffer.ChunkSize - this.recvBuffer.LastIndex);
			this.socket.ReceiveAsync(this.innSocketAsyncEventArgs);
		}

		public void BeginSend()
		{
			this.IsSending = true;
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
			this.socket.SendAsync(outSocketAsyncEventArgs);
		}
	}
}
