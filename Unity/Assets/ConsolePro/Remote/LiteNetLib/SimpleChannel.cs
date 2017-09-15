#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
using System.Collections.Generic;

namespace FlyingWormConsole3.LiteNetLib
{
    internal sealed class SimpleChannel
    {
        private readonly Queue<NetPacket> _outgoingPackets;
        private readonly NetPeer _peer;

        public SimpleChannel(NetPeer peer)
        {
            _outgoingPackets = new Queue<NetPacket>();
            _peer = peer;
        }

        public void AddToQueue(NetPacket packet)
        {
            lock (_outgoingPackets)
            {
                _outgoingPackets.Enqueue(packet);
            }
        }

        public bool SendNextPacket()
        {
            NetPacket packet;
            lock (_outgoingPackets)
            {
                if (_outgoingPackets.Count == 0)
                    return false;
                packet = _outgoingPackets.Dequeue();
            }
            _peer.SendRawData(packet);
            _peer.Recycle(packet);
            return true;
        }
    }
}
#endif
