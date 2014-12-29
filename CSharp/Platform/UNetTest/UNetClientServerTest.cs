using System.Diagnostics;
using System.Threading;
using Common.Helper;
using Common.Logger;
using UNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UNetTest
{
	[TestClass]
	public class UNetClientServerTest
	{
		private const int pingPangCount = 10000;

		private static async void ClientEvent(EService service, string hostName, ushort port)
		{
			var eSocket = new ESocket(service);
			await eSocket.ConnectAsync(hostName, port);
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (int i = 0; i < pingPangCount; ++i)
			{
				eSocket.WriteAsync("0123456789".ToByteArray());

				var bytes = await eSocket.ReadAsync();

				CollectionAssert.AreEqual("9876543210".ToByteArray(), bytes);
			}
			stopWatch.Stop();
			Log.Debug("time: {0}", stopWatch.ElapsedMilliseconds);
			await eSocket.DisconnectAsync();
			service.Stop();
		}

		private static async void ServerEvent(EService service, Barrier barrier)
		{
			barrier.SignalAndWait();

			bool isRunning = true;
			while (isRunning)
			{
				Log.Debug("start accept");
				var eSocket = new ESocket(service);
				await eSocket.AcceptAsync();
				eSocket.Disconnect += ev =>
				{
					isRunning = false;
					service.Stop();
				};
				Echo(eSocket);
			}
		}

		private static async void Echo(ESocket eSocket)
		{
			for (int i = 0; i < pingPangCount; ++i)
			{
				var bytes = await eSocket.ReadAsync();

				CollectionAssert.AreEqual("0123456789".ToByteArray(), bytes);

				eSocket.WriteAsync("9876543210".ToByteArray());
			}
		}

		[TestMethod]
		public void ClientSendToServer()
		{
			const string hostName = "127.0.0.1";
			const ushort port = 8888;
			var clientHost = new EService();
			var serverHost = new EService(hostName, port);

			var serverThread = new Thread(() => serverHost.Start());
			var clientThread = new Thread(() => clientHost.Start());

			serverThread.Start();
			clientThread.Start();

			var barrier = new Barrier(2);

			// 往server host线程增加事件,accept
			serverHost.Events += () => ServerEvent(serverHost, barrier);

			barrier.SignalAndWait();

			// 往client host线程增加事件,client线程连接server
			clientHost.Events += () => ClientEvent(clientHost, hostName, port);

			serverThread.Join();
			clientThread.Join();
		}
	}
}