using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;

namespace ETModel
{
	/// <summary>
	/// 封装Socket,将回调push到主线程处理
	/// </summary>
	public sealed class TChannel: AChannel
	{
		private Socket socket;
		private SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
		private SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();

		private readonly CircularBuffer recvBuffer = new CircularBuffer();
		private readonly CircularBuffer sendBuffer = new CircularBuffer();

		private readonly MemoryStream memoryStream;

		private bool isSending;

		private bool isRecving;

		private bool isConnected;

		private readonly PacketParser parser;

		private readonly byte[] packetSizeCache;
		
		public TChannel(IPEndPoint ipEndPoint, TService service): base(service, ChannelType.Connect)
		{
			int packetSize = service.PacketSizeLength;
			this.packetSizeCache = new byte[packetSize];
			this.memoryStream = service.MemoryStreamManager.GetStream("message", ushort.MaxValue);
			
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.socket.NoDelay = true;
			this.parser = new PacketParser(packetSize, this.recvBuffer, this.memoryStream);
			this.innArgs.Completed += this.OnComplete;
			this.outArgs.Completed += this.OnComplete;

			this.RemoteAddress = ipEndPoint;

			this.isConnected = false;
			this.isSending = false;
		}
		
		public TChannel(Socket socket, TService service): base(service, ChannelType.Accept)
		{
			int packetSize = service.PacketSizeLength;
			this.packetSizeCache = new byte[packetSize];
			this.memoryStream = service.MemoryStreamManager.GetStream("message", ushort.MaxValue);
			
			this.socket = socket;
			this.socket.NoDelay = true;
			this.parser = new PacketParser(packetSize, this.recvBuffer, this.memoryStream);
			this.innArgs.Completed += this.OnComplete;
			this.outArgs.Completed += this.OnComplete;

			this.RemoteAddress = (IPEndPoint)socket.RemoteEndPoint;
			
			this.isConnected = true;
			this.isSending = false;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();
			
			this.socket.Close();
			this.innArgs.Dispose();
			this.outArgs.Dispose();
			this.innArgs = null;
			this.outArgs = null;
			this.socket = null;
			this.memoryStream.Dispose();
		}
		
		private TService GetService()
		{
			return (TService)this.Service;
		}

		public override MemoryStream Stream
		{
			get
			{
				return this.memoryStream;
			}
		}

		public override void Start()
		{
			if (!this.isConnected)
			{
				this.ConnectAsync(this.RemoteAddress);
				return;
			}

			if (!this.isRecving)
			{
				this.isRecving = true;
				this.StartRecv();
			}

			this.GetService().MarkNeedStartSend(this.Id);
		}
		
