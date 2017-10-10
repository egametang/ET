using System;
using System.Threading.Tasks;

namespace Model
{
	internal sealed class UPoller : IDisposable
	{
		static UPoller()
		{
			Library.Initialize();
		}

		public USocketManager USocketManager { get; }
		private readonly EQueue<IntPtr> connQueue = new EQueue<IntPtr>();

		private IntPtr host;
		
		private ENetEvent eNetEventCache;

		private TaskCompletionSource<USocket> AcceptTcs { get; set; }

		public UPoller(string hostName, ushort port)
		{
			try
			{
				this.USocketManager = new USocketManager();

				UAddress address = new UAddress(hostName, port);
				ENetAddress nativeAddress = address.Struct;
				this.host = NativeMethods.enet_host_create(ref nativeAddress,
						NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

				if (this.host == IntPtr.Zero)
				{
					throw new Exception("Host creation call failed.");
				}

				NativeMethods.enet_host_compress_with_range_coder(this.host);
			}
			catch (Exception e)
			{
				throw new Exception($"UPoll construct error, address: {hostName}:{port}", e);
			}
		}

		public UPoller()
		{
			this.USocketManager = new USocketManager();

			this.host = NativeMethods.enet_host_create(IntPtr.Zero, NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

			if (this.host == IntPtr.Zero)
			{
				throw new Exception("Host creation call failed.");
			}

			NativeMethods.enet_host_compress_with_range_coder(this.host);
		}

		public void Dispose()
		{
			if (this.host == IntPtr.Zero)
			{
				return;
			}

			NativeMethods.enet_host_destroy(this.host);

			this.host = IntPtr.Zero;
		}

		public IntPtr Host
		{
			get
			{
				return this.host;
			}
		}

		public void Flush()
		{
			NativeMethods.enet_host_flush(this.host);
		}

		public Task<USocket> AcceptAsync()
		{
			if (this.AcceptTcs != null)
			{
				throw new Exception("do not accept twice!");
			}
			
			// 如果有请求连接缓存的包,从缓存中取
			if (this.connQueue.Count > 0)
			{
				IntPtr ptr = this.connQueue.Dequeue();

				USocket socket = new USocket(ptr, this);
				this.USocketManager.Add(ptr, socket);
				return Task.FromResult(socket);
			}

			this.AcceptTcs = new TaskCompletionSource<USocket>();
			return this.AcceptTcs.Task;
		}

		private void OnAccepted(ENetEvent eEvent)
		{
			if (eEvent.Type == EventType.Disconnect)
			{
				this.AcceptTcs.TrySetException(new Exception("socket disconnected in accpet"));
			}

			USocket socket = new USocket(eEvent.Peer, this);
			this.USocketManager.Add(socket.PeerPtr, socket);
			socket.OnAccepted();

			var tcs = this.AcceptTcs;
			this.AcceptTcs = null;
			tcs.SetResult(socket);
		}

		private int Service()
		{
			int ret = NativeMethods.enet_host_service(this.host, IntPtr.Zero, 0);
			return ret;
		}

		public void Update()
		{
			if (this.Service() < 0)
			{
				return;
			}

			while (true)
			{
				if (NativeMethods.enet_host_check_events(this.host, ref this.eNetEventCache) <= 0)
				{
					return;
				}

				switch (this.eNetEventCache.Type)
				{
					case EventType.Connect:
						{
							// 这是一个connect peer
							if (this.USocketManager.ContainsKey(this.eNetEventCache.Peer))
							{
								USocket uSocket = this.USocketManager[this.eNetEventCache.Peer];
								uSocket.OnConnected();
								break;
							}

							// 这是accept peer
							if (this.AcceptTcs != null)
							{
								this.OnAccepted(this.eNetEventCache);
								break;
							}

							// 如果server端没有acceptasync,则请求放入队列
							this.connQueue.Enqueue(this.eNetEventCache.Peer);
							break;
						}
					case EventType.Receive:
						{
							USocket uSocket = this.USocketManager[this.eNetEventCache.Peer];
							uSocket.OnReceived(this.eNetEventCache);
							break;
						}
					case EventType.Disconnect:
						{
							USocket uSocket = this.USocketManager[this.eNetEventCache.Peer];
							this.USocketManager.Remove(uSocket.PeerPtr);
							uSocket.PeerPtr = IntPtr.Zero;
							uSocket.OnDisconnect(this.eNetEventCache);
							break;
						}
				}
			}
		}
	}
}