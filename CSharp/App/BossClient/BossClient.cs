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

		public async void HandleMessages()
		{
			if (this.gateSession == null)
			{
				throw new BossException("gate session is null");
			}

			try
			{
				await this.gateSession.HandleMessages();
			}
			catch (BossException e)
			{
				Logger.Trace("session: {0}, exception: {1}", this.gateSession.ID, e.ToString());
			}
		}

		public async Task Login(
			string hostName, ushort port, string account, string password)
		{
			int loginSessionId = ++this.sessionId;

			try
			{
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
				this.gateSession = new GateSession(loginSessionId, new ENetChannel(peer));
				await gateSession.Login(realmInfo.Item3);
			}
			catch (Exception e)
			{
				Logger.Trace("session: {0}, exception: {1}", loginSessionId, e.ToString());
			}
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
