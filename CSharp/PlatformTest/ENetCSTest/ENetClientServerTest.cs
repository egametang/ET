using System;
using System.Threading;
using System.Windows.Threading;
using ENet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENetCSTest
{
	[TestClass]
	public class ENetClientServerTest
	{
		private static async void Connect(Host host, Address address)
		{
			var peer = await host.ConnectAsync(address);
			var sPacket = new Packet("0123456789", PacketFlags.Reliable);
			peer.Send(0, sPacket);
			var rPacket = await peer.ReceiveAsync();

			Assert.AreEqual("9876543210", rPacket.Bytes.ToString());

			await peer.DisconnectLaterAsync();
		}

		private static async void Accept(Host host)
		{
			var peer = await host.AcceptAsync();
			var rPacket = await peer.ReceiveAsync();

			Assert.AreEqual("0123456789", rPacket.Bytes.ToString());

			var sPacket = new Packet("9876543210", PacketFlags.Reliable);
			peer.Send(0, sPacket);
			await peer.DisconnectLaterAsync();
		}

		[TestMethod]
		public void ClientSendToServer()
		{
			Library.Initialize();

			var address = new Address { HostName = "127.0.0.1", Port = 8888 };
			var serverHost = new Host();
			var clientHost = new Host(address);

			var server = new Thread(() => serverHost.Run(10));
			var client = new Thread(() => clientHost.Run(10));

			serverHost.Events += () => Accept(serverHost);
			clientHost.Events += () => Connect(clientHost, address);
			server.Start();
			client.Start();

			server.Join();
			client.Join();
		}
	}
}
