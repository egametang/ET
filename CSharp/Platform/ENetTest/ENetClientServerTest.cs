using System.Threading;
using ENet;
using Helper;
using Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENetCSTest
{
	[TestClass]
	public class ENetClientServerTest
	{
		private static async void ClientEvent(IOService service, string hostName, ushort port)
		{
			var eSocket = new ESocket(service);
			await eSocket.ConnectAsync(hostName, port);
			eSocket.WriteAsync("0123456789".ToByteArray());

			var bytes = await eSocket.ReadAsync();
			CollectionAssert.AreEqual("9876543210".ToByteArray(), bytes);

			await eSocket.DisconnectAsync();

			service.Stop();
		}

		private static async void ServerEvent(IOService service, Barrier barrier)
		{
			barrier.SignalAndWait();
			var eSocket = new ESocket(service);
			await eSocket.AcceptAsync();
			// Client断开,Server端收到Disconnect事件,结束Server线程
			eSocket.ESocketEvent.Disconnect += ev => service.Stop();

			var bytes = await eSocket.ReadAsync();
			CollectionAssert.AreEqual("0123456789".ToByteArray(), bytes);

			eSocket.WriteAsync("9876543210".ToByteArray(), 0, PacketFlags.Reliable);
		}

		[TestMethod]
		public void ClientSendToServer()
		{
			const string hostName = "127.0.0.1";
			const ushort port = 8888;
			var clientHost = new IOService();
			var serverHost = new IOService(hostName, port);

			var serverThread = new Thread(() => serverHost.Start(10));
			var clientThread = new Thread(() => clientHost.Start(10));

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