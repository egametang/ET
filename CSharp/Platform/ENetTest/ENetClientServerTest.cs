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
		private static async void ClientEvent(ClientHost host, string hostName, ushort port)
		{
			var peer = await host.ConnectAsync(hostName, port);
			using (var sPacket = new Packet("0123456789".ToByteArray(), PacketFlags.Reliable))
			{
				peer.WriteAsync(0, sPacket);
			}

			using (var rPacket = await peer.ReadAsync())
			{
				Logger.Debug(rPacket.Bytes.ToHex());
				CollectionAssert.AreEqual("9876543210".ToByteArray(), rPacket.Bytes);
			}

			await peer.DisconnectAsync();

			host.Stop();
		}

		private static async void ServerEvent(ServerHost host, Barrier barrier)
		{
			var peer = await host.AcceptAsync();
			// Client断开,Server端收到Disconnect事件,结束Server线程
			peer.PeerEvent.Disconnect += ev => host.Stop();

			barrier.SignalAndWait();

			using (var rPacket = await peer.ReadAsync())
			{
				Logger.Debug(rPacket.Bytes.ToHex());
				CollectionAssert.AreEqual("0123456789".ToByteArray(), rPacket.Bytes);
			}

			using (var sPacket = new Packet("9876543210".ToByteArray(), PacketFlags.Reliable))
			{
				peer.WriteAsync(0, sPacket);
			}
		}

		[TestMethod]
		public void ClientSendToServer()
		{
			const string hostName = "127.0.0.1";
			const ushort port = 8888;
			var clientHost = new ClientHost();
			var serverHost = new ServerHost(hostName, port);

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