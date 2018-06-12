using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ETModel
{
	public sealed class TService : AService
	{
		private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();

		private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
		private Socket acceptor;
		
		/// <summary>
		/// 即可做client也可做server
		/// </summary>
		public TService(IPEndPoint ipEndPoint)
		{
			this.acceptor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.innArgs.Completed += this.OnAcceptComplete;
			this.innArgs.UserToken = new UserTokenInfo() { InstanceId = this.InstanceId };
			
			this.acceptor.Bind(ipEndPoint);
			this.acceptor.Listen(1000);
		}

		public TService()
		{
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

		public override void Start()
		{
			if (this.acceptor != null)
			{
				this.AcceptAsync();
			}
		}
		
		public void AcceptAsync()
		{
			this.innArgs.AcceptSocket = null;
			if (this.acceptor.AcceptAsync(this.innArgs))
			{
				return;
			}
			OnAcceptComplete(this, this.innArgs);
		}

		private void OnAcceptComplete(object sender, SocketAsyncEventArgs o)
		{
			SocketAsyncEventArgs e = o;
			UserTokenInfo userTokenInfo = (UserTokenInfo) e.UserToken;
			if (userTokenInfo.InstanceId != this.InstanceId)
			{
				return;
			}
			
			if (e.SocketError != SocketError.Success)
			{
				Log.Error($"accept error {e.SocketError}");
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

			if (userTokenInfo.InstanceId != this.InstanceId)
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
		}
	}
}