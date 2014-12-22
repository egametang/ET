using System;
using Common.Helper;
using Common.Logger;
using MongoDB.Bson;

namespace TNet
{
	public class TSession: IDisposable
	{
		private const int RecvSendInterval = 100;
		private readonly TServer server;
		private TSocket socket;
		private readonly TBuffer recvBuffer = new TBuffer();
		private readonly TBuffer sendBuffer = new TBuffer();
		public ObjectId SendTimer = ObjectId.Empty;
		public ObjectId RecvTimer = ObjectId.Empty;
		private event Action onRecv = () => { };
		private event Action onSend = () => { };

		public event Action OnRecv
		{
			add
			{
				this.onRecv += value;
			}
			remove
			{
				this.onRecv -= value;
			} 
		}
		public event Action OnSend
		{
			add
			{
				this.onSend += value;
			}
			remove
			{
				this.onSend -= value;
			}
		}

		public TSession(TSocket socket, TServer server)
		{
			this.socket = socket;
			this.server = server;
		}

		public void Dispose()
		{
			if (this.socket == null)
			{
				return;
			}
			this.server.Remove(socket.RemoteAddress);
			this.socket.Dispose();
			this.socket = null;
		}

		public void Send(byte[] buffer)
		{
			this.sendBuffer.SendTo(buffer);
			if (this.SendTimer == ObjectId.Empty)
			{
				this.SendTimer = this.server.Timer.Add(TimeHelper.Now() + RecvSendInterval, this.SendTimerCallback);
			}
		}

		private async void SendTimerCallback()
		{
			try
			{
				while (true)
				{
					if (this.sendBuffer.Count == 0)
					{
						break;
					}
					int sendSize = TBuffer.ChunkSize - this.sendBuffer.FirstIndex;
					if (sendSize > this.sendBuffer.Count)
					{
						sendSize = this.sendBuffer.Count;
					}
					int n = await this.socket.SendAsync(
						this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);
					this.sendBuffer.FirstIndex += n;
					if (this.sendBuffer.FirstIndex == TBuffer.ChunkSize)
					{
						this.sendBuffer.FirstIndex = 0;
						this.sendBuffer.RemoveFirst();
					}
				}
			}
			catch (Exception e)
			{
				Log.Trace(e.ToString());
			}

			this.onSend();
			this.SendTimer = ObjectId.Empty;
		}

		public int RecvSize
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

		public async void Start()
		{
			try
			{
				while (true)
				{
					int n = await this.socket.RecvAsync(
						this.recvBuffer.Last, this.recvBuffer.LastIndex, TBuffer.ChunkSize - this.recvBuffer.LastIndex);
					if (n == 0)
					{
						break;
					}

					this.recvBuffer.LastIndex += n;
					if (this.recvBuffer.LastIndex == TBuffer.ChunkSize)
					{
						this.recvBuffer.AddLast();
						this.recvBuffer.LastIndex = 0;
					}

					if (this.RecvTimer == ObjectId.Empty)
					{
						this.RecvTimer = this.server.Timer.Add(TimeHelper.Now() + RecvSendInterval, this.RecvTimerCallback);
					}
				}
			}
			catch (Exception e)
			{
				Log.Trace(e.ToString());
			}
		}

		private void RecvTimerCallback()
		{
			this.onRecv();
			this.RecvTimer = ObjectId.Empty;
		}
	}
}
