using System;
using System.Threading;
using static ET.IKCP;

#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8625

// ReSharper disable IdentifierTypo
// ReSharper disable GrammarMistakeInComment
// ReSharper disable PossibleNullReferenceException
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

namespace ET
{
    /// <summary>
    ///     Kcp
    /// </summary>
    public sealed unsafe partial class Kcp : IDisposable
    {
        /// <summary>
        ///     Kcp
        /// </summary>
        private IKCPCB* _kcp;

        /// <summary>
        ///     Output function
        /// </summary>
        private KcpCallback _output;

        /// <summary>
        ///     Buffer
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        ///     Disposed
        /// </summary>
        private int _disposed;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="output">Output</param>
        public Kcp(KcpCallback output) : this(0, output)
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="conv">ConversationId</param>
        /// <param name="output">Output</param>
        public Kcp(uint conv, KcpCallback output)
        {
            _kcp = ikcp_create(conv, ref _buffer);
            _output = output;
        }

        /// <summary>
        ///     Set
        /// </summary>
        public bool IsSet => _kcp != null;

        /// <summary>
        ///     Conversation id
        /// </summary>
        public uint ConversationId => _kcp->conv;

        /// <summary>
        ///     Maximum transmission unit
        /// </summary>
        public uint MaximumTransmissionUnit => _kcp->mtu;

        /// <summary>
        ///     Maximum segment size
        /// </summary>
        public uint MaximumSegmentSize => _kcp->mss;

        /// <summary>
        ///     Connection state
        /// </summary>
        public int State => _kcp->state;

        /// <summary>
        ///     The sequence number of the first unacknowledged packet
        /// </summary>
        public uint SendUna => _kcp->snd_una;

        /// <summary>
        ///     The sequence number for the next packet to be sent
        /// </summary>
        public uint SendNext => _kcp->snd_nxt;

        /// <summary>
        ///     The sequence number for the next packet expected to be received
        /// </summary>
        public uint ReceiveNext => _kcp->rcv_nxt;

        /// <summary>
        ///     Slow start threshold for congestion control
        /// </summary>
        public uint SlowStartThreshold => _kcp->ssthresh;

        /// <summary>
        ///     Round-trip time variance
        /// </summary>
        public int RxRttval => _kcp->rx_rttval;

        /// <summary>
        ///     Smoothed round-trip time
        /// </summary>
        public int RxSrtt => _kcp->rx_srtt;

        /// <summary>
        ///     Retransmission timeout
        /// </summary>
        public int RxRto => _kcp->rx_rto;

        /// <summary>
        ///     Minimum retransmission timeout
        /// </summary>
        public int RxMinrto => _kcp->rx_minrto;

        /// <summary>
        ///     Send window size
        /// </summary>
        public uint SendWindowSize => _kcp->snd_wnd;

        /// <summary>
        ///     Receive window size
        /// </summary>
        public uint ReceiveWindowSize => _kcp->rcv_wnd;

        /// <summary>
        ///     Remote window size
        /// </summary>
        public uint RemoteWindowSize => _kcp->rmt_wnd;

        /// <summary>
        ///     Congestion window size
        /// </summary>
        public uint CongestionWindowSize => _kcp->cwnd;

        /// <summary>
        ///     Probe variable for fast recovery
        /// </summary>
        public uint Probe => _kcp->probe;

        /// <summary>
        ///     Current timestamp
        /// </summary>
        public uint Current => _kcp->current;

        /// <summary>
        ///     Flush interval
        /// </summary>
        public uint Interval => _kcp->interval;

        /// <summary>
        ///     Timestamp for the next flush
        /// </summary>
        public uint TimestampFlush => _kcp->ts_flush;

        /// <summary>
        ///     Number of retransmissions
        /// </summary>
        public uint Transmissions => _kcp->xmit;

        /// <summary>
        ///     Number of packets in the receive buffer
        /// </summary>
        public uint ReceiveBufferCount => _kcp->nrcv_buf;

        /// <summary>
        ///     Number of packets in the receive queue
        /// </summary>
        public uint ReceiveQueueCount => _kcp->nrcv_que;

        /// <summary>
        ///     Number of packets wait to receive
        /// </summary>
        public uint WaitReceiveCount => _kcp->nrcv_buf + _kcp->nrcv_que;

        /// <summary>
        ///     Number of packets in the send buffer
        /// </summary>
        public uint SendBufferCount => _kcp->nsnd_buf;

        /// <summary>
        ///     Number of packets in the send queue
        /// </summary>
        public uint SendQueueCount => _kcp->nsnd_que;

