using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Helper;
using UNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Network;

namespace UNetTest
{
	[TestClass]
	public class UNetClientServerTest
	{
		private readonly Barrier barrier = new Barrier(3);

		private async void ClientEvent(IService service, string hostName, ushort port)
		{
			IChannel channel = await service.GetChannel(hostName, port);
			channel.SendAsync("0123456789".ToByteArray());

			byte[] bytes = await channel.RecvAsync();
			Assert.AreEqual("9876543210".ToByteArray(), bytes);

			barrier.RemoveParticipant();
		}

		private async void ServerEvent(IService service)
		{
			IChannel channel = await service.GetChannel();
			byte[] bytes = await channel.RecvAsync();
			Assert.AreEqual("0123456789".ToByteArray(), bytes);
			Array.Reverse(bytes);
			channel.SendAsync(bytes);

			barrier.RemoveParticipant();
		}

		[TestMethod]
		public void ClientSendToServer()
		{
			const string hostName = "127.0.0.1";
			const ushort port = 8889;
			IService clientService = new UService(hostName, 8888);
			IService serverService = new UService(hostName, 8889);

			Task.Factory.StartNew(() => clientService.Start(), TaskCreationOptions.LongRunning);
			Task.Factory.StartNew(() => serverService.Start(), TaskCreationOptions.LongRunning);

			

			// 往server host线程增加事件,accept
			serverService.Add(() => ServerEvent(serverService));

			Thread.Sleep(1000);

			// 往client host线程增加事件,client线程连接server
			clientService.Add(() => ClientEvent(clientService, hostName, port));

			barrier.SignalAndWait();
		}
	}
}