using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using ENet;
using Log;

namespace LoginClient
{
	public class LoginClient : IDisposable
    {
		private int sessionId;

		private readonly ClientHost clientHost = new ClientHost();
		
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

		public async Task<List<Realm_List_Gate>> LoginRealm(
			string hostName, ushort port, string account, string password)
	    {
			using (var tcpClient = new TcpClient())
			{
				await tcpClient.ConnectAsync(hostName, port);

				using (var session = new RealmSession(
					tcpClient.GetStream()) { ID = ++this.sessionId })
				{
					var gateList = await session.Login(account, password);

					Logger.Trace("session: {0}, login success!", session.ID);
					return gateList;
				}
			}
	    }

		public async void LoginGate(string hostName, ushort port)
		{
			Peer peer = await this.clientHost.ConnectAsync(hostName, port);
			using (var session = new GateSession(peer) { ID = ++sessionId })
			{
				session.Login();
			}
		}
    }
}
