using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ENet;
using Log;

namespace BossClient
{
	public class BossClient : IDisposable
	{
		private int sessionId;

		private readonly ClientHost clientHost = new ClientHost();

		public BossClient()
		{
			this.clientHost.EnableCrc();
		}

		public void Dispose()
		{
			this.clientHost.Dispose();
		}

		public void RunOnce()
		{
			this.clientHost.RunOnce();
		}

		public void Start(int timeout)
		{
			this.clientHost.Start(timeout);
		}

		public GateSession GateSession { get; private set; }

		public async Task Login(
			string hostName, ushort port, string account, string password)
		{
			int loginSessionId = ++this.sessionId;

			// 登录realm
			var tcpClient = new TcpClient();
			await tcpClient.ConnectAsync(hostName, port);
			Tuple<string, ushort, SRP6Client> realmInfo = null; // ip, port, K
			using (var realmSession = new RealmSession(loginSessionId, new TcpChannel(tcpClient)))
			{
				realmInfo = await realmSession.Login(account, password);
				Logger.Trace("session: {0}, login success!", realmSession.ID);
			}

			// 登录gate
			Peer peer = await this.clientHost.ConnectAsync(realmInfo.Item1, realmInfo.Item2);
			this.GateSession = new GateSession(loginSessionId, new ENetChannel(peer));
			await this.GateSession.Login(realmInfo.Item3);
		}
	}
}
