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
		public static KService Instance { get; private set; }

		private uint IdGenerater = 1000;

		public uint TimeNow { get; private set; }

		private Socket socket;

		private readonly Dictionary<long, KChannel> localConnChannels = new Dictionary<long, KChannel>();

		private readonly Dictionary<uint, KChannel> waitConnectChannels = new Dictionary<uint, KChannel>();

		private readonly byte[] cache = new byte[8192];

		private readonly Queue<long> removedChannels = new Queue<long>();

		// 下帧要更新的channel
		private readonly HashSet<long> updateChannels = new HashSet<long>();

		// 下次时间更新的channel
		private readonly MultiMap<long, long> timeId = new MultiMap<long, long>();

		private readonly List<long> timeOutTime = new List<long>();

		// 记录最小时间，不用每次都去MultiMap取第一个值
		private long minTime;

		private EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

		public KService(IPEndPoint ipEndPoint)
		{
			this.TimeNow = (uint)TimeHelper.ClientNow();
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//this.socket.Blocking = false;
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
			Instance = this;
		}

		public KService()
		{
			this.TimeNow = (uint)TimeHelper.ClientNow();
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//this.socket.Blocking = false;
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
			Instance = this;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (KeyValuePair<long, KChannel> keyValuePair in this.localConnChannels)
			{
				keyValuePair.Value.Dispose();
			}

			this.socket.Close();
			this.socket = null;
			Instance = null;
		}

		public override void Start()
		{
		}

		public void Recv()
		{
			if (this.socket == null)
			{
				return;
			}

			while (socket != null && this.socket.Available > 0)
			{
				int messageLength = 0;
				try
				{
					messageLength = this.socket.ReceiveFrom(this.cache, ref this.ipEndPoint);
				}
				catch (Exception e)
				{
					Log.Error(e);
					continue;
				}

				// 长度小于4，不是正常的消息
				if (messageLength < 4)
				{
					continue;
				}
				// accept
				uint conn = BitConverter.ToUInt32(this.cache, 0);

				// conn从1000开始，如果为1，2，3则是特殊包
				uint remoteConn = 0;
				uint localConn = 0;
				KChannel kChannel = null;
				switch (conn)
				{
					case KcpProtocalType.SYN:  // accept
											   // 长度!=8，不是accpet消息
						if (messageLength != 8)
						{
							break;
						}

						IPEndPoint acceptIpEndPoint = (IPEndPoint)this.ipEndPoint;
						this.ipEndPoint = new IPEndPoint(0, 0);

						remoteConn = BitConverter.ToUInt32(this.cache, 4);

						// 如果等待连接状态,则重新响应请求
						if (this.waitConnectChannels.TryGetValue(remoteConn, out kChannel))
						{
							kChannel.HandleAccept(remoteConn);
							break;
						}

						localConn = ++this.IdGenerater;
						kChannel = new KChannel(localConn, remoteConn, this.socket, acceptIpEndPoint, this);
						this.localConnChannels[kChannel.Id] = kChannel;
						this.waitConnectChannels[remoteConn] = kChannel;

						kChannel.HandleAccept(remoteConn);

						this.OnAccept(kChannel);

						break;
					case KcpProtocalType.ACK:  // connect返回
											   // 长度!=12，不是connect消息
						if (messageLength != 12)
						{
							break;
						}
						remoteConn = BitConverter.ToUInt32(this.cache, 4);
						localConn = BitConverter.ToUInt32(this.cache, 8);

						kChannel = this.GetKChannel(localConn);
						if (kChannel != null)
						{
							kChannel.HandleConnnect(remoteConn);
						}
						break;
					case KcpProtocalType.FIN:  // 断开
											   // 长度!=12，不是DisConnect消息
						if (messageLength != 16)
						{
							break;
						}

						remoteConn = BitConverter.ToUInt32(this.cache, 4);
						localConn = BitConverter.ToUInt32(this.cache, 8);

						// 处理chanel
						kChannel = this.GetKChannel(localConn);
						if (kChannel != null)
						{
							if (kChannel.RemoteConn == remoteConn)
							{
								kChannel.Disconnect(ErrorCode.ERR_PeerDisconnect);
							}
						}
						break;
					default:  // 接收
							  // 处理chanel
						localConn = conn;
						kChannel = this.GetKChannel(localConn);
						if (kChannel != null)
						{
							kChannel.HandleRecv(this.cache, messageLength);
						}
						break;
				}
			}
		}

		public KChannel GetKChannel(long id)
		{
			AChannel aChannel = this.GetChannel(id);
			if (aChannel == null)
			{
				return null;
			}

			return (KChannel)aChannel;
		}

		public void RemoveFromWaitConnectChannels(uint remoteConn)
		{
			this.waitConnectChannels.Remove(remoteConn);
		}

		public override AChannel GetChannel(long id)
		{
			if (this.removedChannels.Contains(id))
			{
				return null;
			}
			KChannel channel;
			this.localConnChannels.TryGetValue(id, out channel);
			return channel;
		}

		public static void Output(IntPtr bytes, int count, IntPtr user)
		{
			if (Instance == null)
			{
				return;
			}
			AChannel aChannel = Instance.GetChannel((uint)user);
			if (aChannel == null)
			{
				Log.Error($"not found kchannel, {(uint)user}");
				return;
			}

			KChannel kChannel = aChannel as KChannel;
			kChannel.Output(bytes, count);
		}

		public override AChannel ConnectChannel(IPEndPoint remoteEndPoint)
		{
			uint localConn = (uint)RandomHelper.RandomNumber(1000, int.MaxValue);
			KChannel oldChannel;
			if (this.localConnChannels.TryGetValue(localConn, out oldChannel))
			{
				this.localConnChannels.Remove(oldChannel.LocalConn);
				oldChannel.Dispose();
			}

			KChannel channel = new KChannel(localConn, this.socket, remoteEndPoint, this);
			this.localConnChannels[channel.LocalConn] = channel;
			return channel;
		}

		public void AddToUpdateNextTime(long time, long id)
		{
			if (time == 0)
			{
				this.updateChannels.Add(id);
				return;
			}
			if (time < this.minTime)
			{
				this.minTime = time;
			}
			this.timeId.Add(time, id);
		}

		public override void Remove(long id)
		{
			KChannel channel;
			if (!this.localConnChannels.TryGetValue(id, out channel))
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
			this.TimeNow = (uint)TimeHelper.ClientNow();

			this.Recv();

			this.TimerOut();

			foreach (long id in updateChannels)
			{
				KChannel kChannel = this.GetKChannel(id);
				if (kChannel == null)
				{
					continue;
				}
				if (kChannel.Id == 0)
				{
					continue;
				}
				kChannel.Update();
			}
			this.updateChannels.Clear();

			while (true)
			{
				if (this.removedChannels.Count <= 0)
				{
					break;
				}
				long id = this.removedChannels.Dequeue();
				this.localConnChannels.Remove(id);
			}
		}

		// 计算到期需要update的channel
		private void TimerOut()
		{
			if (this.timeId.Count == 0)
			{
				return;
			}

			uint timeNow = this.TimeNow;

			if (timeNow < this.minTime)
			{
				return;
			}

			this.timeOutTime.Clear();

			foreach (KeyValuePair<long, List<long>> kv in this.timeId.GetDictionary())
			{
				long k = kv.Key;
				if (k > timeNow)
				{
					minTime = k;
					break;
				}
				this.timeOutTime.Add(k);
			}

			foreach (long k in this.timeOutTime)
			{
				foreach (long v in this.timeId[k])
				{
					this.updateChannels.Add(v);
				}

				this.timeId.Remove(k);
			}
		}
	}
}