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

		public TService(ThreadSynchronizationContext threadSynchronizationContext, ServiceType serviceType)
		{
			this.ServiceType = serviceType;
			this.ThreadSynchronizationContext = threadSynchronizationContext;
		}

		public TService(ThreadSynchronizationContext threadSynchronizationContext, IPEndPoint ipEndPoint, ServiceType serviceType)
		{
			this.ServiceType = serviceType;
			this.ThreadSynchronizationContext = threadSynchronizationContext;
			
			this.acceptor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.innArgs.Completed += this.OnComplete;
			this.acceptor.Bind(ipEndPoint);
			this.acceptor.Listen(1000);
			
			this.ThreadSynchronizationContext.PostNext(this.AcceptAsync);
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

#region 网络线程

		private void OnAcceptComplete(SocketError socketError, Socket acceptSocket)
		{
			if (this.acceptor == null)
			{
				return;
			}
			
			// 开始新的accept
			this.AcceptAsync();
			
			if (socketError != SocketError.Success)
			{
				Log.Error($"accept error {socketError}");
				return;
			}

			try
			{
				long id = this.CreateAcceptChannelId(0);
				TChannel channel = new TChannel(id, acceptSocket, this);
				this.idChannels.Add(channel.Id, channel);
				long channelId = channel.Id;
				
				this.OnAccept(channelId, channel.RemoteAddress);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}			
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

		protected override void Get(long id, IPEndPoint address)
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

		protected override void Send(long channelId, long actorId, MemoryStream stream)
		{
			try
			{
				TChannel aChannel = this.Get(channelId);
				if (aChannel == null)
				{
					this.OnError(channelId, ErrorCore.ERR_SendMessageNotFoundTChannel);
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
		
#endregion
		
	}
}