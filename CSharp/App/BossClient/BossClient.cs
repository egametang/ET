using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ENet;
using Logger;

namespace BossClient
{
    public class BossClient: IDisposable
    {
        private int sessionId;

        private readonly EService ioService = new EService();

        public BossClient()
        {
            this.ioService.EnableCrc();
        }

        public void Dispose()
        {
            this.ioService.Dispose();
        }

        public void RunOnce()
        {
            this.ioService.RunOnce();
        }

        public void Start(int timeout)
        {
            this.ioService.Start(timeout);
        }

        public GateSession GateSession { get; private set; }

        public async Task Login(string hostName, ushort port, string account, string password)
        {
            int loginSessionId = ++this.sessionId;

            // 登录realm
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(hostName, port);
            Tuple<string, ushort, SRP6Client> realmInfo = null; // ip, port, K
            using (var realmSession = new RealmSession(loginSessionId, new TcpChannel(tcpClient)))
            {
                realmInfo = await realmSession.Login(account, password);
                Log.Trace("session: {0}, login success!", realmSession.ID);
            }

            // 登录gate
            var eSocket = new ESocket(this.ioService);
            await eSocket.ConnectAsync(realmInfo.Item1, realmInfo.Item2);
            this.GateSession = new GateSession(loginSessionId, new ENetChannel(eSocket));
            await this.GateSession.Login(realmInfo.Item3);
        }
    }
}