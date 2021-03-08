﻿using System;
using System.Collections.Concurrent;
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
		public KService Service;
		
		// 保存所有的channel
		public static readonly Dictionary<uint, KChannel> kChannels = new Dictionary<uint, KChannel>();
		
		public static readonly ConcurrentDictionary<long, ulong> idLocalRemoteConn = new ConcurrentDictionary<long, ulong>();
		
		private Socket socket;

		private IntPtr kcp;

		private readonly Queue<KcpWaitPacket> sendBuffer = new Queue<KcpWaitPacket>();

		private uint lastRecvTime;
		
		public readonly uint CreateTime;

		public uint LocalConn { get; set; }
		public uint RemoteConn { get; set; }

		private readonly byte[] sendCache = new byte[1024 * 1024];
		
		public bool IsConnected { get; private set; }

		public string RealAddress { get; set; }
		
		private void InitKcp()
		{
			switch (this.Service.ServiceType)
			{
				case ServiceType.Inner:
					Kcp.KcpNodelay(kcp, 1, 10, 2, 1);
					Kcp.KcpWndsize(kcp, 1024 * 100, 1024 * 100);
					Kcp.KcpSetmtu(kcp, 1400); // 默认1400
					Kcp.KcpSetminrto(kcp, 10);
					break;
				case ServiceType.Outer:
					Kcp.KcpNodelay(kcp, 1, 10, 2, 1);
					Kcp.KcpWndsize(kcp, 128, 128);
					Kcp.KcpSetmtu(kcp, 470);
					Kcp.KcpSetminrto(kcp, 10);
					break;
			}

		}
		
		// connect
		public KChannel(long id, uint localConn, Socket socket, IPEndPoint remoteEndPoint, KService kService)
		{
			this.LocalConn = localConn;
			if (kChannels.ContainsKey(this.LocalConn))
			{
				throw new Exception($"channel create error: {this.LocalConn} {remoteEndPoint} {this.ChannelType}");
			}

			this.Id = id;
			this.ChannelType = ChannelType.Connect;
			
			Log.Info($"channel create: {this.Id} {this.LocalConn} {remoteEndPoint} {this.ChannelType}");
			
			this.Service = kService;
			this.RemoteAddress = remoteEndPoint;
			this.socket = socket;
			this.kcp = Kcp.KcpCreate(this.RemoteConn, (IntPtr) this.LocalConn);

			kChannels.Add(this.LocalConn, this);
			
			this.lastRecvTime = kService.TimeNow;
			this.CreateTime = kService.TimeNow;

			this.Connect();

		}

		// accept
		public KChannel(long id, uint localConn, uint remoteConn, Socket socket, IPEndPoint remoteEndPoint, KService kService)
		{
			if (kChannels.ContainsKey(this.LocalConn))
			{
				throw new Exception($"channel create error: {localConn} {remoteEndPoint} {this.ChannelType}");
			}

			this.Id = id;
			this.ChannelType = ChannelType.Accept;
			
			Log.Info($"channel create: {this.Id} {localConn} {remoteConn} {remoteEndPoint} {this.ChannelType}");

			this.Service = kService;
			this.LocalConn = localConn;
			this.RemoteConn = remoteConn;
			this.RemoteAddress = remoteEndPoint;
			this.socket = socket;
			this.kcp = Kcp.KcpCreate(this.RemoteConn, (IntPtr) localConn);

			kChannels.Add(this.LocalConn, this);
			
			this.lastRecvTime = kService.TimeNow;
			this.CreateTime = kService.TimeNow;

			this.InitKcp();
		}

		

#region 网络线程

		


		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			uint localConn = this.LocalConn;
			uint remoteConn = this.RemoteConn;
			Log.Info($"channel dispose: {this.Id} {localConn} {remoteConn}");
			
			kChannels.Remove(localConn);
			idLocalRemoteConn.TryRemove(this.Id, out ulong _);
			
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

			this.kcp = Kcp.KcpCreate(this.RemoteConn, new IntPtr(this.LocalConn));
			this.InitKcp();

			ulong localRmoteConn = ((ulong) this.RemoteConn << 32) | this.LocalConn;
			idLocalRemoteConn.TryAdd(this.Id, localRmoteConn);

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
				// 200毫秒后再次update发送connect请求
				this.Service.AddToUpdateNextTime(timeNow + 300, this.Id);
			}
			catch (Exception e)
			{
				Log.Error(e);
				this.OnError(ErrorCode.ERR_SocketCantSend);
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
				// 20秒没连接上则报错
				if (timeNow - this.CreateTime > 10 * 1000)
				{
					Log.Error($"kChannel connect timeout: {this.Id} {this.RemoteConn} {timeNow} {this.CreateTime} {this.ChannelType} {this.RemoteAddress}");
					this.OnError(ErrorCode.ERR_KcpConnectTimeout);
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

			try
			{
				Kcp.KcpUpdate(this.kcp, timeNow);
			}
			catch (Exception e)
			{
				Log.Error(e);
				this.OnError(ErrorCode.ERR_SocketError);
				return;
			}

			if (this.kcp != IntPtr.Zero)
			{
				uint nextUpdateTime = Kcp.KcpCheck(this.kcp, timeNow);
				this.Service.AddToUpdateNextTime(nextUpdateTime, this.Id);
			}
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
					break;
				}

				MemoryStream ms = MessageSerializeHelper.GetStream(n);
				
				ms.SetLength(n);
				ms.Seek(0, SeekOrigin.Begin);
				byte[] buffer = ms.GetBuffer();
				int count = Kcp.KcpRecv(this.kcp, buffer, n);
				if (n != count)
				{
					break;
				}

				switch (this.Service.ServiceType)
				{
					case ServiceType.Inner:
						ms.Seek(Packet.ActorIdLength + Packet.OpcodeLength, SeekOrigin.Begin);
						break;
					case ServiceType.Outer:
						ms.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
						break;
				}
				this.lastRecvTime = this.Service.TimeNow;
				this.OnRead(ms);
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
				this.OnError(ErrorCode.ERR_SocketCantSend);
			}
		}

        private void KcpSend(KcpWaitPacket kcpWaitPacket)
		{
			if (this.IsDisposed)
			{
				return;
			}

			MemoryStream memoryStream = kcpWaitPacket.MemoryStream;
			if (this.Service.ServiceType == ServiceType.Inner)
			{
				memoryStream.GetBuffer().WriteTo(0, kcpWaitPacket.ActorId);
			}

			int count = (int) (memoryStream.Length - memoryStream.Position);
			Kcp.KcpSend(this.kcp, memoryStream.GetBuffer(), (int)memoryStream.Position, count);
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
					Log.Error($"kcp wait snd too large: {n}: {this.Id} {this.RemoteConn}");
					this.OnError(ErrorCode.ERR_KcpWaitSendSizeTooLarge);
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
		
#endregion
	}
}
