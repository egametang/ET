using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Common.Helper;
using Common.Logger;
using NUnit.Framework;

namespace TNetTest
{
	[TestFixture]
	public class TcpListenerTest
	{
		private const ushort port = 11111;
		private int count;
		private readonly Barrier barrier = new Barrier(2);
		private readonly object lockObject = new object();

		[Test]
		public void AcceptAsync()
		{
			Thread thread1 = new Thread(this.Server);
			thread1.Start();

			Thread.Sleep(2);

			for (int i = 0; i < 1; ++i)
			{
				Thread thread = new Thread(this.Client);
				thread.Start();
			}
			this.barrier.SignalAndWait();
		}

		private async void Client()
		{
			using (TcpClient tcpClient = new TcpClient(AddressFamily.InterNetwork))
			{
				await tcpClient.ConnectAsync("127.0.0.1", port);
				using (NetworkStream ns = tcpClient.GetStream())
				{
					try
					{
						var bytes = "tanghai".ToByteArray();
						for (int i = 0; i < 100; ++i)
						{
							await ns.WriteAsync(bytes, 0, bytes.Length);
							int n = await ns.ReadAsync(bytes, 0, bytes.Length);
							Assert.AreEqual(7, n);
						}
					}
					catch (Exception e)
					{
						Log.Debug(e.ToString());
					}
				}
			}
			this.barrier.RemoveParticipants(1);
		}

		private async void Server()
		{
			TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
			tcpListener.Start();

			while (this.count != 1)
			{
				Socket socket = await tcpListener.AcceptSocketAsync();
				NetworkStream ns = new NetworkStream(socket);
				this.Response(ns);
			}
		}

		private async void Response(NetworkStream ns)
		{
			try
			{
				var bytes = new byte[1000];
				for (int i = 0; i < 100; ++i)
				{
					int n = await ns.ReadAsync(bytes, 0, 100);
					await ns.WriteAsync(bytes, 0, n);
				}
				lock (this.lockObject)
				{
					++this.count;
				}
			}
			catch (Exception e)
			{
				Log.Debug(e.ToString());
			}
		}
	}
}