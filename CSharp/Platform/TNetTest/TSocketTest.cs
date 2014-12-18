using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Helper;
using Common.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TNet;

namespace TNetTest
{
	[TestClass]
	public class TSocketTest
	{
		private Barrier barrier;
		private const int clientNum = 10;

		[TestMethod]
		public void SendRecv()
		{
			barrier = new Barrier(clientNum + 2);
			IPoller poller = new TPoller();
			Task.Factory.StartNew(() => poller.Run(), TaskCreationOptions.LongRunning);

			poller.Add(() => Server(poller));
			Thread.Sleep(500);

			for (int i = 0; i < clientNum; ++i)
			{
				poller.Add(() => Request(poller));
			}

			this.barrier.SignalAndWait();
		}

		private async void Server(IPoller poller)
		{
			TSocket acceptor = new TSocket(poller);
			acceptor.Bind("127.0.0.1", 10000);
			acceptor.Listen(100);

			for (int i = 0; i < clientNum; i++)
			{
				TSocket socket = new TSocket(poller);
				await acceptor.AcceptAsync(socket);
				Response(socket);
			}
			this.barrier.RemoveParticipant();
		}

		private static async void Response(TSocket socket)
		{
			byte[] buffer = new byte[10];
			for (int i = 0; i < 10000; i++)
			{
				await socket.RecvAsync(buffer, 0, buffer.Length);
				Array.Reverse(buffer);
				await socket.SendAsync(buffer, 0, buffer.Length);
			}
			await socket.DisconnectAsync();
		}

		private async void Request(IPoller poller)
		{
			TSocket client = new TSocket(poller);
			for (int i = 0; i < 10000; i++)
			{
				await client.ConnectAsync("127.0.0.1", 10000);
				byte[] buffer = "0123456789".ToByteArray();
				await client.SendAsync(buffer, 0, buffer.Length);
				await client.RecvAsync(buffer, 0, buffer.Length);
				Assert.AreEqual("9876543210", buffer.ToStr());
			}
			Log.Debug("1111111111111111111111111111111111111");
			this.barrier.RemoveParticipant();
		}
	}
}