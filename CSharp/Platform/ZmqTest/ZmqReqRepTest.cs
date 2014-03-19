using System;
using System.Threading.Tasks;
using Helper;
using Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zmq;
using ZeroMQ;

namespace ZmqTest
{
	[TestClass]
	public class ZmqReqRepTest
	{
		const string address = "tcp://127.0.0.1:5001";

		[TestMethod]
		public void TestSendAsyncAndRecvAsync()
		{
			var clientPoller = new ZPoller();
			var serverPoller = new ZPoller();

			clientPoller.Events += () => Client2(clientPoller);

			serverPoller.Events += () => Server2(serverPoller);

			var task1 = Task.Factory.StartNew(clientPoller.Start, TaskCreationOptions.LongRunning);
			var task2 = Task.Factory.StartNew(serverPoller.Start, TaskCreationOptions.LongRunning);
			Task.WaitAll(task1, task2);
		}

		public static async Task Client2(ZPoller zPoller)
		{
			using (var context = ZmqContext.Create())
			{
				try
				{
					var socket = new ZSocket(context.CreateSocket(SocketType.REP));
					zPoller.Add(socket);
					socket.Connect(address);
					await socket.SendAsync("hello world");
					string recvStr = await socket.RecvAsync();
					Logger.Debug(string.Format("client2: {0}", recvStr));
					zPoller.Stop();
				}
				catch (Exception e)
				{
					Logger.Debug(string.Format("exception: {0}", e.StackTrace));
				}
			}	
		}

		public static async Task Server2(ZPoller zPoller)
		{
			using (var context = ZmqContext.Create())
			{
				try
				{
					var socket = new ZSocket(context.CreateSocket(SocketType.REP));
					zPoller.Add(socket);
					socket.Bind(address);
					string recvStr = await socket.RecvAsync();
					Logger.Debug(string.Format("server2: {0}", recvStr));
					await socket.SendAsync("hello world");
					zPoller.Stop();
				}
				catch (Exception e)
				{
					Logger.Debug(string.Format("exception2: {0}", e.StackTrace));
				}
			}	
		}
	}
}
