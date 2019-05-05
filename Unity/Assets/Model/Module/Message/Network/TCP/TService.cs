using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;

namespace ETModel
{
	public sealed class TService : AService
	{
		private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();

		private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
		private Socket acceptor;
		
		public RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();
		
		public HashSet<long> needStartSendChannel = new HashSet<long>();
		
		public int PacketSizeLength { get; }
		
		/// <summary>
		/// 即可做client也可做server
		/// </summary>
		public TService(int packetSizeLength, IPEndPoint ipEndPoint, Action<AChannel> acceptCallback)
		{
			this.PacketSizeLength = packetSizeLength;
			this.AcceptCallback += acceptCallback;
			
			this.acceptor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.innArgs.Completed += this.OnComplete;
			
			this.acceptor.Bind(ipEndPoint);
			this.acceptor.Listen(1000);

			this.AcceptAsync();
		}

		public TService(int packetSizeLength)
		{
			this.PacketSizeLength = packetSizeLength;
		}
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (long id in this.idChannels.Keys.ToArray())
			{
				TChannel channel = this.idChannels[id];
				channel.Dispose();
			}
			this.acceptor?.Close();
			this.acceptor = null;
			this.innArgs.Dispose();
		}

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					OneThreadSynchronizationContext.Instance.Post(this.OnAcceptComplete, e);
					break;
				default:
					throw new Exception($"socket accept error: {e.LastOperation}");
			}
		}
		
		public void AcceptAsync()
		{
			this.innArgs.AcceptSocket = null;
			if (this.acceptor.AcceptAsync(this.innArgs))
			{
				return;
			}
			OnAcceptComplete(this.innArgs);
		}

		private void OnAcceptComplete(object o)
		{
			if (this.acceptor == null)
			{
				return;
			}
			SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;
			
			if (e.SocketError != SocketError.Success)
			{
				Log.Error($"accept error {e.SocketError}");
				this.AcceptAsync();
				return;
			}
			TChannel channel = new TChannel(e.AcceptSocket, this);
			this.idChannels[channel.Id] = channel;

			try
			{
				this.OnAccept(channel);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}

			if (this.acceptor == null)
			{
				return;
			}
			
			this.AcceptAsync();
		}
		
		public override AChannel GetChannel(long id)
		{
			TChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}

		public override AChannel ConnectChannel(IPEndPoint ipEndPoint)
		{
			TChannel channel = new TChannel(ipEndPoint, this);
			this.idChannels[channel.Id] = channel;

			return channel;
		}

		public override AChannel ConnectChannel(string address)
		{
			IPEndPoint ipEndPoint = NetworkHelper.ToIPEndPoint(address);
			return this.ConnectChannel(ipEndPoint);
		}

		public void MarkNeedStartSend(long id)
		{
			this.needStartSendChannel.Add(id);
		}

		public override void Remove(long id)
		{
			TChannel channel;
			if (!this.idChannels.TryGetValue(id, out channel))
			{
				return;
			}
			if (channel == null)
			{
				return;
			}
			this.idChannels.Remove(id);
			channel.Dispose();
		}

		public override void Update()
		{
			foreach (long id in this.needStartSendChannel)
			{
				TChannel channel;
				if (!this.idChannels.TryGetValue(id, out channel))
				{
					continue;
				}

				if (channel.IsSending)
				{
					continue;
				}

				try
				{
					channel.StartSend();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
			
			this.needStartSendChannel.Clear();
		}
	}
}