using System;
using System.Threading.Tasks;
using Helper;
using Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetMQ;
using Zmq;

namespace ZmqTest
{
	[TestClass]
	public class ZmqReqRepTest
	{
		const string address = "tcp://127.0.0.1:5001";

		[TestMethod]
		public void TestMethod()
		{
			var task1 = Task.Factory.StartNew(Server, TaskCreationOptions.LongRunning);
			var task2 = Task.Factory.StartNew(Client, TaskCreationOptions.LongRunning);
			Task.WaitAll(task1, task2);
		}

		private static void Client()
		{
			using (var context = NetMQContext.Create())
			{
				using (var req = context.CreateRequestSocket())
				{
					var poller = new Poller();
					req.Connect(address);
					req.ReceiveReady += (sender, args) =>
					{
						bool hasMore;
						string msg = args.Socket.ReceiveString(true, out hasMore);
						Logger.Debug(string.Format("req: {0}", msg));
						poller.Stop();
					};
					req.Send("hello world!");
					poller.AddSocket(req);
					poller.Start();
				}
			}
		}

		private static void Server()
		{
			using (var context = NetMQContext.Create())
			{
				using (var rep = context.CreateResponseSocket())
				{
					var poller = new Poller();
					poller.AddSocket(rep);
					rep.Bind(address);
					rep.ReceiveReady += (sender, args) =>
					{
						bool hasMore;
						string msg = args.Socket.ReceiveString(true, out hasMore);
						Logger.Debug(string.Format("rep: {0}", msg));
						args.Socket.Send(msg, true);
						poller.Stop();
					};
					poller.Start();
				}
			}
		}

		[TestMethod]
		public void TestSendAsyncAndRecvAsync()
		{
			var clientPoller = new ZmqPoller();
			var serverPoller = new ZmqPoller();

			clientPoller.Events += () => Client2(clientPoller);

			serverPoller.Events += () => Server2(serverPoller);

			var task1 = Task.Factory.StartNew(clientPoller.Start, TaskCreationOptions.LongRunning);
			var task2 = Task.Factory.StartNew(serverPoller.Start, TaskCreationOptions.LongRunning);
			Task.WaitAll(task1, task2);
		}

		public static async Task Client2(ZmqPoller poller)
		{
			using (var context = NetMQContext.Create())
			{
				try
				{
					var socket = new ZmqSocket(poller, context.CreateRequestSocket());
					socket.Connect(address);
					await socket.SendAsync("hello world".ToByteArray());
					byte[] bytes = await socket.RecvAsync();
					Logger.Debug(string.Format("client2: {0}", bytes.ToStr()));
					await Task.Run(() => poller.Stop(false));
				}
				catch (Exception e)
				{
					Logger.Debug(string.Format("exception: {0}", e.StackTrace));
				}
			}	
		}

		public static async Task Server2(ZmqPoller poller)
		{
			using (var context = NetMQContext.Create())
			{
				try
				{
					var socket = new ZmqSocket(poller, context.CreateResponseSocket());
					socket.Bind(address);
					byte[] bytes = await socket.RecvAsync();
					Logger.Debug(string.Format("server2: {0}", bytes.ToStr()));
					await socket.SendAsync("hello world".ToByteArray());
					await Task.Run(() => poller.Stop(false));
				}
				catch (Exception e)
				{
					Logger.Debug(string.Format("exception2: {0}", e.StackTrace));
				}
			}	
		}
	}
}
