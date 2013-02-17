using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ENet;
using Log;

namespace LoginClient
{
	public class LoginClient : IDisposable
    {
		private int sessionId;
		
		private readonly ClientHost clientHost = new ClientHost();

		private GateSession gateSession;
		
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

		public async void Login(
			string hostName, ushort port, string account, string password)
		{
			++this.sessionId;

			// 登录realm
			var tcpClient = new TcpClient();
			await tcpClient.ConnectAsync(hostName, port);
			Tuple<string, ushort, SRP6Client> realmInfo = null; // ip, port, K
			using (var realmSession = new RealmSession(this.sessionId, new TcpChannel(tcpClient)))
			{
				realmInfo = await realmSession.Login(account, password);
				Logger.Trace("session: {0}, login success!", realmSession.ID);
			}

			// 登录gate
			Peer peer = await this.clientHost.ConnectAsync(realmInfo.Item1, realmInfo.Item2);
			gateSession = new GateSession(this.sessionId, new ENetChannel(peer));
			await gateSession.Login(realmInfo.Item3);
			await gateSession.HandleMessages();
		}

		public void SendCommand(string command)
		{
			var cmsgBossGm = new CMSG_Boss_Gm
			{
				Message = command
			};
			this.gateSession.SendMessage(MessageOpcode.CMSG_BOSS_GM, cmsgBossGm);
		}
    }
}
