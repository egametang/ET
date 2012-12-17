using System.Threading;
using ENet;
using Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENetCSTest
{
	[TestClass]
	public class ENetClientServerTest
	{
		private static async void Connect(ClientHost host, Address address)
		{
			Logger.Debug("Connect");
			var peer = await host.ConnectAsync(address);
			Logger.Debug("Connect OK");
			var sPacket = new Packet("0123456789", PacketFlags.Reliable);
			peer.Send(0, sPacket);
			Logger.Debug("Send OK");
			var rPacket = await peer.ReceiveAsync();
			Logger.Debug("Receive OK");

			Assert.AreEqual("9876543210", rPacket.Bytes.ToString());

			await peer.DisconnectLaterAsync();
			Logger.Debug("Disconnect OK");

			host.Stop();
		}

		private static async void Accept(ServerHost host)
		{
			Logger.Debug("Accept");
			var peer = await host.AcceptAsync();
			Logger.Debug("Accept OK");
			var rPacket = await peer.ReceiveAsync();
			Logger.Debug("Receive OK");

			Assert.AreEqual("0123456789", rPacket.Bytes.ToString());

			var sPacket = new Packet("9876543210", PacketFlags.Reliable);
			peer.Send(0, sPacket);
			Logger.Debug("Send OK");
			await peer.DisconnectLaterAsync();

			host.Stop();
		}

		[TestMethod]
		public void ClientSendToServer()
		{
			Library.Initialize();

			var address = new Address { HostName = "127.0.0.1", Port = 8888 };
			var clientHost = new ClientHost();
			var serverHost = new ServerHost(address);

			var server = new Thread(() => serverHost.Run(10));
			var client = new Thread(() => clientHost.Run(10));

			serverHost.Events += () => Accept(serverHost);
			clientHost.Events += () => Connect(clientHost, address);
			server.Start();
			client.Start();

			server.Join();
			client.Join();

			Library.Deinitialize();
		}
	}
}
