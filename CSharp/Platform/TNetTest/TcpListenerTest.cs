using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Helper;
using Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TNetTest
{
	[TestClass]
	public class TcpListenerTest
	{
		private const ushort port = 11111;
		private int count;
		private readonly Barrier barrier = new Barrier(2);
		private readonly object lockObject = new object();

		[TestMethod]
		public void AcceptAsync()
		{
			var thread1 = new Thread(Server);
			thread1.Start();

			Thread.Sleep(2);

			for (int i = 0; i < 1; ++i)
			{
				var thread = new Thread(this.Client);
				thread.Start();
			}
			barrier.SignalAndWait();
		}

		private async void Client()
		{
			using (var tcpClient = new TcpClient(AddressFamily.InterNetwork))
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
			barrier.RemoveParticipants(1);
		}

		private async void Server()
		{
			var tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
			tcpListener.Start();
			
			while (count != 1)
			{
				var socket = await tcpListener.AcceptSocketAsync();
				var ns = new NetworkStream(socket);
				Response(ns);
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
				lock (lockObject)
				{
					++count;
				}
			}
			catch (Exception e)
			{
				Log.Debug(e.ToString());
			}
		}
	}
}
