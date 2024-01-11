namespace FlyingWormConsole3.LiteNetLib
{
    /// <summary>
    /// Sending method type
    /// </summary>
    public enum DeliveryMethod : byte
    {
        /// <summary>
        /// Unreliable. Packets can be dropped, can be duplicated, can arrive without order.
        /// </summary>
        Unreliable = 4,

        /// <summary>
        /// Reliable. Packets won't be dropped, won't be duplicated, can arrive without order.
        /// </summary>
        ReliableUnordered = 0,

        /// <summary>
        /// Unreliable. Packets can be dropped, won't be duplicated, will arrive in order.
        /// </summary>
        Sequenced = 1,

        /// <summary>
        /// Reliable and ordered. Packets won't be dropped, won't be duplicated, will arrive in order.
        /// </summary>
        ReliableOrdered = 2,

        /// <summary>
        /// Reliable only last packet. Packets can be dropped (except the last one), won't be duplicated, will arrive in order.
        /// Cannot be fragmented
        /// </summary>
        ReliableSequenced = 3
    }

    /// <summary>
    /// Network constants. Can be tuned from sources for your purposes.
    /// </summary>
    public static class NetConstants
    {
        //can be tuned
        public const int DefaultWindowSize = 64;
        public const int SocketBufferSize = 1024 * 1024; //1mb
        public const int SocketTTL = 255;

        public const int HeaderSize = 1;
        public const int ChanneledHeaderSize = 4;
        public const int FragmentHeaderSize = 6;
        public const int FragmentedHeaderTotalSize = ChanneledHeaderSize + FragmentHeaderSize;
        public const ushort MaxSequence = 32768;
        public const ushort HalfMaxSequence = MaxSequence / 2;

        //protocol
        internal const int ProtocolId = 11;
        internal const int MaxUdpHeaderSize = 68;

        internal static readonly int[] PossibleMtu =
        {
            576  - MaxUdpHeaderSize, //minimal (RFC 1191)
            1024,                    //most games standard
            1232 - MaxUdpHeaderSize,
            1460 - MaxUdpHeaderSize, //google cloud
            1472 - MaxUdpHeaderSize, //VPN
            1492 - MaxUdpHeaderSize, //Ethernet with LLC and SNAP, PPPoE (RFC 1042)
            1500 - MaxUdpHeaderSize  //Ethernet II (RFC 1191)
        };

        internal static readonly int MaxPacketSize = PossibleMtu[PossibleMtu.Length - 1];

        //peer specific
        public const byte MaxConnectionNumber = 4;

        public const int PacketPoolSize = 1000;
    }
}
