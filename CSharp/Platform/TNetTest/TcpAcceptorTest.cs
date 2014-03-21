using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Helper;
using Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TNet;

namespace TNetTest
{
	[TestClass]
	public class TcpAcceptorTest
	{
		private const ushort port = 11111;
		private int count;
		private readonly Barrier barrier = new Barrier(100);
		private readonly object lockObject = new object();

		[TestMethod]
		public void AcceptAsync()
		{
			var thread1 = new Thread(Server);
			thread1.Start();

			Thread.Sleep(2);

			for (int i = 0; i < 99; ++i)
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
						await ns.WriteAsync(bytes, 0, bytes.Length);
						int n = await ns.ReadAsync(bytes, 0, bytes.Length);
						Assert.AreEqual(7, n);
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
			using (var tcpAcceptor = new TcpAcceptor(port))
			{
				while (count != 99)
				{
					NetworkStream ns = await tcpAcceptor.AcceptAsync();
					// 这里可能已经不在Server函数线程了
					Response(ns);
				}
			}
		}

		private async void Response(NetworkStream ns)
		{
			try
			{
				var bytes = new byte[1000];
				int n = await ns.ReadAsync(bytes, 0, 100);
				await ns.WriteAsync(bytes, 0, n);
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
