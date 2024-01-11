using System;

namespace FlyingWormConsole3.LiteNetLib.Utils
{
    /// <summary>
    /// Represents RFC4330 SNTP packet used for communication to and from a network time server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most applications should just use the <see cref="NtpPacket.CorrectionOffset" /> property.
    /// </para>
    /// <para>
    /// The same data structure represents both request and reply packets.
    /// Request and reply differ in which properties are set and to what values.
    /// </para>
    /// <para>
    /// The only real property is <see cref="NtpPacket.Bytes" />.
    /// All other properties read from and write to the underlying byte array
    /// with the exception of <see cref="NtpPacket.DestinationTimestamp" />,
    /// which is not part of the packet on network and it is instead set locally after receiving the packet.
    /// </para>
    /// <para>
    /// Copied from <a href="https://guerrillantp.machinezoo.com/">GuerrillaNtp project</a>
    /// with permission from Robert Vazan (@robertvazan) under MIT license, see https://github.com/RevenantX/LiteNetLib/pull/236
    /// </para>
    /// </remarks>
    public class NtpPacket
    {
        private static readonly DateTime Epoch = new DateTime(1900, 1, 1);

        /// <summary>
        /// Gets RFC4330-encoded SNTP packet.
        /// </summary>
        /// <value>
        /// Byte array containing RFC4330-encoded SNTP packet. It is at least 48 bytes long.
        /// </value>
        /// <remarks>
        /// This is the only real property. All other properties except
        /// <see cref="NtpPacket.DestinationTimestamp" /> read from or write to this byte array.
        /// </remarks>
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// Gets the leap second indicator.
        /// </summary>
        /// <value>
        /// Leap second warning, if any. Special value
        /// <see cref="NtpLeapIndicator.AlarmCondition" /> indicates unsynchronized server clock.
        /// Default is <see cref="NtpLeapIndicator.NoWarning" />.
        /// </value>
        /// <remarks>
        /// Only servers fill in this property. Clients can consult this property for possible leap second warning.
        /// </remarks>
        public NtpLeapIndicator LeapIndicator
        {
            get { return (NtpLeapIndicator)((Bytes[0] & 0xC0) >> 6); }
        }

        /// <summary>
        /// Gets or sets protocol version number.
        /// </summary>
        /// <value>
        /// SNTP protocol version. Default is 4, which is the latest version at the time of this writing.
        /// </value>
        /// <remarks>
        /// In request packets, clients should leave this property at default value 4.
        /// Servers usually reply with the same protocol version.
        /// </remarks>
        public int VersionNumber
        {
            get { return (Bytes[0] & 0x38) >> 3; }
            private set { Bytes[0] = (byte)((Bytes[0] & ~0x38) | value << 3); }
        }

        /// <summary>
        /// Gets or sets SNTP packet mode, i.e. whether this is client or server packet.
        /// </summary>
        /// <value>
        /// SNTP packet mode. Default is <see cref="NtpMode.Client" /> in newly created packets.
        /// Server reply should have this property set to <see cref="NtpMode.Server" />.
        /// </value>
        public NtpMode Mode
        {
            get { return (NtpMode)(Bytes[0] & 0x07); }
            private set { Bytes[0] = (byte)((Bytes[0] & ~0x07) | (int)value); }
        }

        /// <summary>
        /// Gets server's distance from the reference clock.
        /// </summary>
        /// <value>
        /// <para>
        /// Distance from the reference clock. This property is set only in server reply packets.
        /// Servers connected directly to reference clock hardware set this property to 1.
        /// Statum number is incremented by 1 on every hop down the NTP server hierarchy.
        /// </para>
        /// <para>
        /// Special value 0 indicates that this packet is a Kiss-o'-Death message
        /// with kiss code stored in <see cref="NtpPacket.ReferenceId" />.
        /// </para>
        /// </value>
        public int Stratum { get { return Bytes[1]; } }

        /// <summary>
        /// Gets server's preferred polling interval.
        /// </summary>
        /// <value>
        /// Polling interval in log2 seconds, e.g. 4 stands for 16s and 17 means 131,072s.
        /// </value>
        public int Poll { get { return Bytes[2]; } }

