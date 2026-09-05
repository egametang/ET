using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Test
{
    public class Core_UdpTransport_ZeroLengthDatagram_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            using UdpTransport receiver = new(new IPEndPoint(IPAddress.Loopback, 0));
            using UdpTransport sender = new(AddressFamily.InterNetwork);

            byte[] datagram = Array.Empty<byte>();
            sender.Send(datagram, 0, datagram.Length, receiver.GetBindPoint(), ChannelType.Connect);
            byte[] followingDatagram = new byte[1];
            sender.Send(followingDatagram, 0, followingDatagram.Length, receiver.GetBindPoint(), ChannelType.Connect);

            if (!receiver.Available())
            {
                Log.Console("UdpTransport should report a queued zero-length datagram as available");
                return 1;
            }

            byte[] receiveBuffer = new byte[1];
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            int receivedLength = receiver.Recv(receiveBuffer, ref remoteEndPoint);
            if (receivedLength != 0)
            {
                Log.Console($"Expected a zero-length datagram, but received {receivedLength} bytes");
                return 2;
            }

            if (!receiver.Available())
            {
                Log.Console("UdpTransport should report the datagram queued after a zero-length datagram");
                return 3;
            }

            receivedLength = receiver.Recv(receiveBuffer, ref remoteEndPoint);
            if (receivedLength != followingDatagram.Length)
            {
                Log.Console($"Expected the following datagram length to be {followingDatagram.Length}, but received {receivedLength} bytes");
                return 4;
            }

            if (receiver.Available())
            {
                Log.Console("UdpTransport should consume all queued datagrams");
                return 5;
            }

            return ErrorCode.ERR_Success;
        }
    }
}
