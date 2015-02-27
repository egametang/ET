using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Helper;
using Common.Logger;
using Common.Network;
using NUnit.Framework;
using UNet;

namespace UNetTest
{
	[TestFixture]
	public class UServiceTest
	{
		private readonly Barrier barrier = new Barrier(2);

		private async void ClientEvent(IService clientService, string hostName, ushort port)
		{
			AChannel channel = await clientService.GetChannel(hostName, port);
			channel.SendAsync("0123456789".ToByteArray());

			byte[] bytes = await channel.RecvAsync();
			CollectionAssert.AreEqual("9876543210".ToByteArray(), bytes);
			barrier.RemoveParticipant();
		}

		private async void ServerEvent(IService service)
		{
			AChannel channel = await service.GetChannel();
			byte[] bytes = await channel.RecvAsync();
			CollectionAssert.AreEqual("0123456789".ToByteArray(), bytes);
			Array.Reverse(bytes);
			channel.SendAsync(bytes);
		}

		[Test]
		public void ClientSendToServer()
		{
			const string hostName = "127.0.0.1";
			const ushort port = 8889;
			using (IService clientService = new UService(hostName, 8888))
			using (IService serverService = new UService(hostName, 8889))
			{	
				Task task1 = Task.Factory.StartNew(() => clientService.Start(), TaskCreationOptions.LongRunning);
				Task task2 = Task.Factory.StartNew(() => serverService.Start(), TaskCreationOptions.LongRunning);

				// 往server host线程增加事件,accept
				serverService.Add(() => this.ServerEvent(serverService));

				Thread.Sleep(1000);

				// 往client host线程增加事件,client线程连接server
				clientService.Add(() => this.ClientEvent(clientService, hostName, port));
				barrier.SignalAndWait();

				serverService.Add(serverService.Stop);
				clientService.Add(clientService.Stop);
				Task.WaitAll(task1, task2);
			}
		}
	}
}