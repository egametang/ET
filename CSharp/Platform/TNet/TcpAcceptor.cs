using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TNet
{
    public class TcpAcceptor
    {
	    private readonly Socket socket;
		private readonly SocketAsyncEventArgs asyncEventArgs = new SocketAsyncEventArgs();

	    public TcpAcceptor(Socket socket, EndPoint endPoint)
	    {
		    this.socket = socket;
			this.asyncEventArgs.Completed += OnArgsCompletion;
			this.socket.Bind(endPoint);
	    }

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
						return Task.FromResult(new NetworkStream(asyncEventArgs.AcceptSocket, true));
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

			if (asyncEventArgs.SocketError != SocketError.Success)
			{
				tcs.TrySetException(new InvalidOperationException(asyncEventArgs.SocketError.ToString()));
				return;
			}
			tcs.SetResult(new NetworkStream(e.AcceptSocket, true));
	    }
    }
}
