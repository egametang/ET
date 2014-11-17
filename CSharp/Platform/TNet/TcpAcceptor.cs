using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TNet
{
	public class TcpAcceptor: IDisposable
	{
		private readonly Socket socket;
		private readonly SocketAsyncEventArgs asyncEventArgs = new SocketAsyncEventArgs();

		public TcpAcceptor(string ip, ushort port, int backLog = 100)
		{
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.asyncEventArgs.Completed += OnArgsCompletion;
			this.socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
			this.socket.Listen(backLog);
		}

		public void Dispose()
		{
			this.socket.Dispose();
		}

		/// <summary>
		/// SocketAsyncEventArgs Accept回调函数,回调可能已经到了另外的线程
		/// </summary>
		/// <returns></returns>
		public Task<NetworkStream> AcceptAsync()
		{
			var tcs = new TaskCompletionSource<NetworkStream>();

			try
			{
				this.asyncEventArgs.UserToken = tcs;

				bool ret = this.socket.AcceptAsync(this.asyncEventArgs);

				if (!ret)
				{
					if (this.asyncEventArgs.SocketError == SocketError.Success)
					{
						var acceptSocket = this.asyncEventArgs.AcceptSocket;
						this.asyncEventArgs.AcceptSocket = null;
						return Task.FromResult(new NetworkStream(acceptSocket, true));
					}
					tcs.TrySetException(new InvalidOperationException(this.asyncEventArgs.SocketError.ToString()));
				}
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
			return tcs.Task;
		}

		private static void OnArgsCompletion(object sender, SocketAsyncEventArgs e)
		{
			var tcs = (TaskCompletionSource<NetworkStream>) e.UserToken;

			if (e.SocketError != SocketError.Success)
			{
				tcs.TrySetException(new InvalidOperationException(e.SocketError.ToString()));
				return;
			}
			var acceptSocket = e.AcceptSocket;
			e.AcceptSocket = null;
			tcs.SetResult(new NetworkStream(acceptSocket, true));
		}
	}
}