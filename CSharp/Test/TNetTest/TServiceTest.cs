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
		private const int echoTimes = 10000;
		private readonly Barrier barrier = new Barrier(3);

		private bool isClientStop;
		private bool isServerStop;

		private async void ClientEvent(IService service, string hostName, ushort port)
		{
			AChannel channel = await service.GetChannel(hostName, port);
			for (int i = 0; i < echoTimes; ++i)
			{
				channel.SendAsync("0123456789".ToByteArray());
				byte[] bytes = await channel.RecvAsync();
				CollectionAssert.AreEqual("9876543210".ToByteArray(), bytes);
			}

			this.barrier.RemoveParticipant();
		}

		private async void ServerEvent(IService service)
		{
			AChannel channel = await service.GetChannel();
			for (int i = 0; i < echoTimes; ++i)
			{
				byte[] bytes = await channel.RecvAsync();
				CollectionAssert.AreEqual("0123456789".ToByteArray(), bytes);
				Array.Reverse(bytes);
				channel.SendAsync(bytes);
			}

			this.barrier.RemoveParticipant();
		}

		[Test]
		public void ClientSendToServer()
		{
			const string hostName = "127.0.0.1";
			const ushort port = 8889;
			using(IService clientService = new TService())
			using (IService serverService = new TService(hostName, 8889))
			{
				Task task1 = Task.Factory.StartNew(() =>
				{
					while (!isClientStop)
					{
						clientService.Update();
					}
				}, TaskCreationOptions.LongRunning);

				Task task2 = Task.Factory.StartNew(() =>
				{
					while (!isServerStop)
					{
						serverService.Update();
					}
				}, TaskCreationOptions.LongRunning);

				// 往server host线程增加事件,accept
				serverService.Add(() => this.ServerEvent(serverService));

				Thread.Sleep(1000);

				// 往client host线程增加事件,client线程连接server
				clientService.Add(() => this.ClientEvent(clientService, hostName, port));

				this.barrier.SignalAndWait();

				serverService.Add(() => { isServerStop = true; });
				clientService.Add(() => { isClientStop = true; });
				Task.WaitAll(task1, task2);
			}
		}
	}
}