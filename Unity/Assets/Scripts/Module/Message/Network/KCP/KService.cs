using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

		private UdpClient socket;

		private readonly Dictionary<long, KChannel> idChannels = new Dictionary<long, KChannel>();

		private TaskCompletionSource<AChannel> acceptTcs;

		private readonly Queue<long> removedChannels = new Queue<long>();

		// 下帧要更新的channel
		private readonly HashSet<long> updateChannels = new HashSet<long>();

		// 下次时间更新的channel
		private readonly MultiMap<long, long> timerId = new MultiMap<long, long>();

		private readonly List<long> timeOutId = new List<long>();

		public KService(IPEndPoint ipEndPoint)
		{
			this.TimeNow = (uint)TimeHelper.Now();
			this.socket = new UdpClient(ipEndPoint);

#if SERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				const uint IOC_IN = 0x80000000;
				const uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				this.socket.Client.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
			}
#endif

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

			this.socket.Close();
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
					Log.Error(e);
					continue;
				}

				try
				{
					int messageLength = udpReceiveResult.Buffer.Length;

					// 长度小于4，不是正常的消息
					if (messageLength < 4)
					{
						continue;
					}

					// accept
					uint conn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 0);

					// conn从1000开始，如果为1，2，3则是特殊包
					switch (conn)
					{
						case KcpProtocalType.SYN:
							// 长度!=8，不是accpet消息
							if (messageLength != 8)
							{
								break;
							}
							this.HandleAccept(udpReceiveResult);
							break;
						case KcpProtocalType.ACK:
							// 长度!=12，不是connect消息
							if (messageLength != 12)
							{
								break;
							}
							this.HandleConnect(udpReceiveResult);
							break;
						case KcpProtocalType.FIN:
							// 长度!=12，不是DisConnect消息
							if (messageLength != 12)
							{
								break;
							}
							this.HandleDisConnect(udpReceiveResult);
							break;
						default:
							this.HandleRecv(udpReceiveResult, conn);
							break;
					}
				}
				catch (Exception e)
				{
					Log.Error(e);
					continue;
				}
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

		private KChannel CreateConnectChannel(IPEndPoint remoteEndPoint)
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