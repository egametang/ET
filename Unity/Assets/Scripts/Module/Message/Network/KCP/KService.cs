using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ETModel
{
	public static class KcpProtocalType
	{
		public const uint SYN = 1;
		public const uint ACK = 2;
		public const uint FIN = 3;
	}

	public sealed class KService : AService
	{
		private uint IdGenerater = 1000;

		public uint TimeNow { get; set; }

		private Socket socket;

		private readonly Dictionary<long, KChannel> idChannels = new Dictionary<long, KChannel>();
		
		private readonly byte[] cache = new byte[8192];

		private readonly Queue<long> removedChannels = new Queue<long>();

		// 下帧要更新的channel
		private readonly HashSet<long> updateChannels = new HashSet<long>();

		// 下次时间更新的channel
		private readonly MultiMap<long, long> timerId = new MultiMap<long, long>();

		private readonly List<long> timeOutId = new List<long>();
		
		private EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

		public KService(IPEndPoint ipEndPoint)
		{
			this.TimeNow = (uint)TimeHelper.ClientNow();
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			this.socket.Bind(ipEndPoint);
#if SERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				const uint IOC_IN = 0x80000000;
				const uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				this.socket.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
			}
#endif
		}

		public KService()
		{
			this.TimeNow = (uint)TimeHelper.ClientNow();
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			this.socket.Bind(new IPEndPoint(IPAddress.Any, 0));
#if SERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				const uint IOC_IN = 0x80000000;
				const uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				this.socket.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
			}
#endif
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (KeyValuePair<long,KChannel> keyValuePair in this.idChannels)
			{
				keyValuePair.Value.Dispose();
			}

			this.socket.Close();
			this.socket = null;
		}

		public override void Start()
		{
		}

		public void Recv()
		{
			while (this.socket.Available > 0)
			{
				if (this.IsDisposed)
				{
					return;
				}

				int messageLength = 0;
				try
				{
					messageLength = this.socket.ReceiveFrom(this.cache, ref this.ipEndPoint);
				}
				catch (Exception)
				{
					return;
				}

				// 长度小于4，不是正常的消息
				if (messageLength < 4)
				{
					continue;
				}

				// accept
				uint conn = BitConverter.ToUInt32(this.cache, 0);

				// conn从1000开始，如果为1，2，3则是特殊包
				switch (conn)
				{
					case KcpProtocalType.SYN:
						// 长度!=8，不是accpet消息
						if (messageLength != 8)
						{
							break;
						}

						IPEndPoint acceptIpEndPoint = (IPEndPoint)this.ipEndPoint;
						this.ipEndPoint = new IPEndPoint(0, 0);
						this.HandleAccept(this.cache, acceptIpEndPoint);
						break;
					case KcpProtocalType.ACK:
						// 长度!=12，不是connect消息
						if (messageLength != 12)
						{
							break;
						}
						this.HandleConnect(this.cache);
						break;
					case KcpProtocalType.FIN:
						// 长度!=12，不是DisConnect消息
						if (messageLength != 12)
						{
							break;
						}
						this.HandleDisConnect(this.cache);
						break;
					default:
						this.HandleRecv(this.cache, messageLength, conn);
						break;
				}
			}
		}

		private void HandleConnect(byte[] bytes)
		{
			uint requestConn = BitConverter.ToUInt32(bytes, 4);
			uint responseConn = BitConverter.ToUInt32(bytes, 8);

			KChannel kChannel;
			if (!this.idChannels.TryGetValue(requestConn, out kChannel))
			{
				return;
			}

			// 处理chanel
			kChannel.HandleConnnect(responseConn);
		}

		private void HandleDisConnect(byte[] bytes)
		{
			uint requestConn = BitConverter.ToUInt32(bytes, 8);

			KChannel kChannel;
			if (!this.idChannels.TryGetValue(requestConn, out kChannel))
			{
				return;
			}

			// 处理chanel
			this.idChannels.Remove(requestConn);
			kChannel.Dispose();
		}

		private void HandleRecv(byte[] bytes, int length, uint conn)
		{
			KChannel kChannel;
			if (!this.idChannels.TryGetValue(conn, out kChannel))
			{
				return;
			}

			// 处理chanel
			kChannel.HandleRecv(bytes, length, this.TimeNow);
		}

		private void HandleAccept(byte[] bytes, IPEndPoint remoteEndPoint)
		{
			uint requestConn = BitConverter.ToUInt32(bytes, 4);

			// 如果已经连接上,则重新响应请求
			KChannel kChannel;
			if (this.idChannels.TryGetValue(requestConn, out kChannel))
			{
				kChannel.HandleAccept(requestConn);
				return;
			}

			kChannel = this.CreateAcceptChannel(remoteEndPoint, requestConn);
			kChannel.HandleAccept(requestConn);
			this.OnAccept(kChannel);
		}

		private KChannel CreateAcceptChannel(IPEndPoint remoteEndPoint, uint remoteConn)
		{
			KChannel channel = new KChannel(++this.IdGenerater, remoteConn, this.socket, remoteEndPoint, this);
			KChannel oldChannel;
			if (this.idChannels.TryGetValue(channel.Id, out oldChannel))
			{
				this.idChannels.Remove(oldChannel.Id);
				oldChannel.Dispose();
			}
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public override AChannel GetChannel(long id)
		{
			KChannel channel;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}

		public override AChannel ConnectChannel(IPEndPoint remoteEndPoint)
		{
			uint conv = (uint)RandomHelper.RandomNumber(1000, int.MaxValue);
			KChannel channel = new KChannel(conv, this.socket, remoteEndPoint, this);
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
			this.timerId.Add(time, id);
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
			this.Recv();
			
			this.TimerOut();

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

		// 计算到期需要update的channel
		private void TimerOut()
		{
			if (this.timerId.Count == 0)
			{
				return;
			}

			this.TimeNow = (uint)TimeHelper.ClientNow();
			
			timeOutId.Clear();

			while (this.timerId.Count > 0)
			{
				long k = this.timerId.FirstKey();
				if (k > this.TimeNow)
				{
					break;
				}
				foreach (long ll in this.timerId[k])
				{
					this.timeOutId.Add(ll);
				}
				this.timerId.Remove(k);
			}

			foreach (long k in this.timeOutId)
			{
				this.updateChannels.Add(k);
			}
		}
	}
}