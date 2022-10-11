﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ET
{
	/// <summary>
	/// 封装Socket,将回调push到主线程处理
	/// </summary>
	public sealed class TChannel: AChannel
	{
		private readonly TService Service;
		private Socket socket;
		private SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
		private SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();

		private readonly CircularBuffer recvBuffer = new CircularBuffer();
		private readonly CircularBuffer sendBuffer = new CircularBuffer();

		private bool isSending;

		private bool isConnected;

		private readonly PacketParser parser;

		private readonly byte[] sendCache = new byte[Packet.OpcodeLength + Packet.ActorIdLength];

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			this.Service.Queue.Enqueue(new TArgs() {ChannelId = this.Id, SocketAsyncEventArgs = e});
		}
		
		public TChannel(long id, IPEndPoint ipEndPoint, TService service)
		{
			this.ChannelType = ChannelType.Connect;
			this.Id = id;
			this.Service = service;
			this.socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			this.socket.NoDelay = true;
			this.parser = new PacketParser(this.recvBuffer, this.Service);
			this.innArgs.Completed += this.OnComplete;
			this.outArgs.Completed += this.OnComplete;

			this.RemoteAddress = ipEndPoint;
			this.isConnected = false;
			this.isSending = false;
			
			this.Service.Queue.Enqueue(new TArgs(){Op = TcpOp.Connect,ChannelId = this.Id});
		}
		
		public TChannel(long id, Socket socket, TService service)
		{
			this.ChannelType = ChannelType.Accept;
			this.Id = id;
			this.Service = service;
			this.socket = socket;
			this.socket.NoDelay = true;
			this.parser = new PacketParser(this.recvBuffer, this.Service);
			this.innArgs.Completed += this.OnComplete;
			this.outArgs.Completed += this.OnComplete;

			this.RemoteAddress = (IPEndPoint)socket.RemoteEndPoint;
			this.isConnected = true;
			this.isSending = false;
			
			this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
			this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = this.Id});
		}
		
		

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			Log.Info($"channel dispose: {this.Id} {this.RemoteAddress} {this.Error}");
			
			long id = this.Id;
			this.Id = 0;
			this.Service.Remove(id);
			this.socket.Close();
			this.innArgs.Dispose();
			this.outArgs.Dispose();
			this.innArgs = null;
			this.outArgs = null;
			this.socket = null;
		}

		public void Send(long actorId, MemoryStream stream)
		{
			if (this.IsDisposed)
			{
				throw new Exception("TChannel已经被Dispose, 不能发送消息");
			}

			switch (this.Service.ServiceType)
			{
				case ServiceType.Inner:
				{
					int messageSize = (int) (stream.Length - stream.Position);
					if (messageSize > ushort.MaxValue * 16)
					{
						throw new Exception($"send packet too large: {stream.Length} {stream.Position}");
					}

					this.sendCache.WriteTo(0, messageSize);
					this.sendBuffer.Write(this.sendCache, 0, PacketParser.InnerPacketSizeLength);

					// actorId
					stream.GetBuffer().WriteTo(0, actorId);
					this.sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
					break;
				}
				case ServiceType.Outer:
				{
					stream.Seek(Packet.ActorIdLength, SeekOrigin.Begin); // 外网不需要actorId
					ushort messageSize = (ushort) (stream.Length - stream.Position);

					this.sendCache.WriteTo(0, messageSize);
					this.sendBuffer.Write(this.sendCache, 0, PacketParser.OuterPacketSizeLength);
					
					this.sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
					break;
				}
			}
			
			if (!this.isSending)
			{
				//this.StartSend();
				this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
			}
		}

		public void ConnectAsync()
		{
			this.outArgs.RemoteEndPoint = this.RemoteAddress;
			if (this.socket.ConnectAsync(this.outArgs))
			{
				return;
			}
			OnConnectComplete(this.outArgs);
		}

		public void OnConnectComplete(SocketAsyncEventArgs e)
		{
			if (this.socket == null)
			{
				return;
			}
			
			if (e.SocketError != SocketError.Success)
			{
				this.OnError((int)e.SocketError);	
				return;
			}

			e.RemoteEndPoint = null;
			this.isConnected = true;
			
			this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
			this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = this.Id});
		}

		public void OnDisconnectComplete(SocketAsyncEventArgs e)
		{
			this.OnError((int)e.SocketError);
		}

		public void StartRecv()
		{
			while (true)
			{
				try
				{
					if (this.socket == null)
					{
						return;
					}
					
					int size = this.recvBuffer.ChunkSize - this.recvBuffer.LastIndex;
					this.innArgs.SetBuffer(this.recvBuffer.Last, this.recvBuffer.LastIndex, size);
				}
				catch (Exception e)
				{
					Log.Error($"tchannel error: {this.Id}\n{e}");
					this.OnError(ErrorCore.ERR_TChannelRecvError);
					return;
				}
			
				if (this.socket.ReceiveAsync(this.innArgs))
				{
					return;
				}
				this.HandleRecv(this.innArgs);
			}
		}

		public void OnRecvComplete(SocketAsyncEventArgs o)
		{
			this.HandleRecv(o);
			
			if (this.socket == null)
			{
				return;
			}
			
			this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = this.Id});
		}

		private void HandleRecv(SocketAsyncEventArgs e)
		{
			if (this.socket == null)
			{
				return;
			}
			if (e.SocketError != SocketError.Success)
			{
				this.OnError((int)e.SocketError);
				return;
			}

			if (e.BytesTransferred == 0)
			{
				this.OnError(ErrorCore.ERR_PeerDisconnect);
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
				// 这里循环解析消息执行，有可能，执行消息的过程中断开了session
				if (this.socket == null)
				{
					return;
				}
				try
				{
					bool ret = this.parser.Parse();
					if (!ret)
					{
						break;
					}
					
					this.OnRead(this.parser.MemoryStream);
				}
				catch (Exception ee)
				{
					Log.Error($"ip: {this.RemoteAddress} {ee}");
					this.OnError(ErrorCore.ERR_SocketError);
					return;
				}
			}
		}

		public void StartSend()
		{
			if(!this.isConnected)
			{
				return;
			}

			if (this.isSending)
			{
				return;
			}
			
			while (true)
			{
				try
				{
					if (this.socket == null)
					{
						this.isSending = false;
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
					
					this.outArgs.SetBuffer(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);
					
					if (this.socket.SendAsync(this.outArgs))
					{
						return;
					}
				
					HandleSend(this.outArgs);
				}
				catch (Exception e)
				{
					throw new Exception($"socket set buffer error: {this.sendBuffer.First.Length}, {this.sendBuffer.FirstIndex}", e);
				}
			}
		}

		public void OnSendComplete(SocketAsyncEventArgs o)
		{
			HandleSend(o);
			
			this.isSending = false;
			
			this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
		}

		private void HandleSend(SocketAsyncEventArgs e)
		{
			if (this.socket == null)
			{
				return;
			}

			if (e.SocketError != SocketError.Success)
			{
				this.OnError((int)e.SocketError);
				return;
			}
			
			if (e.BytesTransferred == 0)
			{
				this.OnError(ErrorCore.ERR_PeerDisconnect);
				return;
			}
			
			this.sendBuffer.FirstIndex += e.BytesTransferred;
			if (this.sendBuffer.FirstIndex == this.sendBuffer.ChunkSize)
			{
				this.sendBuffer.FirstIndex = 0;
				this.sendBuffer.RemoveFirst();
			}
		}
		
		private void OnRead(MemoryStream memoryStream)
		{
			try
			{
				long channelId = this.Id;
				object message = null;
				long actorId = 0;
				switch (this.Service.ServiceType)
				{
					case ServiceType.Outer:
					{
						ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
						Type type = NetServices.Instance.GetType(opcode);
						message = SerializeHelper.Deserialize(type, memoryStream);
						break;
					}
					case ServiceType.Inner:
					{
						actorId = BitConverter.ToInt64(memoryStream.GetBuffer(), Packet.ActorIdIndex);
						ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
						Type type = NetServices.Instance.GetType(opcode);
						message = SerializeHelper.Deserialize(type, memoryStream);
						break;
					}
				}
				NetServices.Instance.OnRead(this.Service.Id, channelId, actorId, message);
			}
			catch (Exception e)
			{
				Log.Error($"{this.RemoteAddress} {memoryStream.Length} {e}");
				// 出现任何消息解析异常都要断开Session，防止客户端伪造消息
				this.OnError(ErrorCore.ERR_PacketParserError);
			}
		}

		private void OnError(int error)
		{
			Log.Info($"TChannel OnError: {error} {this.RemoteAddress}");
			
			long channelId = this.Id;
			
			this.Service.Remove(channelId);
			
			NetServices.Instance.OnError(this.Service.Id, channelId, error);
		}
	}
}