        /// <summary>
        /// Gets the precision of server clock.
        /// </summary>
        /// <value>
        /// Clock precision in log2 seconds, e.g. -20 for microsecond precision.
        /// </value>
        public int Precision { get { return (sbyte)Bytes[3]; } }

        /// <summary>
        /// Gets the total round-trip delay from the server to the reference clock.
        /// </summary>
        /// <value>
        /// Round-trip delay to the reference clock. Normally a positive value smaller than one second.
        /// </value>
        public TimeSpan RootDelay { get { return GetTimeSpan32(4); } }

        /// <summary>
        /// Gets the estimated error in time reported by the server.
        /// </summary>
        /// <value>
        /// Estimated error in time reported by the server. Normally a positive value smaller than one second.
        /// </value>
        public TimeSpan RootDispersion { get { return GetTimeSpan32(8); } }

        /// <summary>
        /// Gets the ID of the time source used by the server or Kiss-o'-Death code sent by the server.
        /// </summary>
        /// <value>
        /// <para>
        /// ID of server's time source or Kiss-o'-Death code.
        /// Purpose of this property depends on value of <see cref="NtpPacket.Stratum" /> property.
        /// </para>
        /// <para>
        /// Stratum 1 servers write here one of several special values that describe the kind of hardware clock they use.
        /// </para>
        /// <para>
        /// Stratum 2 and lower servers set this property to IPv4 address of their upstream server.
        /// If upstream server has IPv6 address, the address is hashed, because it doesn't fit in this property.
        /// </para>
        /// <para>
        /// When server sets <see cref="NtpPacket.Stratum" /> to special value 0,
        /// this property contains so called kiss code that instructs the client to stop querying the server.
        /// </para>
        /// </value>
        public uint ReferenceId { get { return GetUInt32BE(12); } }

        /// <summary>
        /// Gets or sets the time when the server clock was last set or corrected.
        /// </summary>
        /// <value>
        /// Time when the server clock was last set or corrected or <c>null</c> when not specified.
        /// </value>
        /// <remarks>
        /// This Property is usually set only by servers. It usually lags server's current time by several minutes,
        /// so don't use this property for time synchronization.
        /// </remarks>
        public DateTime? ReferenceTimestamp { get { return GetDateTime64(16); } }

        /// <summary>
        /// Gets or sets the time when the client sent its request.
        /// </summary>
        /// <value>
        /// This property is <c>null</c> in request packets.
        /// In reply packets, it is the time when the client sent its request.
        /// Servers copy this value from <see cref="NtpPacket.TransmitTimestamp" />
        /// that they find in received request packet.
        /// </value>
        /// <seealso cref="NtpPacket.CorrectionOffset" />
        /// <seealso cref="NtpPacket.RoundTripTime" />
        public DateTime? OriginTimestamp { get { return GetDateTime64(24); } }

        /// <summary>
        /// Gets or sets the time when the request was received by the server.
        /// </summary>
        /// <value>
        /// This property is <c>null</c> in request packets.
        /// In reply packets, it is the time when the server received client request.
        /// </value>
        /// <seealso cref="NtpPacket.CorrectionOffset" />
        /// <seealso cref="NtpPacket.RoundTripTime" />
        public DateTime? ReceiveTimestamp { get { return GetDateTime64(32); } }

        /// <summary>
        /// Gets or sets the time when the packet was sent.
        /// </summary>
        /// <value>
        /// Time when the packet was sent. It should never be <c>null</c>.
        /// Default value is <see cref="System.DateTime.UtcNow" />.
        /// </value>
        /// <remarks>
        /// This property must be set by both clients and servers.
        /// </remarks>
        /// <seealso cref="NtpPacket.CorrectionOffset" />
        /// <seealso cref="NtpPacket.RoundTripTime" />
        public DateTime? TransmitTimestamp { get { return GetDateTime64(40); } private set { SetDateTime64(40, value); } }

