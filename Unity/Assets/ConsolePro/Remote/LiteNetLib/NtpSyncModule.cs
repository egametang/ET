#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
using System;
using System.Threading;

namespace FlyingWormConsole3.LiteNetLib
{
    public class NtpSyncModule
    {
        public DateTime? SyncedTime { get; private set; }
        private readonly NetSocket _socket;
        private readonly NetEndPoint _ntpEndPoint;
        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);

        public NtpSyncModule(string ntpServer)
        {
            _ntpEndPoint = new NetEndPoint(ntpServer, 123);
            _socket = new NetSocket(OnMessageReceived);
            _socket.Bind(0, false);
            SyncedTime = null;
        }

        private void OnMessageReceived(byte[] data, int length, int errorCode, NetEndPoint remoteEndPoint)
        {
            if (errorCode != 0)
            {
                _waiter.Set();
                return;
            }

            ulong intPart = (ulong)data[40] << 24 | (ulong)data[41] << 16 | (ulong)data[42] << 8 | (ulong)data[43];
            ulong fractPart = (ulong)data[44] << 24 | (ulong)data[45] << 16 | (ulong)data[46] << 8 | (ulong)data[47];

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            SyncedTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            _waiter.Set();
        }

        public void GetNetworkTime()
        {
            if (SyncedTime != null)
                return;

            var ntpData = new byte[48];
            //LeapIndicator = 0 (no warning)
            //VersionNum = 3
            //Mode = 3 (Client Mode)
            ntpData[0] = 0x1B;

            //send
            int errorCode = 0;
            _socket.SendTo(ntpData, 0, ntpData.Length, _ntpEndPoint, ref errorCode);

            if(errorCode == 0)
                _waiter.WaitOne(1000);
        }
    }
}
#endif
