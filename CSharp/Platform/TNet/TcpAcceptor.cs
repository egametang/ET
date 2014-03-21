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

	    public TcpAcceptor(ushort port, int backLog = 100)
	    {
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.asyncEventArgs.Completed += OnArgsCompletion;
			this.socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
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
				asyncEventArgs.UserToken = tcs;

				bool ret = this.socket.AcceptAsync(asyncEventArgs);

				if (!ret)
				{
					if (asyncEventArgs.SocketError == SocketError.Success)
					{
						var acceptSocket = asyncEventArgs.AcceptSocket;
						asyncEventArgs.AcceptSocket = null;
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

	    private void OnArgsCompletion(object sender, SocketAsyncEventArgs e)
	    {
			var tcs = (TaskCompletionSource<NetworkStream>)e.UserToken;

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
