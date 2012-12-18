using System;
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
		private static async void Client(ClientHost host, Address address)
		{
			try
			{
				Logger.Debug("Client Connect");
				var peer = await host.ConnectAsync(address);
				Logger.Debug("Client Connect OK");
				var sPacket = new Packet("0123456789".ToByteArray(), PacketFlags.Reliable);
				peer.Send(0, sPacket);
				Logger.Debug("Client Send OK");
				var rPacket = await peer.ReceiveAsync();
				Logger.Debug("Client Receive OK");

				CollectionAssert.AreEqual("9876543210".ToByteArray(), rPacket.Bytes);

				await peer.DisconnectLaterAsync();
				Logger.Debug("Client Disconnect OK");
			}
			catch (ENetException e)
			{
				Assert.Fail("Client ENetException: {0}", e.Message);
			}
			finally
			{
				host.Stop();
			}
		}

		private static async void Server(ServerHost host)
		{
			try
			{
				Logger.Debug("Server Accept");
				var peer = await host.AcceptAsync();
				Logger.Debug("Server Accept OK");
				var rPacket = await peer.ReceiveAsync();
				Logger.Debug("Server Receive OK");

				CollectionAssert.AreEqual("0123456789".ToByteArray(), rPacket.Bytes);

				var sPacket = new Packet("9876543210".ToByteArray(), PacketFlags.Reliable);
				peer.Send(0, sPacket);
				Logger.Debug("Server Send OK");
				await peer.DisconnectLaterAsync();
				Logger.Debug("Server Disconnected OK");
			}
			catch (ENetException e)
			{
				Assert.Fail("Server ENetException: {0}", e.Message);
			}
			finally
			{
				host.Stop();
			}
		}

		[TestMethod]
		public void ClientSendToServer()
		{
			var address = new Address { HostName = "127.0.0.1", Port = 8888 };
			var clientHost = new ClientHost();
			var serverHost = new ServerHost(address);

			var server = new Thread(() => serverHost.Start(10));
			var client = new Thread(() => clientHost.Start(10));

			serverHost.Events += () => Server(serverHost);
			clientHost.Events += () => Client(clientHost, address);
			server.Start();
			client.Start();

			server.Join();
			client.Join();
		}
	}
}
