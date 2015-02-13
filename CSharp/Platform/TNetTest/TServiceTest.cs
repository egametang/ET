using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Helper;
using Common.Network;
using NUnit.Framework;
using TNet;

namespace TNetTest
{
	[TestFixture]
	public class TServiceTest
	{
		private readonly Barrier barrier = new Barrier(3);

		private async void ClientEvent(IService service, string hostName, ushort port)
		{
			AChannel channel = await service.GetChannel(hostName, port);
			channel.SendAsync("0123456789".ToByteArray());

			byte[] bytes = await channel.RecvAsync();
			CollectionAssert.AreEqual("9876543210".ToByteArray(), bytes);

			this.barrier.RemoveParticipant();
		}

		private async void ServerEvent(IService service)
		{
			AChannel channel = await service.GetChannel();
			byte[] bytes = await channel.RecvAsync();
			CollectionAssert.AreEqual("0123456789".ToByteArray(), bytes);
			Array.Reverse(bytes);
			channel.SendAsync(bytes);

			this.barrier.RemoveParticipant();
		}

		[Test]
		public void ClientSendToServer()
		{
			const string hostName = "127.0.0.1";
			const ushort port = 8889;
			IService clientService = new TService();
			IService serverService = new TService(hostName, 8889);

			Task.Factory.StartNew(() => clientService.Start(), TaskCreationOptions.LongRunning);
			Task.Factory.StartNew(() => serverService.Start(), TaskCreationOptions.LongRunning);

			// 往server host线程增加事件,accept
			serverService.Add(() => this.ServerEvent(serverService));

			Thread.Sleep(1000);

			// 往client host线程增加事件,client线程连接server
			clientService.Add(() => this.ClientEvent(clientService, hostName, port));

			this.barrier.SignalAndWait();
		}
	}
}