		public override void Send(MemoryStream stream)
		{
			if (this.IsDisposed)
			{
				throw new Exception("TChannel已经被Dispose, 不能发送消息");
			}

			switch (this.GetService().PacketSizeLength)
			{
				case Packet.PacketSizeLength4:
					if (stream.Length > ushort.MaxValue * 16)
					{
						throw new Exception($"send packet too large: {stream.Length}");
					}
					this.packetSizeCache.WriteTo(0, (int) stream.Length);
					break;
				case Packet.PacketSizeLength2:
					if (stream.Length > ushort.MaxValue)
					{
						throw new Exception($"send packet too large: {stream.Length}");
					}
					this.packetSizeCache.WriteTo(0, (ushort) stream.Length);
					break;
				default:
					throw new Exception("packet size must be 2 or 4!");
			}

			this.sendBuffer.Write(this.packetSizeCache, 0, this.packetSizeCache.Length);
			this.sendBuffer.Write(stream);

			this.GetService().MarkNeedStartSend(this.Id);
		}

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Connect:
					OneThreadSynchronizationContext.Instance.Post(this.OnConnectComplete, e);
					break;
				case SocketAsyncOperation.Receive:
					OneThreadSynchronizationContext.Instance.Post(this.OnRecvComplete, e);
					break;
				case SocketAsyncOperation.Send:
					OneThreadSynchronizationContext.Instance.Post(this.OnSendComplete, e);
					break;
				case SocketAsyncOperation.Disconnect:
					OneThreadSynchronizationContext.Instance.Post(this.OnDisconnectComplete, e);
					break;
				default:
					throw new Exception($"socket error: {e.LastOperation}");
			}
		}

		public void ConnectAsync(IPEndPoint ipEndPoint)
		{
			this.outArgs.RemoteEndPoint = ipEndPoint;
			if (this.socket.ConnectAsync(this.outArgs))
			{
				return;
			}
			OnConnectComplete(this.outArgs);
		}

		private void OnConnectComplete(object o)
		{
			if (this.socket == null)
			{
				return;
			}
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) o;
			
			if (e.SocketError != SocketError.Success)
			{
				this.OnError((int)e.SocketError);	
				return;
			}

			e.RemoteEndPoint = null;
			this.isConnected = true;
			
			this.Start();
		}

		private void OnDisconnectComplete(object o)
		{
			SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;
			this.OnError((int)e.SocketError);
		}

		private void StartRecv()
		{
			int size = this.recvBuffer.ChunkSize - this.recvBuffer.LastIndex;
			this.RecvAsync(this.recvBuffer.Last, this.recvBuffer.LastIndex, size);
		}

		public void RecvAsync(byte[] buffer, int offset, int count)
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
				return;
			}
			OnRecvComplete(this.innArgs);
		}

		private void OnRecvComplete(object o)
		{
			if (this.socket == null)
			{
				return;
			}
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) o;

			if (e.SocketError != SocketError.Success)
			{
				this.OnError((int)e.SocketError);
				return;
			}

			if (e.BytesTransferred == 0)
			{
				this.OnError(ErrorCode.ERR_PeerDisconnect);
				return;
			}

			this.recvBuffer.LastIndex += e.BytesTransferred;
			if (this.recvBuffer.LastIndex == this.recvBuffer.ChunkSize)
			{
				this.recvBuffer.AddLast();
				this.recvBuffer.LastIndex = 0;
			}

			// 收到消息回调
			while (true)
			{
				try
				{
					if (!this.parser.Parse())
					{
						break;
					}
				}
				catch (Exception ee)
				{
					Log.Error($"ip: {this.RemoteAddress} {ee}");
					this.OnError(ErrorCode.ERR_SocketError);
					return;
				}

				try
				{
					this.OnRead(this.parser.GetPacket());
				}
				catch (Exception ee)
				{
					Log.Error(ee);
				}
			}

			if (this.socket == null)
			{
				return;
			}
			
			this.StartRecv();
		}

		public bool IsSending => this.isSending;

		public void StartSend()
		{
			if(!this.isConnected)
			{
				return;
			}
			
			// 没有数据需要发送
			if (this.sendBuffer.Length == 0)
			{
				this.isSending = false;
				return;
			}

			this.isSending = true;

			int sendSize = this.sendBuffer.ChunkSize - this.sendBuffer.FirstIndex;
			if (sendSize > this.sendBuffer.Length)
			{
				sendSize = (int)this.sendBuffer.Length;
			}

			this.SendAsync(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);
		}

		public void SendAsync(byte[] buffer, int offset, int count)
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
				return;
			}
			OnSendComplete(this.outArgs);
		}

		private void OnSendComplete(object o)
		{
			if (this.socket == null)
			{
				return;
			}
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) o;

			if (e.SocketError != SocketError.Success)
			{
				this.OnError((int)e.SocketError);
				return;
			}
			
			if (e.BytesTransferred == 0)
			{
				this.OnError(ErrorCode.ERR_PeerDisconnect);
				return;
			}
			
			this.sendBuffer.FirstIndex += e.BytesTransferred;
			if (this.sendBuffer.FirstIndex == this.sendBuffer.ChunkSize)
			{
				this.sendBuffer.FirstIndex = 0;
				this.sendBuffer.RemoveFirst();
			}
			
			this.StartSend();
		}
	}
}