        /// <summary>
        /// Gets or sets the time of reception of response SNTP packet on the client.
        /// </summary>
        /// <value>
        /// Time of reception of response SNTP packet on the client. It is <c>null</c> in request packets.
        /// </value>
        /// <remarks>
        /// This property is not part of the protocol and has to be set when reply packet is received.
        /// </remarks>
        /// <seealso cref="NtpPacket.CorrectionOffset" />
        /// <seealso cref="NtpPacket.RoundTripTime" />
        public DateTime? DestinationTimestamp { get; private set; }

        /// <summary>
        /// Gets the round-trip time to the server.
        /// </summary>
        /// <value>
        /// Time the request spent traveling to the server plus the time the reply spent traveling back.
        /// This is calculated from timestamps in the packet as <c>(t1 - t0) + (t3 - t2)</c>
        /// where t0 is <see cref="NtpPacket.OriginTimestamp" />,
        /// t1 is <see cref="NtpPacket.ReceiveTimestamp" />,
        /// t2 is <see cref="NtpPacket.TransmitTimestamp" />,
        /// and t3 is <see cref="NtpPacket.DestinationTimestamp" />.
        /// This property throws an exception in request packets.
        /// </value>
        public TimeSpan RoundTripTime
        {
            get
            {
                CheckTimestamps();
                return (ReceiveTimestamp.Value - OriginTimestamp.Value) + (DestinationTimestamp.Value - TransmitTimestamp.Value);
            }
        }

        /// <summary>
        /// Gets the offset that should be added to local time to synchronize it with server time.
        /// </summary>
        /// <value>
        /// Time difference between server and client. It should be added to local time to get server time.
        /// It is calculated from timestamps in the packet as <c>0.5 * ((t1 - t0) - (t3 - t2))</c>
        /// where t0 is <see cref="NtpPacket.OriginTimestamp" />,
        /// t1 is <see cref="NtpPacket.ReceiveTimestamp" />,
        /// t2 is <see cref="NtpPacket.TransmitTimestamp" />,
        /// and t3 is <see cref="NtpPacket.DestinationTimestamp" />.
        /// This property throws an exception in request packets.
        /// </value>
        public TimeSpan CorrectionOffset
        {
            get
            {
                CheckTimestamps();
                return TimeSpan.FromTicks(((ReceiveTimestamp.Value - OriginTimestamp.Value) - (DestinationTimestamp.Value - TransmitTimestamp.Value)).Ticks / 2);
            }
        }

