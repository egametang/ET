using System.Threading;
using System.Threading.Tasks;
using Common.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TNet;

namespace TNetTest
{
	[TestClass]
	public class TServerTest
	{
		private Barrier barrier;
		private const int clientNum = 10;

		private const int sendNum = 10000000;

		[TestMethod]
		public void SendRecv()
		{
			barrier = new Barrier(clientNum + 1);
			TServer server = new TServer(10000);
			Task.Factory.StartNew(() => server.Start(), TaskCreationOptions.LongRunning);

			server.Push(() => Server(server));
			Thread.Sleep(1000);

			for (int i = 0; i < clientNum; ++i)
			{
				server.Push(() => this.ClientRequest(server));
			}

			this.barrier.SignalAndWait();
		}

		private async void Server(TServer server)
		{
			for (int i = 0; i < clientNum; i++)
			{
				TSession session = await server.AcceptAsync();
				int count = 0;
				session.OnRecv += () => this.ServerResponse(session, ref count);
				session.Start();
			}
		}

		private void ServerResponse(TSession session, ref int count)
		{
			byte[] buffer = new byte[10];
			while (session.RecvSize >= 10)
			{
				buffer = new byte[10];
				session.Recv(buffer);
				Assert.AreEqual("0123456789", buffer.ToStr());
				++count;
			}

			if (count == sendNum)
			{
				buffer = "9876543210".ToByteArray();
				session.Send(buffer);
			}
		}

		private async void ClientRequest(TServer server)
		{
			TSession session = await server.ConnectAsync("127.0.0.1", 10000);
			session.OnRecv += () => ClientOnResponse(session);
			session.Start();

			byte[] buffer = "0123456789".ToByteArray();
			for (int i = 0; i < sendNum; i++)
			{
				session.Send(buffer);
			}
		}

		private void ClientOnResponse(TSession session)
		{
			if (session.RecvSize < 10)
			{
				return;
			}
			byte[] buffer = new byte[10];
			session.Recv(buffer);
			Assert.AreEqual("9876543210", buffer.ToStr());
			this.barrier.RemoveParticipant();
		}
	}
}