        /// <summary>
        ///     Number of packets wait to send
        /// </summary>
        public uint WaitSendCount => _kcp->nsnd_buf + _kcp->nsnd_que;

        /// <summary>
        ///     Whether Nagle's algorithm is disabled
        /// </summary>
        public uint NoDelay => _kcp->nodelay;

        /// <summary>
        ///     Whether the KCP connection has been updated
        /// </summary>
        public uint Updated => _kcp->updated;

        /// <summary>
        ///     Timestamp for the next probe
        /// </summary>
        public uint TimestampProbe => _kcp->ts_probe;

        /// <summary>
        ///     Probe wait time
        /// </summary>
        public uint ProbeWait => _kcp->probe_wait;

        /// <summary>
        ///     Incremental increase
        /// </summary>
        public uint Increment => _kcp->incr;

        /// <summary>
        ///     Pointer to the acknowledge list
        /// </summary>
        public uint* AckList => _kcp->acklist;

        /// <summary>
        ///     Count of acknowledges
        /// </summary>
        public uint AckCount => _kcp->ackcount;

        /// <summary>
        ///     Number of acknowledge blocks
        /// </summary>
        public uint AckBlock => _kcp->ackblock;

        /// <summary>
        ///     Buffer
        /// </summary>
        public byte[] Buffer => _buffer;

        /// <summary>
        ///     Fast resend trigger count
        /// </summary>
        public int FastResend => _kcp->fastresend;

        /// <summary>
        ///     Fast resend limit
        /// </summary>
        public int FastResendLimit => _kcp->fastlimit;

        /// <summary>
        ///     Whether congestion control is disabled
        /// </summary>
        public int NoCongestionWindow => _kcp->nocwnd;

        /// <summary>
        ///     Whether stream mode is enabled
        /// </summary>
        public int StreamMode => _kcp->stream;

        /// <summary>
        ///     Output function pointer
        /// </summary>
        public KcpCallback Output => _output;

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;
            ikcp_release(_kcp);
            _kcp = null;
            _output = null;
            _buffer = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Set output
        /// </summary>
        /// <param name="output">Output</param>
        public void SetOutput(KcpCallback output) => _output = output;

