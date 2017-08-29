using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Model
{
	public class TChannel : AChannel
	{
		private readonly TcpClient tcpClient;

		private readonly TBuffer recvBuffer = new TBuffer();
		private readonly TBuffer sendBuffer = new TBuffer();

		private bool isSending;
		private readonly PacketParser parser;
		private bool isConnected;
		private TaskCompletionSource<byte[]> recvTcs;

		/// <summary>
		/// connect
		/// </summary>
		public TChannel(TcpClient tcpClient, string host, int port, TService service) : base(service, ChannelType.Connect)
		{
			this.tcpClient = tcpClient;
			this.parser = new PacketParser(this.recvBuffer);
			this.RemoteAddress = host + ":" + port;

			this.ConnectAsync(host, port);
		}

		/// <summary>
		/// accept
		/// </summary>
		public TChannel(TcpClient tcpClient, TService service) : base(service, ChannelType.Accept)
		{
			this.tcpClient = tcpClient;
			this.parser = new PacketParser(this.recvBuffer);

			IPEndPoint ipEndPoint = (IPEndPoint)this.tcpClient.Client.RemoteEndPoint;
			this.RemoteAddress = ipEndPoint.Address + ":" + ipEndPoint.Port;
			this.OnAccepted();
		}

		private async void ConnectAsync(string host, int port)
		{
			try
			{
				await this.tcpClient.ConnectAsync(host, port);
				this.isConnected = true;
				this.StartSend();
				this.StartRecv();
			}
			catch (SocketException e)
			{
				Log.Error($"connect error: {e.SocketErrorCode}");
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			long id = this.Id;

			base.Dispose();

			this.tcpClient.Close();
			this.service.Remove(id);
		}

		private void OnAccepted()
		{
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
			byte[] size = BitConverter.GetBytes((ushort)buffer.Length);
			this.sendBuffer.SendTo(size);
			this.sendBuffer.SendTo(buffer);
			if (this.isConnected)
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
			ushort size = (ushort)buffers.Select(b => b.Length).Sum();
			size = NetworkHelper.HostToNetworkOrder(size);
			byte[] sizeBuffer = BitConverter.GetBytes(size);
			this.sendBuffer.SendTo(sizeBuffer);
			foreach (byte[] buffer in buffers)
			{
				this.sendBuffer.SendTo(buffer);
			}
			if (this.isConnected)
			{
				this.StartSend();
			}
		}

		private async void StartSend()
		{
			try
			{
				// 如果正在发送中,不需要再次发送
				if (this.isSending)
				{
					return;
				}

				while (true)
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
					await this.tcpClient.GetStream().WriteAsync(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);
					this.sendBuffer.FirstIndex += sendSize;
					if (this.sendBuffer.FirstIndex == TBuffer.ChunkSize)
					{
						this.sendBuffer.FirstIndex = 0;
						this.sendBuffer.RemoveFirst();
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				this.OnError(this, SocketError.SocketError);
			}
		}

		private async void StartRecv()
		{
			try
			{
				while (true)
				{
					if (this.Id == 0)
					{
						return;
					}
					int size = TBuffer.ChunkSize - this.recvBuffer.LastIndex;

					int n = await this.tcpClient.GetStream().ReadAsync(this.recvBuffer.Last, this.recvBuffer.LastIndex, size);

					if (n == 0)
					{
						this.OnError(this, SocketError.NetworkReset);
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
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				this.OnError(this, SocketError.SocketError);
			}
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