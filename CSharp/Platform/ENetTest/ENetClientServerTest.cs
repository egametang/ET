using System.Threading;
using ENet;
using Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENetCSTest
{
	[TestClass]
	public class ENetClientServerTest
	{
		private static async void ClientEvent(ClientHost host, Address address)
		{
			using (var peer = await host.ConnectAsync(address))
			{
				using (var sPacket = new Packet("0123456789".ToByteArray(), PacketFlags.Reliable))
				{
					peer.Send(0, sPacket);
				}

				using (var rPacket = await peer.ReceiveAsync())
				{
					CollectionAssert.AreEqual("9876543210".ToByteArray(), rPacket.Bytes);
				}

				await peer.DisconnectLaterAsync();
			}
			host.Stop();
		}

		private static async void OnAccept(ServerHost host, Peer peer)
		{
			using (var rPacket = await peer.ReceiveAsync())
			{
				CollectionAssert.AreEqual("0123456789".ToByteArray(), rPacket.Bytes);
			}

			using(var sPacket = new Packet("9876543210".ToByteArray(), PacketFlags.Reliable))
			{
				peer.Send(0, sPacket);
				host.Flush();
			}

			// Client断开,Server端收到Disconnect事件,结束Server线程
			peer.PeerEvent.Disconnect += (ev) => host.Stop();
		}

		[TestMethod]
		public void ClientSendToServer()
		{
			var address = new Address { HostName = "127.0.0.1", Port = 8888 };
			var clientHost = new ClientHost();
			var serverHost = new ServerHost(address);

			// accept回调事件
			serverHost.AcceptEvent = peer => OnAccept(serverHost, peer);

			var serverThread = new Thread(() => serverHost.Start(10));
			var clientThread = new Thread(() => clientHost.Start(10));

			serverThread.Start();
			clientThread.Start();

			// 往client host线程增加事件,client线程连接server
			clientHost.Events += () => ClientEvent(clientHost, address);

			serverThread.Join();
			clientThread.Join();
		}
	}
}
