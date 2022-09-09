using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ET
{
	public sealed class TService : AService
	{
		private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();

		private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
		
		private Socket acceptor;

		public HashSet<long> NeedStartSend = new HashSet<long>();

		public ThreadSynchronizationContext ThreadSynchronizationContext;

		public TService(AddressFamily addressFamily, ServiceType serviceType)
		{
			this.ServiceType = serviceType;
			this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
		}

		public TService(IPEndPoint ipEndPoint, ServiceType serviceType)
		{
			this.ServiceType = serviceType;
			this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
			
			this.acceptor = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			// 容易出问题，先注释掉，按需开启
			//this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.innArgs.Completed += this.OnComplete;
			try
			{
				this.acceptor.Bind(ipEndPoint);
			}
			catch (Exception e)
			{
				throw new Exception($"bind error: {ipEndPoint}", e);
			}
			
			this.acceptor.Listen(1000);
			
			this.ThreadSynchronizationContext.Post(this.AcceptAsync);
		}

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					SocketError socketError = e.SocketError;
					Socket acceptSocket = e.AcceptSocket;
					this.ThreadSynchronizationContext.Post(()=>{this.OnAcceptComplete(socketError, acceptSocket);});
					break;
				default:
					throw new Exception($"socket error: {e.LastOperation}");
			}
		}

		private void OnAcceptComplete(SocketError socketError, Socket acceptSocket)
		{
			if (this.acceptor == null)
			{
				return;
			}

			if (socketError != SocketError.Success)
			{
				Log.Error($"accept error {socketError}");
				return;
			}

			try
			{
				long id = NetServices.Instance.CreateAcceptChannelId(0);
				TChannel channel = new TChannel(id, acceptSocket, this);
				this.idChannels.Add(channel.Id, channel);
				long channelId = channel.Id;
				
				NetServices.Instance.OnAccept(this.Id, channelId, channel.RemoteAddress);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}		
			
			// 开始新的accept
			this.AcceptAsync();
		}
		
		private void AcceptAsync()
		{
			this.innArgs.AcceptSocket = null;
			if (this.acceptor.AcceptAsync(this.innArgs))
			{
				return;
			}
			OnAcceptComplete(this.innArgs.SocketError, this.innArgs.AcceptSocket);
		}

		private TChannel Create(IPEndPoint ipEndPoint, long id)
		{
			TChannel channel = new TChannel(id, ipEndPoint, this);
			this.idChannels.Add(channel.Id, channel);
			return channel;
		}

		public override void Create(long id, IPEndPoint address)
		{
			if (this.idChannels.TryGetValue(id, out TChannel _))
			{
				return;
			}
			this.Create(address, id);
		}
		
		private TChannel Get(long id)
		{
			TChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}
		
		public override void Dispose()
		{
			base.Dispose();
			
			this.acceptor?.Close();
			this.acceptor = null;
			this.innArgs.Dispose();
			ThreadSynchronizationContext = null;
			
			foreach (long id in this.idChannels.Keys.ToArray())
			{
				TChannel channel = this.idChannels[id];
				channel.Dispose();
			}
			this.idChannels.Clear();
		}

		public override void Remove(long id)
		{
			if (this.idChannels.TryGetValue(id, out TChannel channel))
			{
				channel.Dispose();	
			}

			this.idChannels.Remove(id);
		}

		public override void Send(long channelId, long actorId, MemoryStream stream)
		{
			try
			{
				TChannel aChannel = this.Get(channelId);
				if (aChannel == null)
				{
					NetServices.Instance.OnError(this.Id, channelId, ErrorCore.ERR_SendMessageNotFoundTChannel);
					return;
				}
				aChannel.Send(actorId, stream);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		
		public override void Update()
		{
			this.ThreadSynchronizationContext.Update();
			
			foreach (long channelId in this.NeedStartSend)
			{
				TChannel tChannel = this.Get(channelId);
				tChannel?.Update();
			}
			this.NeedStartSend.Clear();
		}
		
		public override bool IsDispose()
		{
			return this.ThreadSynchronizationContext == null;
		}
	}
}