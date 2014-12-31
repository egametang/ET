using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Network;
using TNet;

namespace TServiceTest
{
	[TestClass]
	public class TServiceTest
	{
		private readonly Barrier barrier = new Barrier(3);

		private async void ClientEvent(IService service, string hostName, ushort port)
		{
			IChannel channel = await service.GetChannel(hostName, port);
			channel.SendAsync("0123456789".ToByteArray());

			byte[] bytes = await channel.RecvAsync();
			CollectionAssert.AreEqual("9876543210".ToByteArray(), bytes);

			barrier.RemoveParticipant();
		}

		private async void ServerEvent(IService service)
		{
			IChannel channel = await service.GetChannel();
			byte[] bytes = await channel.RecvAsync();
			CollectionAssert.AreEqual("0123456789".ToByteArray(), bytes);
			Array.Reverse(bytes);
			channel.SendAsync(bytes);

			barrier.RemoveParticipant();
		}

		[TestMethod]
		public void ClientSendToServer()
		{
			const string hostName = "127.0.0.1";
			const ushort port = 8889;
			IService clientService = new TService();
			IService serverService = new TService(hostName, 8889);

			Task.Factory.StartNew(() => clientService.Run(), TaskCreationOptions.LongRunning);
			Task.Factory.StartNew(() => serverService.Run(), TaskCreationOptions.LongRunning);

			

			// 往server host线程增加事件,accept
			serverService.Add(() => ServerEvent(serverService));

			Thread.Sleep(1000);

			// 往client host线程增加事件,client线程连接server
			clientService.Add(() => ClientEvent(clientService, hostName, port));

			barrier.SignalAndWait();
		}
	}
}