using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Base
{
	public class TChannel : AChannel
	{
		private readonly TSocket socket;

		private readonly TBuffer recvBuffer = new TBuffer();
		private readonly TBuffer sendBuffer = new TBuffer();

		private bool isSending;
		private readonly PacketParser parser;
		private bool isConnected;
		private TaskCompletionSource<byte[]> recvTcs;

		/// <summary>
		/// connect
		/// </summary>
		public TChannel(TSocket socket, string host, int port, TService service) : base(service, ChannelType.Connect)
		{
			this.socket = socket;
			this.parser = new PacketParser(this.recvBuffer);
			this.RemoteAddress = host + ":" + port;
			
			bool result = this.socket.ConnectAsync(host, port);
			if (!result)
			{
				this.OnConnected(this.Id, SocketError.Success);
				return;
			}
			this.socket.OnConn += e => OnConnected(this.Id, e);
		}

		/// <summary>
		/// accept
		/// </summary>
		public TChannel(TSocket socket, TService service) : base(service, ChannelType.Accept)
		{
			this.socket = socket;
			this.parser = new PacketParser(this.recvBuffer);
			this.RemoteAddress = socket.RemoteAddress;
			this.OnAccepted();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			long id = this.Id;

			base.Dispose();

			this.socket.Dispose();
			this.service.Remove(id);
		}

		private void OnAccepted()
		{
			this.isConnected = true;
			this.StartSend();
			this.StartRecv();
		}

		private void OnConnected(long channelId, SocketError error)
		{
			if (this.service.GetChannel(channelId) == null)
			{
				return;
			}
			if (error != SocketError.Success)
			{
				Log.Error($"connect error: {error}");
				return;
			}
			this.isConnected = true;
			this.StartSend();
			this.StartRecv();
		}

		public override void Send(byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			if (this.Id == 0)
			{
				throw new Exception("TChannel已经被Dispose, 不能发送消息");
			}
			byte[] size = BitConverter.GetBytes(buffer.Length);
			this.sendBuffer.SendTo(size);
			this.sendBuffer.SendTo(buffer);
			if (!this.isSending && this.isConnected)
			{
				this.StartSend();
			}
		}

		public override void Send(List<byte[]> buffers, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			if (this.Id == 0)
			{
				throw new Exception("TChannel已经被Dispose, 不能发送消息");
			}
			int size = buffers.Select(b => b.Length).Sum();
			byte[] sizeBuffer = BitConverter.GetBytes(size);
			this.sendBuffer.SendTo(sizeBuffer);
			foreach (byte[] buffer in buffers)
			{
				this.sendBuffer.SendTo(buffer);
			}
			if (!this.isSending && this.isConnected)
			{
				this.StartSend();
			}
		}

		private void StartSend()
		{
			if (this.Id == 0)
			{
				return;
			}
			// 没有数据需要发送
			if (this.sendBuffer.Count == 0)
			{
				this.isSending = false;
				return;
			}

			this.isSending = true;

			int sendSize = TBuffer.ChunkSize - this.sendBuffer.FirstIndex;
			if (sendSize > this.sendBuffer.Count)
			{
				sendSize = this.sendBuffer.Count;
			}

			if (!this.socket.SendAsync(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize))
			{
				this.OnSend(sendSize, SocketError.Success);
				return;
			}
			this.socket.OnSend = this.OnSend;
		}

		private void OnSend(int n, SocketError error)
		{
			if (this.Id == 0)
			{
				return;
			}
			this.socket.OnSend = null;
			if (error != SocketError.Success)
			{
				this.OnError(this, error);
				return;
			}
			this.sendBuffer.FirstIndex += n;
			if (this.sendBuffer.FirstIndex == TBuffer.ChunkSize)
			{
				this.sendBuffer.FirstIndex = 0;
				this.sendBuffer.RemoveFirst();
			}

			this.StartSend();
		}

		private void StartRecv()
		{
			if (this.Id == 0)
			{
				return;
			}
			int size = TBuffer.ChunkSize - this.recvBuffer.LastIndex;
			
			if (!this.socket.RecvAsync(this.recvBuffer.Last, this.recvBuffer.LastIndex, size))
			{
				this.OnRecv(size, SocketError.Success);
			}
			this.socket.OnRecv = this.OnRecv;
		}

		private void OnRecv(int n, SocketError error)
		{
			if (this.Id == 0)
			{
				return;
			}
			this.socket.OnRecv = null;
			if (error != SocketError.Success)
			{
				this.OnError(this, error);
				return;
			}

			if (n == 0)
			{
				this.OnError(this, error);
				return;
			}
			
			this.recvBuffer.LastIndex += n;
			if (this.recvBuffer.LastIndex == TBuffer.ChunkSize)
			{
				this.recvBuffer.AddLast();
				this.recvBuffer.LastIndex = 0;
			}
			
			if (this.recvTcs != null)
			{
				byte[] packet = this.parser.GetPacket();
				if (packet != null)
				{
					var tcs = this.recvTcs;
					this.recvTcs = null;
					tcs.SetResult(packet);
				}
			}

			StartRecv();
		}

		public override Task<byte[]> Recv()
		{
			if (this.Id == 0)
			{
				throw new Exception("TChannel已经被Dispose, 不能接收消息");
			}
			
			byte[] packet = this.parser.GetPacket();
			if (packet != null)
			{
				return Task.FromResult(packet);
			}
			
			recvTcs = new TaskCompletionSource<byte[]>();
			return recvTcs.Task;
		}
	}
}