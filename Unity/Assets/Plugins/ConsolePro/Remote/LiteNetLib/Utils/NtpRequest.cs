using System.Net;
using System.Net.Sockets;

namespace FlyingWormConsole3.LiteNetLib.Utils
{
    internal sealed class NtpRequest
    {
        private const int ResendTimer = 1000;
        private const int KillTimer = 10000;
        public const int DefaultPort = 123;
        private readonly IPEndPoint _ntpEndPoint;
        private int _resendTime = ResendTimer;
        private int _killTime = 0;

        public NtpRequest(IPEndPoint endPoint)
        {
            _ntpEndPoint = endPoint;
        }

        public bool NeedToKill
        {
            get { return _killTime >= KillTimer; }
        }

        public bool Send(NetSocket socket, int time)
        {
            _resendTime += time;
            _killTime += time;
            if (_resendTime < ResendTimer)
            {
                return false;
            }
            SocketError errorCode = 0;
            var packet = new NtpPacket();
            var sendCount = socket.SendTo(packet.Bytes, 0, packet.Bytes.Length, _ntpEndPoint, ref errorCode);
            return errorCode == 0 && sendCount == packet.Bytes.Length;
        }
    }
}
