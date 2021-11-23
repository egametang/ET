using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET
{
	public struct KcpWaitPacket
	{
		public long ActorId;
		public MemoryStream MemoryStream;
	}
	
	public class KChannel : AChannel
	{
		public static readonly Dictionary<IntPtr, KChannel> KcpPtrChannels = new Dictionary<IntPtr, KChannel>();
		
		public KService Service;
		
		private Socket socket;

		public IntPtr kcp { get; private set; }

		private readonly Queue<KcpWaitPacket> sendBuffer = new Queue<KcpWaitPacket>();

		private uint lastRecvTime;
		
		public readonly uint CreateTime;

		public uint LocalConn { get; set; }
		public uint RemoteConn { get; set; }

		private readonly byte[] sendCache = new byte[2 * 1024];
		
		public bool IsConnected { get; private set; }

		public string RealAddress { get; set; }
		
		private const int maxPacketSize = 10000;

		private MemoryStream ms = new MemoryStream(maxPacketSize);

		private MemoryStream readMemory;
		private int needReadSplitCount;
		
		private void InitKcp()
		{
			KcpPtrChannels.Add(this.kcp, this);
			switch (this.Service.ServiceType)
			{
				case ServiceType.Inner:
					Kcp.KcpNodelay(kcp, 1, 10, 2, 1);
					Kcp.KcpWndsize(kcp, ushort.MaxValue, ushort.MaxValue);
					Kcp.KcpSetmtu(kcp, 1400); // 默认1400
					Kcp.KcpSetminrto(kcp, 30);
					break;
				case ServiceType.Outer:
					Kcp.KcpNodelay(kcp, 1, 10, 2, 1);
					Kcp.KcpWndsize(kcp, 256, 256);
					Kcp.KcpSetmtu(kcp, 470);
					Kcp.KcpSetminrto(kcp, 30);
					break;
			}

		}
		
		// connect
		public KChannel(long id, uint localConn, Socket socket, IPEndPoint remoteEndPoint, KService kService)
		{
			this.LocalConn = localConn;

			this.Id = id;
			this.ChannelType = ChannelType.Connect;
			
			Log.Info($"channel create: {this.Id} {this.LocalConn} {remoteEndPoint} {this.ChannelType}");
			
			this.kcp = IntPtr.Zero;
			this.Service = kService;
			this.RemoteAddress = remoteEndPoint;
			this.socket = socket;
			this.lastRecvTime = kService.TimeNow;
			this.CreateTime = kService.TimeNow;

			this.Connect();

		}

		// accept
		public KChannel(long id, uint localConn, uint remoteConn, Socket socket, IPEndPoint remoteEndPoint, KService kService)
		{
			this.Id = id;
			this.ChannelType = ChannelType.Accept;
			
			Log.Info($"channel create: {this.Id} {localConn} {remoteConn} {remoteEndPoint} {this.ChannelType}");

			this.Service = kService;
			this.LocalConn = localConn;
			this.RemoteConn = remoteConn;
			this.RemoteAddress = remoteEndPoint;
			this.socket = socket;
			this.kcp = Kcp.KcpCreate(this.RemoteConn, IntPtr.Zero);
			this.InitKcp();
			
			this.lastRecvTime = kService.TimeNow;
			this.CreateTime = kService.TimeNow;
		}
	

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			uint localConn = this.LocalConn;
			uint remoteConn = this.RemoteConn;
			Log.Info($"channel dispose: {this.Id} {localConn} {remoteConn}");
			
			long id = this.Id;
			this.Id = 0;
			this.Service.Remove(id);

			try
			{
				//this.Service.Disconnect(localConn, remoteConn, this.Error, this.RemoteAddress, 3);
			}

			catch (Exception e)
			{
				Log.Error(e);
			}

			if (this.kcp != IntPtr.Zero)
			{
				KcpPtrChannels.Remove(this.kcp);
				Kcp.KcpRelease(this.kcp);
				this.kcp = IntPtr.Zero;
			}

			this.socket = null;
		}

		public void HandleConnnect()
		{
			// 如果连接上了就不用处理了
			if (this.IsConnected)
			{
				return;
			}

			this.kcp = Kcp.KcpCreate(this.RemoteConn, IntPtr.Zero);
			this.InitKcp();

			Log.Info($"channel connected: {this.Id} {this.LocalConn} {this.RemoteConn} {this.RemoteAddress}");
			this.IsConnected = true;
			this.lastRecvTime = this.Service.TimeNow;
			
			while (true)
			{
				if (this.sendBuffer.Count <= 0)
				{
					break;
				}
				
				KcpWaitPacket buffer = this.sendBuffer.Dequeue();
				this.KcpSend(buffer);
			}
		}

		/// <summary>
		/// 发送请求连接消息
		/// </summary>
		private void Connect()
		{
			try
			{
				uint timeNow = this.Service.TimeNow;
				
				this.lastRecvTime = timeNow;
				
				byte[] buffer = sendCache;
				buffer.WriteTo(0, KcpProtocalType.SYN);
				buffer.WriteTo(1, this.LocalConn);
				buffer.WriteTo(5, this.RemoteConn);
				this.socket.SendTo(buffer, 0, 9, SocketFlags.None, this.RemoteAddress);
				Log.Info($"kchannel connect {this.Id} {this.LocalConn} {this.RemoteConn} {this.RealAddress} {this.socket.LocalEndPoint}");
				
				// 300毫秒后再次update发送connect请求
				this.Service.AddToUpdateNextTime(timeNow + 300, this.Id);
			}
			catch (Exception e)
			{
				Log.Error(e);
				this.OnError(ErrorCore.ERR_SocketCantSend);
			}
		}

		public void Update()
		{
			if (this.IsDisposed)
			{
				return;
			}

			uint timeNow = this.Service.TimeNow;
			
			// 如果还没连接上，发送连接请求
			if (!this.IsConnected)
			{
				// 10秒超时没连接上则报错
				if (timeNow - this.CreateTime > 10000)
				{
					Log.Error($"kChannel connect timeout: {this.Id} {this.RemoteConn} {timeNow} {this.CreateTime} {this.ChannelType} {this.RemoteAddress}");
					this.OnError(ErrorCore.ERR_KcpConnectTimeout);
					return;
				}
				switch (ChannelType)
				{
					case ChannelType.Connect:
						this.Connect();
						break;
				}
				return;
			}

			if (this.kcp == IntPtr.Zero)
			{
				return;
			}
			
			try
			{
				Kcp.KcpUpdate(this.kcp, timeNow);
			}
			catch (Exception e)
			{
				Log.Error(e);
				this.OnError(ErrorCore.ERR_SocketError);
				return;
			}

			uint nextUpdateTime = Kcp.KcpCheck(this.kcp, timeNow);
			this.Service.AddToUpdateNextTime(nextUpdateTime, this.Id);
		}

		public void HandleRecv(byte[] date, int offset, int length)
		{
			if (this.IsDisposed)
			{
				return;
			}

			this.IsConnected = true;
			
			Kcp.KcpInput(this.kcp, date, offset, length);
			this.Service.AddToUpdateNextTime(0, this.Id);

			while (true)
			{
				if (this.IsDisposed)
				{
					break;
				}
				int n = Kcp.KcpPeeksize(this.kcp);
				if (n < 0)
				{
					break;
				}
				if (n == 0)
				{
					this.OnError((int)SocketError.NetworkReset);
					return;
				}


				if (this.needReadSplitCount > 0) // 说明消息分片了
				{
					byte[] buffer = readMemory.GetBuffer();
					int count = Kcp.KcpRecv(this.kcp, buffer, (int)this.readMemory.Length - this.needReadSplitCount, n);
					this.needReadSplitCount -= count;
					if (n != count)
					{
						Log.Error($"kchannel read error1: {this.LocalConn} {this.RemoteConn}");
						this.OnError(ErrorCore.ERR_KcpReadNotSame);
						return;
					}
					
					if (this.needReadSplitCount < 0)
					{
						Log.Error($"kchannel read error2: {this.LocalConn} {this.RemoteConn}");
						this.OnError(ErrorCore.ERR_KcpSplitError);
						return;
					}
										
					// 没有读完
					if (this.needReadSplitCount != 0)
					{
						continue;
					}
				}
				else
				{
					this.readMemory = this.ms;
					this.readMemory.SetLength(n);
					this.readMemory.Seek(0, SeekOrigin.Begin);
					
					byte[] buffer = readMemory.GetBuffer();
					int count = Kcp.KcpRecv(this.kcp, buffer, 0, n);
					if (n != count)
					{
						break;
					}
					
					// 判断是不是分片
					if (n == 8)
					{
						int headInt = BitConverter.ToInt32(this.readMemory.GetBuffer(), 0);
						if (headInt == 0)
						{
							this.needReadSplitCount = BitConverter.ToInt32(readMemory.GetBuffer(), 4);
							if (this.needReadSplitCount <= maxPacketSize)
							{
								Log.Error($"kchannel read error3: {this.needReadSplitCount} {this.LocalConn} {this.RemoteConn}");
								this.OnError(ErrorCore.ERR_KcpSplitCountError);
								return;
							}
							this.readMemory = new MemoryStream(this.needReadSplitCount);
							this.readMemory.SetLength(this.needReadSplitCount);
							this.readMemory.Seek(0, SeekOrigin.Begin);
							continue;
						}
					}
				}


				switch (this.Service.ServiceType)
				{
					case ServiceType.Inner:
						this.readMemory.Seek(Packet.ActorIdLength + Packet.OpcodeLength, SeekOrigin.Begin);
						break;
					case ServiceType.Outer:
						this.readMemory.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
						break;
				}
				this.lastRecvTime = this.Service.TimeNow;
				MemoryStream mem = this.readMemory;
				this.readMemory = null;
				this.OnRead(mem);
			}
		}

		public void Output(IntPtr bytes, int count)
		{
			if (this.IsDisposed)
			{
				return;
			}
			try
			{
				// 没连接上 kcp不往外发消息, 其实本来没连接上不会调用update，这里只是做一层保护
				if (!this.IsConnected)
				{
					return;
				}
				
				if (count == 0)
				{
					Log.Error($"output 0");
					return;
				}

				byte[] buffer = this.sendCache;
				buffer.WriteTo(0, KcpProtocalType.MSG);
				// 每个消息头部写下该channel的id;
				buffer.WriteTo(1, this.LocalConn);
				Marshal.Copy(bytes, buffer, 5, count);
				this.socket.SendTo(buffer, 0, count + 5, SocketFlags.None, this.RemoteAddress);
			}
			catch (Exception e)
			{
				Log.Error(e);
				this.OnError(ErrorCore.ERR_SocketCantSend);
			}
		}

        private void KcpSend(KcpWaitPacket kcpWaitPacket)
		{
			if (this.IsDisposed)
			{
				return;
			}

			MemoryStream memoryStream = kcpWaitPacket.MemoryStream;
			int count = (int) (memoryStream.Length - memoryStream.Position);
			
			if (this.Service.ServiceType == ServiceType.Inner)
			{
				memoryStream.GetBuffer().WriteTo(0, kcpWaitPacket.ActorId);
			}

			// 超出maxPacketSize需要分片
			if (count <= maxPacketSize)
			{
				Kcp.KcpSend(this.kcp, memoryStream.GetBuffer(), (int)memoryStream.Position, count);
			}
			else
			{
				// 先发分片信息
				this.sendCache.WriteTo(0, 0);
				this.sendCache.WriteTo(4, count);
				Kcp.KcpSend(this.kcp, this.sendCache, 0, 8);

				// 分片发送
				int alreadySendCount = 0;
				while (alreadySendCount < count)
				{
					int leftCount = count - alreadySendCount;
					
					int sendCount = leftCount < maxPacketSize? leftCount: maxPacketSize;
					
					Kcp.KcpSend(this.kcp, memoryStream.GetBuffer(), (int)memoryStream.Position + alreadySendCount, sendCount);
					
					alreadySendCount += sendCount;
				}
			}

			this.Service.AddToUpdateNextTime(0, this.Id);
		}
		
		public void Send(long actorId, MemoryStream stream)
		{
			if (this.kcp != IntPtr.Zero)
			{
				// 检查等待发送的消息，如果超出最大等待大小，应该断开连接
				int n = Kcp.KcpWaitsnd(this.kcp);

				int maxWaitSize = 0;
				switch (this.Service.ServiceType)
				{
					case ServiceType.Inner:
						maxWaitSize = Kcp.InnerMaxWaitSize;
						break;
					case ServiceType.Outer:
						maxWaitSize = Kcp.OuterMaxWaitSize;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				
				if (n > maxWaitSize)
				{
					Log.Error($"kcp wait snd too large: {n}: {this.Id} {this.LocalConn} {this.RemoteConn}");
					this.OnError(ErrorCore.ERR_KcpWaitSendSizeTooLarge);
					return;
				}
			}

			KcpWaitPacket kcpWaitPacket = new KcpWaitPacket() { ActorId = actorId, MemoryStream = stream };
			if (!this.IsConnected)
			{
				this.sendBuffer.Enqueue(kcpWaitPacket);
				return;
			}
			this.KcpSend(kcpWaitPacket);
		}
		
		private void OnRead(MemoryStream memoryStream)
		{
			this.Service.OnRead(this.Id, memoryStream);
		}
		
		public void OnError(int error)
		{
			long channelId = this.Id;
			this.Service.Remove(channelId);
			this.Service.OnError(channelId, error);
		}
	}
}