        /// <summary>
        ///     Destructure
        /// </summary>
        ~Kcp() => Dispose();

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Sent bytes</returns>
        public int Send(byte[] buffer)
        {
            fixed (byte* src = &buffer[0])
                return ikcp_send(_kcp, src, buffer.Length);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Sent bytes</returns>
        public int Send(byte[] buffer, int length)
        {
            fixed (byte* src = &buffer[0])
                return ikcp_send(_kcp, src, length);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Sent bytes</returns>
        public int Send(byte[] buffer, int offset, int length)
        {
            fixed (byte* src = &buffer[offset])
                return ikcp_send(_kcp, src, length);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Sent bytes</returns>
        public int Send(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* src = &buffer[0])
                return ikcp_send(_kcp, src, buffer.Length);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Sent bytes</returns>
        public int Send(ReadOnlyMemory<byte> buffer)
        {
            fixed (byte* src = &buffer.Span[0])
                return ikcp_send(_kcp, src, buffer.Length);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Sent bytes</returns>
        public int Send(ArraySegment<byte> buffer)
        {
            fixed (byte* src = &buffer.Array[buffer.Offset])
                return ikcp_send(_kcp, src, buffer.Count);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Sent bytes</returns>
        public int Send(byte* buffer, int length) => ikcp_send(_kcp, buffer, length);

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Sent bytes</returns>
        public int Send(byte* buffer, int offset, int length) => ikcp_send(_kcp, buffer + offset, length);

        /// <summary>
        ///     Input
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Input bytes</returns>
        public int Input(byte[] buffer)
        {
            fixed (byte* src = &buffer[0])
                return ikcp_input(_kcp, src, buffer.Length);
        }

        /// <summary>
        ///     Input
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Input bytes</returns>
        public int Input(byte[] buffer, int length)
        {
            fixed (byte* src = &buffer[0])
                return ikcp_input(_kcp, src, length);
        }

        /// <summary>
        ///     Input
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Input bytes</returns>
        public int Input(byte[] buffer, int offset, int length)
        {
            fixed (byte* src = &buffer[offset])
                return ikcp_input(_kcp, src, length);
        }

        /// <summary>
        ///     Input
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Input bytes</returns>
        public int Input(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* src = &buffer[0])
                return ikcp_input(_kcp, src, buffer.Length);
        }

        /// <summary>
        ///     Input
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Input bytes</returns>
        public int Input(ReadOnlyMemory<byte> buffer)
        {
            fixed (byte* src = &buffer.Span[0])
                return ikcp_input(_kcp, src, buffer.Length);
        }

        /// <summary>
        ///     Input
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Input bytes</returns>
        public int Input(ArraySegment<byte> buffer)
        {
            fixed (byte* src = &buffer.Array[buffer.Offset])
                return ikcp_input(_kcp, src, buffer.Count);
        }

        /// <summary>
        ///     Input
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Input bytes</returns>
        public int Input(byte* buffer, int length) => ikcp_input(_kcp, buffer, length);

        /// <summary>
        ///     Input
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Input bytes</returns>
        public int Input(byte* buffer, int offset, int length) => ikcp_input(_kcp, buffer + offset, length);

        /// <summary>
        ///     Peek size
        /// </summary>
        /// <returns>Peeked size</returns>
        public int PeekSize() => ikcp_peeksize(_kcp);

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Received bytes</returns>
        public int Receive(byte[] buffer)
        {
            fixed (byte* dest = &buffer[0])
                return ikcp_recv(_kcp, dest, buffer.Length);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Received bytes</returns>
        public int Receive(byte[] buffer, int length)
        {
            fixed (byte* dest = &buffer[0])
                return ikcp_recv(_kcp, dest, length);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Received bytes</returns>
        public int Receive(byte[] buffer, int offset, int length)
        {
            fixed (byte* dest = &buffer[offset])
                return ikcp_recv(_kcp, dest, length);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Received bytes</returns>
        public int Receive(Span<byte> buffer)
        {
            fixed (byte* dest = &buffer[0])
                return ikcp_recv(_kcp, dest, buffer.Length);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Received bytes</returns>
        public int Receive(Memory<byte> buffer)
        {
            fixed (byte* dest = &buffer.Span[0])
                return ikcp_recv(_kcp, dest, buffer.Length);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Received bytes</returns>
        public int Receive(ArraySegment<byte> buffer)
        {
            fixed (byte* dest = &buffer.Array[buffer.Offset])
                return ikcp_recv(_kcp, dest, buffer.Count);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Received bytes</returns>
        public int Receive(byte* buffer, int length) => ikcp_recv(_kcp, buffer, length);

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Received bytes</returns>
        public int Receive(byte* buffer, int offset, int length) => ikcp_recv(_kcp, buffer + offset, length);

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="current">Timestamp</param>
        public void Update(uint current) => ikcp_update(_kcp, current, _output, _buffer);

        /// <summary>
        ///     Check
        /// </summary>
        /// <param name="current">Timestamp</param>
        /// <returns>Next flush timestamp</returns>
        public uint Check(uint current) => ikcp_check(_kcp, current);

        /// <summary>
        ///     Flush
        /// </summary>
        public void Flush() => ikcp_flush(_kcp, _output, _buffer);

        /// <summary>
        ///     Set maximum transmission unit
        /// </summary>
        /// <param name="mtu">Maximum transmission unit</param>
        /// <returns>Set</returns>
        public int SetMtu(int mtu) => ikcp_setmtu(_kcp, mtu, ref _buffer);

        /// <summary>
        ///     Set flush interval
        /// </summary>
        /// <param name="interval">Flush interval</param>
        public void SetInterval(int interval) => ikcp_interval(_kcp, interval);

        /// <summary>
        ///     Set no delay
        /// </summary>
        /// <param name="nodelay">Whether Nagle's algorithm is disabled</param>
        /// <param name="interval">Flush interval</param>
        /// <param name="resend">Fast resend trigger count</param>
        /// <param name="nc">No congestion window</param>
        public void SetNoDelay(int nodelay, int interval, int resend, int nc) => ikcp_nodelay(_kcp, nodelay, interval, resend, nc);

        /// <summary>
        ///     Set window size
        /// </summary>
        /// <param name="sndwnd">Send window size</param>
        /// <param name="rcvwnd">Receive window size</param>
        public void SetWindowSize(int sndwnd, int rcvwnd) => ikcp_wndsize(_kcp, sndwnd, rcvwnd);

        /// <summary>
        ///     Set fast resend limit
        /// </summary>
        /// <param name="fastlimit">Fast resend limit</param>
        public void SetFastResendLimit(int fastlimit) => ikcp_fastresendlimit(_kcp, fastlimit);

        /// <summary>
        ///     Set whether stream mode is enabled
        /// </summary>
        /// <param name="stream">Whether stream mode is enabled</param>
        public void SetStreamMode(int stream) => ikcp_streammode(_kcp, stream);

        /// <summary>
        ///     Set minimum retransmission timeout
        /// </summary>
        /// <param name="minrto">Minimum retransmission timeout</param>
        public void SetMinrto(int minrto) => ikcp_minrto(_kcp, minrto);
    }
}