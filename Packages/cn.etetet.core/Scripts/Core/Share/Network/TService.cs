using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ET
{
	public enum TcpOp
	{
		StartSend,
		StartRecv,
		Connect,
	}
	
	public struct TArgs
	{
		public TcpOp Op;
		public long ChannelId;
		public SocketAsyncEventArgs SocketAsyncEventArgs;
	}
	
	public sealed class TService : AService
	{
		private readonly Dictionary<long, TChannel> idChannels = new();

		private readonly SocketAsyncEventArgs innArgs = new();
		
		private Socket acceptor;

		public ConcurrentQueue<TArgs> Queue = new();

		public TService(AddressFamily addressFamily, ServiceType serviceType)
		{
			this.ServiceType = serviceType;
		}

		public TService(IPEndPoint ipEndPoint, ServiceType serviceType)
		{
			this.ServiceType = serviceType;
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
			
			this.AcceptAsync();
		}

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					this.Queue.Enqueue(new TArgs() {SocketAsyncEventArgs = e});
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
				this.AcceptAsync();
				return;
			}

			try
			{
				long id = NetServices.Instance.CreateAcceptChannelId();
				TChannel channel = new TChannel(id, acceptSocket, this);
				this.idChannels.Add(channel.Id, channel);
				long channelId = channel.Id;
				
				this.AcceptCallback(channelId, channel.RemoteAddress);
			}
			catch (Exception e)
			{
				Log.Error(e);
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

		public override void Create(long id, IPEndPoint ipEndPoint)
		{
			if (this.idChannels.TryGetValue(id, out TChannel _))
			{
				return;
			}

			TChannel channel = new(id, ipEndPoint, this);
			this.idChannels.Add(channel.Id, channel);
		}

		public TChannel Get(long id)
		{
			TChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}
		
		public override void Dispose()
		{
			if (this.IsDisposed())
			{
				return;
			}
			
			base.Dispose();
			
			this.acceptor?.Close();
			this.acceptor = null;
			this.innArgs.Dispose();
			
			foreach (long id in this.idChannels.Keys.ToArray())
			{
				TChannel channel = this.idChannels[id];
				channel.Dispose();
			}
		}

		public override void Remove(long id, int error = 0)
		{
			if (this.idChannels.TryGetValue(id, out TChannel channel))
			{
				channel.Error = error;
				channel.Dispose();	
			}
			this.idChannels.Remove(id);
		}

		public override void Send(long channelId, MemoryBuffer memoryBuffer)
		{
			try
			{
				TChannel aChannel = this.Get(channelId);
				if (aChannel == null)
				{
					this.ErrorCallback(channelId, ErrorCore.ERR_SendMessageNotFoundTChannel);
					return;
				}
				
				aChannel.Send(memoryBuffer);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		
		public override void Update()
		{
			while (true)
			{
				if (!this.Queue.TryDequeue(out var result))
				{
					break;
				}
				
				SocketAsyncEventArgs e = result.SocketAsyncEventArgs;
				if (e == null)
				{
					switch (result.Op)
					{
						case TcpOp.StartSend:
						{
							TChannel tChannel = this.Get(result.ChannelId);
							if (tChannel != null)
							{
								tChannel.StartSend();
							}
							break;
						}
						case TcpOp.StartRecv:
						{
							TChannel tChannel = this.Get(result.ChannelId);
							if (tChannel != null)
							{
								tChannel.StartRecv();
							}
							break;
						}
						case TcpOp.Connect:
						{
							TChannel tChannel = this.Get(result.ChannelId);
							if (tChannel != null)
							{
								tChannel.ConnectAsync();
							}
							break;
						}
					}
					continue;
				}

				switch (e.LastOperation)
				{
					case SocketAsyncOperation.Accept:
					{
						SocketError socketError = e.SocketError;
						Socket acceptSocket = e.AcceptSocket;
						this.OnAcceptComplete(socketError, acceptSocket);
						break;
					}
					case SocketAsyncOperation.Connect:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnConnectComplete(e);
						}

						break;
					}
					case SocketAsyncOperation.Disconnect:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnDisconnectComplete(e);
						}
						break;
					}
					case SocketAsyncOperation.Receive:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnRecvComplete(e);
						}
						break;
					}
					case SocketAsyncOperation.Send:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnSendComplete(e);
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException($"{e.LastOperation}");
				}
			}
		}
	}
}