        /// <summary>
        /// Initializes default request packet.
        /// </summary>
        /// <remarks>
        /// Properties <see cref="NtpPacket.Mode" /> and <see cref="NtpPacket.VersionNumber" />
        /// are set appropriately for request packet. Property <see cref="NtpPacket.TransmitTimestamp" />
        /// is set to <see cref="System.DateTime.UtcNow" />.
        /// </remarks>
        public NtpPacket() : this(new byte[48])
        {
            Mode = NtpMode.Client;
            VersionNumber = 4;
            TransmitTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes packet from received data.
        /// </summary>
        internal NtpPacket(byte[] bytes)
        {
            if (bytes.Length < 48)
                throw new ArgumentException("SNTP reply packet must be at least 48 bytes long.", "bytes");
            Bytes = bytes;
        }

        /// <summary>
        /// Initializes packet from data received from a server.
        /// </summary>
        /// <param name="bytes">Data received from the server.</param>
        /// <param name="destinationTimestamp">Utc time of reception of response SNTP packet on the client.</param>
        /// <returns></returns>
        public static NtpPacket FromServerResponse(byte[] bytes, DateTime destinationTimestamp)
        {
            return new NtpPacket(bytes) { DestinationTimestamp = destinationTimestamp };
        }

        internal void ValidateRequest()
        {
            if (Mode != NtpMode.Client)
                throw new InvalidOperationException("This is not a request SNTP packet.");
            if (VersionNumber == 0)
                throw new InvalidOperationException("Protocol version of the request is not specified.");
            if (TransmitTimestamp == null)
                throw new InvalidOperationException("TransmitTimestamp must be set in request packet.");
        }

        internal void ValidateReply()
        {
            if (Mode != NtpMode.Server)
                throw new InvalidOperationException("This is not a reply SNTP packet.");
            if (VersionNumber == 0)
                throw new InvalidOperationException("Protocol version of the reply is not specified.");
            if (Stratum == 0)
                throw new InvalidOperationException(string.Format("Received Kiss-o'-Death SNTP packet with code 0x{0:x}.", ReferenceId));
            if (LeapIndicator == NtpLeapIndicator.AlarmCondition)
                throw new InvalidOperationException("SNTP server has unsynchronized clock.");
            CheckTimestamps();
        }

        private void CheckTimestamps()
        {
            if (OriginTimestamp == null)
                throw new InvalidOperationException("Origin timestamp is missing.");
            if (ReceiveTimestamp == null)
                throw new InvalidOperationException("Receive timestamp is missing.");
            if (TransmitTimestamp == null)
                throw new InvalidOperationException("Transmit timestamp is missing.");
            if (DestinationTimestamp == null)
                throw new InvalidOperationException("Destination timestamp is missing.");
        }

        private DateTime? GetDateTime64(int offset)
        {
            var field = GetUInt64BE(offset);
            if (field == 0)
                return null;
            return new DateTime(Epoch.Ticks + Convert.ToInt64(field * (1.0 / (1L << 32) * 10000000.0)));
        }

        private void SetDateTime64(int offset, DateTime? value)
        {
            SetUInt64BE(offset, value == null ? 0 : Convert.ToUInt64((value.Value.Ticks - Epoch.Ticks) * (0.0000001 * (1L << 32))));
        }

        private TimeSpan GetTimeSpan32(int offset)
        {
            return TimeSpan.FromSeconds(GetInt32BE(offset) / (double)(1 << 16));
        }

        private ulong GetUInt64BE(int offset)
        {
            return SwapEndianness(BitConverter.ToUInt64(Bytes, offset));
        }

        private void SetUInt64BE(int offset, ulong value)
        {
            FastBitConverter.GetBytes(Bytes, offset, SwapEndianness(value));
        }

        private int GetInt32BE(int offset)
        {
            return (int)GetUInt32BE(offset);
        }

        private uint GetUInt32BE(int offset)
        {
            return SwapEndianness(BitConverter.ToUInt32(Bytes, offset));
        }

        private static uint SwapEndianness(uint x)
        {
            return ((x & 0xff) << 24) | ((x & 0xff00) << 8) | ((x & 0xff0000) >> 8) | ((x & 0xff000000) >> 24);
        }

        private static ulong SwapEndianness(ulong x)
        {
            return ((ulong)SwapEndianness((uint)x) << 32) | SwapEndianness((uint)(x >> 32));
        }
    }

    /// <summary>
    /// Represents leap second warning from the server that instructs the client to add or remove leap second.
    /// </summary>
    /// <seealso cref="NtpPacket.LeapIndicator" />
    public enum NtpLeapIndicator
    {
        /// <summary>
        /// No leap second warning. No action required.
        /// </summary>
        NoWarning,

        /// <summary>
        /// Warns the client that the last minute of the current day has 61 seconds.
        /// </summary>
        LastMinuteHas61Seconds,

        /// <summary>
        /// Warns the client that the last minute of the current day has 59 seconds.
        /// </summary>
        LastMinuteHas59Seconds,

        /// <summary>
        /// Special value indicating that the server clock is unsynchronized and the returned time is unreliable.
        /// </summary>
        AlarmCondition
    }

    /// <summary>
    /// Describes SNTP packet mode, i.e. client or server.
    /// </summary>
    /// <seealso cref="NtpPacket.Mode" />
    public enum NtpMode
    {
        /// <summary>
        /// Identifies client-to-server SNTP packet.
        /// </summary>
        Client = 3,

        /// <summary>
        /// Identifies server-to-client SNTP packet.
        /// </summary>
        Server = 4,
    }
}