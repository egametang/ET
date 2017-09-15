#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
namespace FlyingWormConsole3.LiteNetLib
{
    public enum SendOptions
    {
        Unreliable,
        ReliableUnordered,
        Sequenced,
        ReliableOrdered
    }

    public static class NetConstants
    {
        public const int HeaderSize = 1;
        public const int SequencedHeaderSize = 3;
        public const int FragmentHeaderSize = 6;
        public const int DefaultWindowSize = 64;
        public const ushort MaxSequence = 32768;
        public const ushort HalfMaxSequence = MaxSequence / 2;

        //socket
        public const string MulticastGroupIPv4 = "224.0.0.1";
        public const string MulticastGroupIPv6 = "FF02:0:0:0:0:0:0:1";
        public const int SocketBufferSize = 1024*1024; //2mb
        public const int SocketTTL = 255;

        //protocol
        public const int ProtocolId = 1;
        public const int MaxUdpHeaderSize = 68;
        public const int PacketSizeLimit = ushort.MaxValue - MaxUdpHeaderSize;
        public const int MinPacketSize = 576 - MaxUdpHeaderSize;
        public const int MinPacketDataSize = MinPacketSize - HeaderSize;
        public const int MinSequencedPacketDataSize = MinPacketSize - SequencedHeaderSize;

        public static readonly int[] PossibleMtu =
        {
            576 - MaxUdpHeaderSize,  //Internet Path MTU for X.25 (RFC 879)
            1492 - MaxUdpHeaderSize, //Ethernet with LLC and SNAP, PPPoE (RFC 1042)
            1500 - MaxUdpHeaderSize, //Ethernet II (RFC 1191)
            4352 - MaxUdpHeaderSize, //FDDI
            4464 - MaxUdpHeaderSize, //Token ring
            7981 - MaxUdpHeaderSize  //WLAN
        };

        public static int MaxPacketSize = PossibleMtu[PossibleMtu.Length - 1];

        //peer specific
        public const int FlowUpdateTime = 1000;
        public const int FlowIncreaseThreshold = 4;
        public const int DefaultPingInterval = 1000;
    }
}
#endif
