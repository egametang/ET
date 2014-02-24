using System.Threading.Tasks;
using Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetMQ;

namespace ZmqTest
{
	[TestClass]
	public class ZmqReqRepTest
	{
		const string address = "tcp://127.0.0.1:5001";

		[TestMethod]
		public void TestMethod()
		{
			var task1 = Task.Factory.StartNew(Server);
			var task2 = Task.Factory.StartNew(Client);
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
	}
}
