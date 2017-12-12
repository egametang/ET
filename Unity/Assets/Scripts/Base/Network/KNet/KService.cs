﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Model
{
	public static class KcpProtocalType
	{
		public const uint SYN = 1;
		public const uint ACK = 2;
		public const uint FIN = 3;
	}

	public sealed class KService: AService
	{
		private uint IdGenerater = 1000;
		private uint IdAccept = 2000000000;

		public uint TimeNow { get; set; }

		private UdpClient socket;
		
		private readonly Dictionary<long, KChannel> idChannels = new Dictionary<long, KChannel>();

		private TaskCompletionSource<AChannel> acceptTcs;

		private readonly Queue<long> removedChannels = new Queue<long>();

		// 下帧要更新的channel
		private readonly HashSet<long> updateChannels = new HashSet<long>();

		// 下次时间更新的channel
		private readonly MultiMap<long, long> timerMap = new MultiMap<long, long>();

		public KService(IPEndPoint ipEndPoint)
		{
			this.TimeNow = (uint)TimeHelper.Now();
			this.socket = new UdpClient(ipEndPoint);

			uint IOC_IN = 0x80000000;
			uint IOC_VENDOR = 0x18000000;
			uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
			this.socket.Client.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);

			this.StartRecv();
		}

		public KService()
		{
			this.TimeNow = (uint)TimeHelper.Now();
			this.socket = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
			this.StartRecv();
		}

		public override void Dispose()
		{
			if (this.socket == null)
			{
				return;
			}

			this.socket.Dispose();
			this.socket = null;
		}

		public async void StartRecv()
		{
			while (true)
			{
				if (this.socket == null)
				{
					return;
				}

				UdpReceiveResult udpReceiveResult;
				try
				{
					udpReceiveResult = await this.socket.ReceiveAsync();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
					continue;
				}

				// accept
				uint conn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 0);
				
				// conn从1000开始，如果为1，2则是特殊包
				if (conn == KcpProtocalType.SYN)
				{
					this.HandleAccept(udpReceiveResult);
					continue;
				}
				
				// connect response
				if (conn == KcpProtocalType.ACK)
				{
					this.HandleConnect(udpReceiveResult);
					continue;
				}

				if (conn == KcpProtocalType.FIN)
				{
					this.HandleDisConnect(udpReceiveResult);
					continue;
				}

				this.HandleRecv(udpReceiveResult, conn);
			}
		}

		private void HandleConnect(UdpReceiveResult udpReceiveResult)
		{
			uint requestConn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 4);
			uint responseConn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 8);
			
			KChannel kChannel;
			if (!this.idChannels.TryGetValue(requestConn, out kChannel))
			{
				return;
			}
			// 处理chanel
			kChannel.HandleConnnect(responseConn);
		}

		private void HandleDisConnect(UdpReceiveResult udpReceiveResult)
		{
			uint requestConn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 8);
			
			KChannel kChannel;
			if (!this.idChannels.TryGetValue(requestConn, out kChannel))
			{
				return;
			}
			// 处理chanel
			this.idChannels.Remove(requestConn);
			kChannel.Dispose();
		}

		private void HandleRecv(UdpReceiveResult udpReceiveResult, uint conn)
		{
			KChannel kChannel;
			if (!this.idChannels.TryGetValue(conn, out kChannel))
			{
				return;
			}
			// 处理chanel
			kChannel.HandleRecv(udpReceiveResult.Buffer, this.TimeNow);
		}

		private void HandleAccept(UdpReceiveResult udpReceiveResult)
		{
			if (this.acceptTcs == null)
			{
				return;
			}

			uint requestConn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 4);
			
			// 如果已经连接上,则重新响应请求
			KChannel kChannel;
			if (this.idChannels.TryGetValue(requestConn, out kChannel))
			{
				kChannel.HandleAccept(requestConn);
				return;
			}
			
			TaskCompletionSource<AChannel> t = this.acceptTcs;
			this.acceptTcs = null;
			kChannel = this.CreateAcceptChannel(udpReceiveResult.RemoteEndPoint, requestConn);
			kChannel.HandleAccept(requestConn);
			t.SetResult(kChannel);
		}

		private KChannel CreateAcceptChannel(IPEndPoint remoteEndPoint, uint remoteConn)
		{
			KChannel channel = new KChannel(--this.IdAccept, remoteConn, this.socket, remoteEndPoint, this);
			KChannel oldChannel;
			if (this.idChannels.TryGetValue(channel.Id, out oldChannel))
			{
				this.idChannels.Remove(oldChannel.Id);
				oldChannel.Dispose();
			}
			this.idChannels[channel.Id] = channel;
			return channel;
		}
		
		private KChannel CreateConnectChannel(IPEndPoint remoteEndPoint)
		{
			KChannel channel = new KChannel(++this.IdGenerater, this.socket, remoteEndPoint, this);
			KChannel oldChannel;
			if (this.idChannels.TryGetValue(channel.Id, out oldChannel))
			{
				this.idChannels.Remove(oldChannel.Id);
				oldChannel.Dispose();
			}
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public void AddToUpdate(long id)
		{
			this.updateChannels.Add(id);
		}

		public void AddToNextTimeUpdate(long time, long id)
		{
			this.timerMap.Add(time, id);
		}
		
		public override AChannel GetChannel(long id)
		{
			KChannel channel;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}

		public override Task<AChannel> AcceptChannel()
		{
			acceptTcs = new TaskCompletionSource<AChannel>();
			return this.acceptTcs.Task;
		}

		public override AChannel ConnectChannel(IPEndPoint ipEndPoint)
		{
			KChannel channel = this.CreateConnectChannel(ipEndPoint);
			return channel;
		}


		public override void Remove(long id)
		{
			KChannel channel;
			if (!this.idChannels.TryGetValue(id, out channel))
			{
				return;
			}
			if (channel == null)
			{
				return;
			}
			this.removedChannels.Enqueue(id);
			channel.Dispose();
		}
		
		public override void Update()
		{
			this.TimeNow = (uint)TimeHelper.Now();

			while (true)
			{
				if (this.timerMap.Count <= 0)
				{
					break;
				}
				var kv = this.timerMap.First();
				if (kv.Key > TimeNow)
				{
					break;
				}
				List<long> timeOutId = kv.Value;
				foreach (long id in timeOutId)
				{
					this.updateChannels.Add(id);
				}
				this.timerMap.Remove(kv.Key);
			}
			foreach (long id in updateChannels)
			{
				KChannel kChannel;
				if (!this.idChannels.TryGetValue(id, out kChannel))
				{
					continue;
				}
				if (kChannel.Id == 0)
				{
					continue;
				}
				kChannel.Update(this.TimeNow);
			}
			this.updateChannels.Clear();

			while (true)
			{
				if (this.removedChannels.Count <= 0)
				{
					break;
				}
				long id = this.removedChannels.Dequeue();
				this.idChannels.Remove(id);
			}
		}
	}
}