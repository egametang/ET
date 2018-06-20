// Copyright (C) 2017 ichenq@outlook.com. All rights reserved.
// Distributed under the terms and conditions of the MIT License.
// See accompanying files LICENSE.

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace ETModel
{
    public class KCP
    {
        public const int IKCP_RTO_NDL = 30;         // no delay min rto
        public const int IKCP_RTO_MIN = 100;        // normal min rto
        public const int IKCP_RTO_DEF = 200;
        public const int IKCP_RTO_MAX = 60000;
        public const int IKCP_CMD_PUSH = 81;        // cmd: push data
        public const int IKCP_CMD_ACK = 82;         // cmd: ack
        public const int IKCP_CMD_WASK = 83;        // cmd: window probe (ask)
        public const int IKCP_CMD_WINS = 84;        // cmd: window size (tell)
        public const int IKCP_ASK_SEND = 1;         // need to send IKCP_CMD_WASK
        public const int IKCP_ASK_TELL = 2;         // need to send IKCP_CMD_WINS
        public const int IKCP_WND_SND = 32;
        public const int IKCP_WND_RCV = 32;
        public const int IKCP_MTU_DEF = 1400;
        public const int IKCP_ACK_FAST = 3;
        public const int IKCP_INTERVAL = 100;
        public const int IKCP_OVERHEAD = 24;
        public const int IKCP_DEADLINK = 20;
        public const int IKCP_THRESH_INIT = 2;
        public const int IKCP_THRESH_MIN = 2;
        public const int IKCP_PROBE_INIT = 7000;    // 7 secs to probe window size
        public const int IKCP_PROBE_LIMIT = 120000; // up to 120 secs to probe window

        public const int IKCP_LOG_OUTPUT = 0x1;
        public const int IKCP_LOG_INPUT = 0x2;
        public const int IKCP_LOG_SEND = 0x4;
        public const int IKCP_LOG_RECV = 0x8;
        public const int IKCP_LOG_IN_DATA = 0x10;
        public const int IKCP_LOG_IN_ACK = 0x20;
        public const int IKCP_LOG_IN_PROBE = 0x40;
        public const int IKCP_LOG_IN_WINS = 0x80;
        public const int IKCP_LOG_OUT_DATA = 0x100;
        public const int IKCP_LOG_OUT_ACK = 0x200;
        public const int IKCP_LOG_OUT_PROBE = 0x400;
        public const int IKCP_LOG_OUT_WINS = 0x800;


        // encode 8 bits unsigned int
        public static void ikcp_encode8u(byte[] p, int offset, byte c)
        {
            p[offset] = c;
        }

        // decode 8 bits unsigned int
        public static byte ikcp_decode8u(byte[] p, ref int offset)
        {
            return p[offset++];
        }

        // encode 16 bits unsigned int (lsb)
        public static void ikcp_encode16u(byte[] p, int offset, ushort v)
        {
            p[offset] = (byte)(v & 0xFF);
            p[offset + 1] = (byte)(v >> 8);
        }

        // decode 16 bits unsigned int (lsb)
        public static ushort ikcp_decode16u(byte[] p, ref int offset)
        {
            int pos = offset;
            offset += 2;
            return (ushort)((ushort)p[pos] | (ushort)(p[pos + 1] << 8));
        }

        // encode 32 bits unsigned int (lsb)
        public static void ikcp_encode32u(byte[] p, int offset, uint l)
        {
            p[offset] = (byte)(l & 0xFF);
            p[offset + 1] = (byte)(l >> 8);
            p[offset + 2] = (byte)(l >> 16);
            p[offset + 3] = (byte)(l >> 24);
        }

        // decode 32 bits unsigned int (lsb)
        public static uint ikcp_decode32u(byte[] p, ref int offset)
        {
            int pos = offset;
            offset += 4;
            return ((uint)p[pos] | (uint)(p[pos + 1] << 8)
                | (uint)(p[pos + 2] << 16) | (uint)(p[pos + 3] << 24));
        }

        public static uint _imin_(uint a, uint b)
        {
            return a <= b ? a : b;
        }

        public static uint _imax_(uint a, uint b)
        {
            return a >= b ? a : b;
        }

        public static uint _ibound_(uint lower, uint middle, uint upper)
        {
            return _imin_(_imax_(lower, middle), upper);
        }

        public static int _itimediff(uint later, uint earlier)
        {
            return (int)(later - earlier);
        }

        internal class Segment: IDisposable
        {
            internal uint conv;
            internal uint cmd;
            internal uint frg;
            internal uint wnd;
            internal uint ts;
            internal uint sn;
            internal uint una;
            internal uint resendts;
            internal uint rto;
            internal uint faskack;
            internal uint xmit;
            internal byte[] data { get; }

            internal Segment(int size = 0)
            {
                data = new byte[size];
            }

            internal void Encode(byte[] ptr, ref int offset)
            {
                uint len = (uint)data.Length;
                ikcp_encode32u(ptr, offset, conv);
                ikcp_encode8u(ptr, offset + 4, (byte)cmd);
                ikcp_encode8u(ptr, offset + 5, (byte)frg);
                ikcp_encode16u(ptr, offset + 6, (ushort)wnd);
                ikcp_encode32u(ptr, offset + 8, ts);
                ikcp_encode32u(ptr, offset + 12, sn);
                ikcp_encode32u(ptr, offset + 16, una);
                ikcp_encode32u(ptr, offset + 20, len);
                offset += IKCP_OVERHEAD;
            }

            public void Dispose()
            {
                this.conv = 0;
                this.cmd = 0;
                this.frg = 0;
                this.wnd = 0;
                this.ts = 0;
                this.sn = 0;
                this.una = 0;
                this.resendts = 0;
                this.rto = 0;
                this.faskack = 0;
                this.xmit = 0;
            }
        }

        uint conv_;
        uint mtu_;
        uint mss_;
        uint state_;

        uint snd_una_;
        uint snd_nxt_;
        uint rcv_nxt_;

        uint ts_recent_ = 0;
        uint ts_lastack_ = 0;
        uint ssthresh_;

        int rx_rttval_;
        int rx_srtt_;
        int rx_rto_;
        int rx_minrto_;

        uint snd_wnd_;
        uint rcv_wnd_;
        uint rmt_wnd_;
        uint cwnd_;
        uint probe_;

        uint current_;
        uint interval_;
        uint ts_flush_;
        uint xmit_;

        uint nrcv_buf_;
        uint nsnd_buf_;
        uint nrcv_que_;
        uint nsnd_que_;

        uint nodelay_;
        uint updated_;
        uint ts_probe_;
        uint probe_wait_;
        uint dead_link_;
        uint incr_;

        LinkedList<Segment> snd_queue_;
        LinkedList<Segment> rcv_queue_;
        LinkedList<Segment> snd_buf_;
        LinkedList<Segment> rcv_buf_;

        uint[] acklist_;
        uint ackcount_;
        uint ackblock_;

        byte[] buffer_;
        object user_;

        int fastresend_;
        int nocwnd_;

        public delegate void OutputDelegate(byte[] data, int size, object user);
        OutputDelegate output_;

        // create a new kcp control object, 'conv' must equal in two endpoint
        // from the same connection. 'user' will be passed to the output callback
        // output callback can be setup like this: 'kcp->output = my_udp_output'
        public KCP(uint conv, object user)
        {
            Debug.Assert(BitConverter.IsLittleEndian); // we only support little endian device

            user_ = user;
            conv_ = conv;
            snd_wnd_ = IKCP_WND_SND;
            rcv_wnd_ = IKCP_WND_RCV;
            rmt_wnd_ = IKCP_WND_RCV;
            mtu_ = IKCP_MTU_DEF;
            mss_ = mtu_ - IKCP_OVERHEAD;
            rx_rto_ = IKCP_RTO_DEF;
            rx_minrto_ = IKCP_RTO_MIN;
            interval_ = IKCP_INTERVAL;
            ts_flush_ = IKCP_INTERVAL;
            ssthresh_ = IKCP_THRESH_INIT;
            dead_link_ = IKCP_DEADLINK;
            buffer_ = new byte[(mtu_ + IKCP_OVERHEAD) * 3];
            snd_queue_ = new LinkedList<Segment>();
            rcv_queue_ = new LinkedList<Segment>();
            snd_buf_ = new LinkedList<Segment>();
            rcv_buf_ = new LinkedList<Segment>();
        }

        // release kcp control object
        public void Release()
        {
            snd_buf_.Clear();
            rcv_buf_.Clear();
            snd_queue_.Clear();
            rcv_queue_.Clear();
            nrcv_buf_ = 0;
            nsnd_buf_ = 0;
            nrcv_que_ = 0;
            nsnd_que_ = 0;
            ackblock_ = 0;
            ackcount_ = 0;
            buffer_ = null;
            acklist_ = null;
        }

        // set output callback, which will be invoked by kcp
        public void SetOutput(OutputDelegate output)
        {
            output_ = output;
        }

        // user/upper level recv: returns size, returns below zero for EAGAIN
        public int Recv(byte[] buffer, int offset, int len)
        {
            int ispeek = (len < 0 ? 1 : 0);
            int recover = 0;

            if (rcv_queue_.Count == 0)
                return -1;

            if (len < 0)
                len = -len;

            int peeksize = PeekSize();
            if (peeksize < 0)
                return -2;

            if (peeksize > len)
                return -3;

            if (nrcv_que_ >= rcv_wnd_)
                recover = 1;

            // merge fragment
            len = 0;
            LinkedListNode<Segment> next = null;
            for (var node = rcv_queue_.First; node != null; node = next)
            {
                int fragment = 0;
                var seg = node.Value;
                next = node.Next;
                
                if (buffer != null)
                {
                    Buffer.BlockCopy(seg.data, 0, buffer, offset, seg.data.Length);
                    offset += seg.data.Length;
                }
                len += seg.data.Length;
                fragment = (int)seg.frg;

                Log(IKCP_LOG_RECV, "recv sn={0}", seg.sn);

                if (ispeek == 0)
                {
                    rcv_queue_.Remove(node);
                    nrcv_que_--;
                }

                if (fragment == 0)
                    break;
            }

            Debug.Assert(len == peeksize);

            // move available data from rcv_buf -> rcv_queue
            while (rcv_buf_.Count > 0)
            {
                var node = rcv_buf_.First;
                var seg = node.Value;
                if (seg.sn == rcv_nxt_ && nrcv_que_ < rcv_wnd_)
                {
                    rcv_buf_.Remove(node);
                    nrcv_buf_--;
                    rcv_queue_.AddLast(node);
                    nrcv_que_++;
                    rcv_nxt_++;
                }
                else
                {
                    break;
                }
            }

            // fast recover
            if (nrcv_que_ < rcv_wnd_ && recover != 0)
            {
                // ready to send back IKCP_CMD_WINS in ikcp_flush
                // tell remote my window size
                probe_ |= IKCP_ASK_TELL;
            }

            return len;
        }

        // check the size of next message in the recv queue
        public int PeekSize()
        {
            if (rcv_queue_.Count == 0)
                return -1;

            var node = rcv_queue_.First;
            var seg = node.Value;
            if (seg.frg == 0)
                return seg.data.Length;

            if (nrcv_que_ < seg.frg + 1)
                return -1;

            int length = 0;
            for (node = rcv_queue_.First; node != null; node = node.Next)
            {
                seg = node.Value;
                length += seg.data.Length;
                if (seg.frg == 0)
                    break;
            }
            return length;
        }

        // user/upper level send, returns below zero for error
        public int Send(byte[] buffer, int offset, int len)
        {
            Debug.Assert(mss_ > 0);
            if (len < 0)
                return -1;

            //
            // not implement streaming mode here as ikcp.c
            //

            int count = 0;
            if (len <= (int)mss_)
                count = 1;
            else
                count = (len + (int)mss_ - 1) / (int)mss_;

            if (count > 255) // maximum value `frg` can present
                return -2;

            if (count == 0)
                count = 1;

            // fragment
            for (int i = 0; i < count; i++)
            {
                int size = len > (int)mss_ ? (int)mss_ : len;
                var seg = new Segment(size);
                if (buffer != null && len > 0)
                {
                    Buffer.BlockCopy(buffer, offset, seg.data, 0, size);
                    offset += size;
                }
                seg.frg = (uint)(count - i - 1);
                snd_queue_.AddLast(seg);
                nsnd_que_++;
                len -= size;
            }
            return 0;
        }

        // parse ack
        void UpdateACK(int rtt)
        {
            if (rx_srtt_ == 0)
            {
                rx_srtt_ = rtt;
                rx_rttval_ = rtt / 2;
            }
            else
            {
                int delta = rtt - rx_srtt_;
                if (delta < 0)
                    delta = -delta;

                rx_rttval_ = (3 * rx_rttval_ + delta) / 4;
                rx_srtt_ = (7 * rx_srtt_ + rtt) / 8;
                if (rx_srtt_ < 1)
                    rx_srtt_ = 1;
            }

            var rto = rx_srtt_ + _imax_(interval_, (uint)(4 * rx_rttval_));
            rx_rto_ = (int)_ibound_((uint)rx_minrto_, (uint)rto, IKCP_RTO_MAX);
        }

        void ShrinkBuf()
        {
            var node = snd_buf_.First;
            if (node != null)
            {
                var seg = node.Value;
                snd_una_ = seg.sn;
            }
            else
            {
                snd_una_ = snd_nxt_;
            }
        }

        void ParseACK(uint sn)
        {
            if (_itimediff(sn, snd_una_) < 0 || _itimediff(sn, snd_nxt_) >= 0)
                return;

            LinkedListNode<Segment> next = null;
            for (var node = snd_buf_.First; node != null; node = next)
            {
                var seg = node.Value;
                next = node.Next;
                if (sn == seg.sn)
                {
                    snd_buf_.Remove(node);
                    nsnd_buf_--;
                    break;
                }
                if (_itimediff(sn, seg.sn) < 0)
                    break;
            }
        }

        void ParseUNA(uint una)
        {
            LinkedListNode<Segment> next = null;
            for (var node = snd_buf_.First; node != null; node = next)
            {
                var seg = node.Value;
                next = node.Next;
                if (_itimediff(una, seg.sn) > 0)
                {
                    snd_buf_.Remove(node);
                    nsnd_buf_--;
                }
                else
                {
                    break;
                }
            }
        }

        void ParseFastACK(uint sn)
        {
            if (_itimediff(sn, snd_una_) < 0 || _itimediff(sn, snd_nxt_) >= 0)
                return;

            LinkedListNode<Segment> next = null;
            for (var node = snd_buf_.First; node != null; node = next)
            {
                var seg = node.Value;
                next = node.Next;
                if (_itimediff(sn, seg.sn) < 0)
                {
                    break;
                }
                else if (sn != seg.sn)
                {
                    seg.faskack++;
                }
            }
        }

        // ack append
        void ACKPush(uint sn, uint ts)
        {
            var newsize = ackcount_ + 1;
            if (newsize > ackblock_)
            {
                uint newblock = 8;
                for (; newblock < newsize; newblock <<= 1)
                    ;

                var acklist = new uint[newblock * 2];
                if (acklist_ != null)
                {
                    for (var i = 0; i < ackcount_; i++)
                    {
                        acklist[i * 2] = acklist_[i * 2];
                        acklist[i * 2 + 1] = acklist_[i * 2 + 1];
                    }
                }
                acklist_ = acklist;
                ackblock_ = newblock;
            }
            acklist_[ackcount_ * 2] = sn;
            acklist_[ackcount_ * 2 + 1] = ts;
            ackcount_++;
        }

        void ACKGet(int pos, ref uint sn, ref uint ts)
        {
            sn = acklist_[pos * 2];
            ts = acklist_[pos * 2 + 1];
        }

        // parse data
        void ParseData(Segment newseg)
        {
            uint sn = newseg.sn;
            int repeat = 0;

            if (_itimediff(sn, rcv_nxt_ + rcv_wnd_) >= 0 ||
                _itimediff(sn, rcv_nxt_) < 0)
            {
                return;
            }

            LinkedListNode<Segment> node = null;
            LinkedListNode<Segment> prev = null;
            for (node = rcv_buf_.Last; node != null; node = prev)
            {
                var seg = node.Value;
                prev = node.Previous;
                if (seg.sn == sn)
                {
                    repeat = 1;
                    break;
                }
                if (_itimediff(sn, seg.sn) > 0) 
                {
                    break;
                }
            }
            if (repeat == 0)
            {
                if (node != null)
                {
                    rcv_buf_.AddAfter(node, newseg);
                }
                else
                {
                    rcv_buf_.AddFirst(newseg);
                }
                nrcv_buf_++;
            }

            // move available data from rcv_buf -> rcv_queue
            while (rcv_buf_.Count > 0)
            {
                node = rcv_buf_.First;
                var seg = node.Value;
                if (seg.sn == rcv_nxt_ && nrcv_que_ < rcv_wnd_)
                {
                    rcv_buf_.Remove(node);
                    nrcv_buf_--;
                    rcv_queue_.AddLast(node);
                    nrcv_que_++;
                    rcv_nxt_++;
                }
                else
                {
                    break;
                }
            }
        }

        // when you received a low level packet (eg. UDP packet), call it
        public int Input(byte[] data, int offset, int size)
        {
            uint maxack = 0;
            int flag = 0;

            Log(IKCP_LOG_INPUT, "[RI] {0} bytes", size);

            if (data == null || size < IKCP_OVERHEAD)
                return -1;

            while (true)
            {
                if (size < IKCP_OVERHEAD)
                    break;

                uint conv = ikcp_decode32u(data, ref offset);
                //if (conv_ != conv)
                //    return -1;
                uint cmd = ikcp_decode8u(data, ref offset);
                uint frg = ikcp_decode8u(data, ref offset);
                uint wnd = ikcp_decode16u(data, ref offset);
                uint ts = ikcp_decode32u(data, ref offset);
                uint sn = ikcp_decode32u(data, ref offset);
                uint una = ikcp_decode32u(data, ref offset);
                uint len = ikcp_decode32u(data, ref offset);

                size -= IKCP_OVERHEAD;
                if (size < len)
                    return -2;

                if (cmd != IKCP_CMD_PUSH && cmd != IKCP_CMD_ACK &&
                    cmd != IKCP_CMD_WASK && cmd != IKCP_CMD_WINS)
                    return -3;

                rmt_wnd_ = wnd;
                ParseUNA(una);
                ShrinkBuf();

                if (cmd == IKCP_CMD_ACK)
                {
                    if (_itimediff(current_, ts) >= 0)
                    {
                        UpdateACK(_itimediff(current_, ts));
                    }
                    ParseACK(sn);
                    ShrinkBuf();
                    if (flag == 0)
                    {
                        flag = 1;
                        maxack = sn;
                    }
                    else
                    {
                        if (_itimediff(sn, maxack) > 0)
                        {
                            maxack = sn;
                        }
                    }
                    Log(IKCP_LOG_IN_DATA, "input ack: sn={0} rtt={1} rto={2}",
                        sn, _itimediff(current_, ts), rx_rto_);
                }
                else if (cmd == IKCP_CMD_PUSH)
                {
                    Log(IKCP_LOG_IN_DATA, "input psh: sn={0} ts={1}", sn, ts);
                    if (_itimediff(sn, rcv_nxt_ + rcv_wnd_) < 0)
                    {
                        ACKPush(sn, ts);
                        if (_itimediff(sn, rcv_nxt_) >= 0)
                        {
                            var seg = new Segment((int)len);
                            seg.conv = conv;
                            seg.cmd = cmd;
                            seg.frg = frg;
                            seg.wnd = wnd;
                            seg.ts = ts;
                            seg.sn = sn;
                            seg.una = una;
                            if (len > 0)
                            {
                                Buffer.BlockCopy(data, offset, seg.data, 0, (int)len);
                            }
                            ParseData(seg);
                        }
                    }
                }
                else if (cmd == IKCP_CMD_WASK)
                {
                    // ready to send back IKCP_CMD_WINS in ikcp_flush
                    // tell remote my window size
                    probe_ |= IKCP_ASK_TELL;
                    Log(IKCP_LOG_IN_PROBE, "input probe");
                }
                else if (cmd == IKCP_CMD_WINS)
                {
                    // do nothing
                    Log(IKCP_LOG_IN_WINS, "input wins: {0}", wnd);
                }
                else
                {
                    return -3;
                }

                offset += (int)len;
                size -= (int)len;
            }

            if (flag != 0)
            {
                ParseFastACK(maxack);
            }

            uint unack = snd_una_;
            if (_itimediff(snd_una_, unack) > 0)
            {
                if (cwnd_ < rmt_wnd_)
                {
                    if (cwnd_ < ssthresh_)
                    {
                        cwnd_++;
                        incr_ += mss_;
                    }
                    else
                    {
                        if (incr_ < mss_)
                            incr_ = mss_;
                        incr_ += (mss_ * mss_) / incr_ + (mss_ / 16);
                        if ((cwnd_ + 1) * mss_ <= incr_)
                            cwnd_++;
                    }
                    if (cwnd_ > rmt_wnd_)
                    {
                        cwnd_ = rmt_wnd_;
                        incr_ = rmt_wnd_ * mss_;
                    }
                }
            }

            return 0;
        }

        int WndUnused()
        {
            if (nrcv_que_ < rcv_wnd_)
                return (int)(rcv_wnd_ - nrcv_que_);
            return 0;
        }

        // flush pending data
        void Flush()
        {
            int change = 0;
            int lost = 0;
            int offset = 0;

            // 'ikcp_update' haven't been called. 
            if (updated_ == 0)
                return;

            var seg = new Segment
            {
                conv = conv_,
                cmd = IKCP_CMD_ACK,
                wnd = (uint)WndUnused(),
                una = rcv_nxt_,
            };

            // flush acknowledges
            int count = (int)ackcount_;
            for (int i = 0; i < count; i++)
            {
                if ((offset + IKCP_OVERHEAD) > mtu_)
                {
                    output_(buffer_, offset, user_);
                    offset = 0;
                }
                ACKGet(i, ref seg.sn, ref seg.ts);
                seg.Encode(buffer_, ref offset);
            }

            ackcount_ = 0;

            // probe window size (if remote window size equals zero)
            if (rmt_wnd_ == 0)
            {
                if (probe_wait_ == 0)
                {
                    probe_wait_ = IKCP_PROBE_INIT;
                    ts_probe_ = current_ + probe_wait_;
                }
                else
                {
                    if (_itimediff(current_, ts_probe_) >= 0)
                    {
                        if (probe_wait_ < IKCP_PROBE_INIT)
                            probe_wait_ = IKCP_PROBE_INIT;
                        probe_wait_ += probe_wait_ / 2;
                        if (probe_wait_ > IKCP_PROBE_LIMIT)
                            probe_wait_ = IKCP_PROBE_LIMIT;
                        ts_probe_ = current_ + probe_wait_;
                        probe_ |= IKCP_ASK_SEND;
                    }
                }
            }
            else
            {
                ts_probe_ = 0;
                probe_wait_ = 0;
            }

            // flush window probing commands
            if ((probe_ & IKCP_ASK_SEND) > 0)
            {
                seg.cmd = IKCP_CMD_WASK;
                if ((offset + IKCP_OVERHEAD) > mtu_)
                {
                    output_(buffer_, offset, user_);
                    offset = 0;
                }
                seg.Encode(buffer_, ref offset);
            }

            // flush window probing commands
            if ((probe_ & IKCP_ASK_TELL) > 0)
            {
                seg.cmd = IKCP_CMD_WINS;
                if ((offset + IKCP_OVERHEAD) > mtu_)
                {
                    output_(buffer_, offset, user_);
                    offset = 0;
                }
                seg.Encode(buffer_, ref offset);
            }

            probe_ = 0;

            // calculate window size
            uint cwnd = _imin_(snd_wnd_, rmt_wnd_);
            if (nocwnd_ == 0)
                cwnd = _imin_(cwnd_, cwnd);

            // move data from snd_queue to snd_buf
            while (_itimediff(snd_nxt_, snd_una_ + cwnd) < 0)
            {
                if (snd_queue_.Count == 0)
                    break;

                var node = snd_queue_.First;
                var newseg = node.Value;
                snd_queue_.Remove(node);
                snd_buf_.AddLast(node);
                nsnd_que_--;
                nsnd_buf_++;

                newseg.conv = conv_;
                newseg.cmd = IKCP_CMD_PUSH;
                newseg.wnd = seg.wnd;
                newseg.ts = current_;
                newseg.sn = snd_nxt_++;
                newseg.una = rcv_nxt_;
                newseg.resendts = current_;
                newseg.rto = (uint)rx_rto_;
                newseg.faskack = 0;
                newseg.xmit = 0;
            }

            // calculate resent
            uint resent = (fastresend_ > 0 ? (uint)fastresend_ : 0xffffffff);
            uint rtomin = (nodelay_ == 0 ? (uint)(rx_rto_ >> 3) : 0);

            // flush data segments
            for (var node = snd_buf_.First; node != null; node = node.Next)
            {
                var segment = node.Value;
                int needsend = 0;
                if (segment.xmit == 0)
                {
                    needsend = 1;
                    segment.xmit++;
                    segment.rto = (uint)rx_rto_;
                    segment.resendts = current_ + segment.rto + rtomin;
                }
                else if (_itimediff(current_, segment.resendts) >= 0)
                {
                    needsend = 1;
                    segment.xmit++;
                    xmit_++;
                    if (nodelay_ == 0)
                        segment.rto += (uint)rx_rto_;
                    else
                        segment.rto += (uint)rx_rto_ / 2;
                    segment.resendts = current_ + segment.rto;
                    lost = 1;
                }
                else if (segment.faskack >= resent)
                {
                    needsend = 1;
                    segment.xmit++;
                    segment.faskack = 0;
                    segment.resendts = current_ + segment.rto;
                    change++;
                }

                if (needsend > 0)
                {
                    segment.ts = current_;
                    segment.wnd = seg.wnd;
                    segment.una = rcv_nxt_;

                    int need = IKCP_OVERHEAD;
                    if (segment.data != null)
                        need += segment.data.Length;

                    if (offset + need > mtu_)
                    {
                        output_(buffer_, offset, user_);
                        offset = 0;
                    }
                    segment.Encode(buffer_, ref offset);
                    if (segment.data.Length > 0)
                    {
                        Buffer.BlockCopy(segment.data, 0, buffer_, offset, segment.data.Length);
                        offset += segment.data.Length;
                    }
                    if (segment.xmit >= dead_link_)
                        state_ = 0xffffffff;
                }
            }

            // flush remain segments
            if (offset > 0)
            {
                output_(buffer_, offset, user_);
                offset = 0;
            }

            // update ssthresh
            if (change > 0)
            {
                uint inflight = snd_nxt_ - snd_una_;
                ssthresh_ = inflight / 2;
                if (ssthresh_ < IKCP_THRESH_MIN)
                    ssthresh_ = IKCP_THRESH_MIN;
                cwnd_ = ssthresh_ + resent;
                incr_ = cwnd_ * mss_;
            }

            if (lost > 0)
            {
                ssthresh_ = cwnd / 2;
                if (ssthresh_ < IKCP_THRESH_MIN)
                    ssthresh_ = IKCP_THRESH_MIN;
                cwnd_ = 1;
                incr_ = mss_;
            }

            if (cwnd_ < 1)
            {
                cwnd_ = 1;
                incr_ = mss_;
            }
        }

        // update state (call it repeatedly, every 10ms-100ms), or you can ask 
        // ikcp_check when to call it again (without ikcp_input/_send calling).
        // 'current' - current timestamp in millisec. 
        public void Update(uint current)
        {
            current_ = current;
            if (updated_ == 0)
            {
                updated_ = 1;
                ts_flush_ = current;
            }

            int slap = _itimediff(current_, ts_flush_);
            if (slap >= 10000 || slap < -10000)
            {
                ts_flush_ = current;
                slap = 0;
            }

            if (slap >= 0)
            {
                ts_flush_ += interval_;
                if (_itimediff(current_, ts_flush_) >= 0)
                    ts_flush_ = current_ + interval_;

                Flush();
            }
        }

        // Determine when should you invoke ikcp_update:
        // returns when you should invoke ikcp_update in millisec, if there 
        // is no ikcp_input/_send calling. you can call ikcp_update in that
        // time, instead of call update repeatly.
        // Important to reduce unnacessary ikcp_update invoking. use it to 
        // schedule ikcp_update (eg. implementing an epoll-like mechanism, 
        // or optimize ikcp_update when handling massive kcp connections)
        public uint Check(uint current)
        {
            uint ts_flush = ts_flush_;
            int tm_flush = 0x7fffffff;
            int tm_packet = 0x7fffffff;

            if (updated_ == 0)
                return current;

            if (_itimediff(current, ts_flush) >= 10000 || 
                _itimediff(current, ts_flush) < -10000)
            {
                ts_flush = current;
            }

            if (_itimediff(current, ts_flush) >= 0)
                return current;

            tm_flush = _itimediff(ts_flush, current);

            for (var node = snd_buf_.First; node != null; node = node.Next)
            {
                var seg = node.Value;
                int diff = _itimediff(seg.resendts, current);
                if (diff <= 0)
                    return current;

                if (diff < tm_packet)
                    tm_packet = diff;
            }

            uint minimal = (uint)(tm_packet < tm_flush ? tm_packet : tm_flush);
            if (minimal >= interval_)
                minimal = interval_;

            return current + minimal;
        }

        // change MTU size, default is 1400
        public int SetMTU(int mtu)
        {
            if (mtu < 50 || mtu < IKCP_OVERHEAD)
                return -1;

            var buffer = new byte[(mtu + IKCP_OVERHEAD) * 3];
            mtu_ = (uint)mtu;
            mss_ = mtu_ - IKCP_OVERHEAD;
            buffer_ = buffer;
            return 0;
        }

        public int Interval(int interval)
        {
            if (interval > 5000)
                interval = 5000;
            else if (interval < 10)
                interval = 10;

            interval_ = (uint)interval;
            return 0;
        }

        // fastest: ikcp_nodelay(kcp, 1, 20, 2, 1)
        // nodelay: 0:disable(default), 1:enable
        // interval: internal update timer interval in millisec, default is 100ms 
        // resend: 0:disable fast resend(default), 1:enable fast resend
        // nc: 0:normal congestion control(default), 1:disable congestion control
        public int NoDelay(int nodelay, int interval, int resend, int nc)
        {
            if (nodelay >= 0)
            {
                nodelay_ = (uint)nodelay;
                if (nodelay > 0)
                {
                    rx_minrto_ = IKCP_RTO_NDL;
                }
                else
                {
                    rx_minrto_ = IKCP_RTO_MIN;
                }
            }
            if (interval >= 0)
            {
                if (interval > 5000)
                    interval = 5000;
                else if (interval < 10)
                    interval = 10;

                interval_ = (uint)interval;
            }

            if (resend >= 0)
                fastresend_ = resend;

            if (nc >= 0)
                nocwnd_ = nc;

            return 0;
        }

        // set maximum window size: sndwnd=32, rcvwnd=32 by default
        public int WndSize(int sndwnd, int rcvwnd)
        {
            if (sndwnd > 0)
                snd_wnd_ = (uint)sndwnd;
            if (rcvwnd > 0)
                rcv_wnd_ = (uint)rcvwnd;
            return 0;
        }

        // get how many packet is waiting to be sent
        public int WaitSnd()
        {
            return (int)(nsnd_buf_ + nsnd_que_);
        }

        // read conv
        public uint GetConv()
        {
            return conv_;
        }

        public uint GetState()
        {
            return state_;
        }

        public void SetMinRTO(int minrto)
        {
            rx_minrto_ = minrto;
        }

        public void SetFastResend(int resend)
        {
            fastresend_ = resend;
        }

        void Log(int mask, string format, params object[] args)
        {
            // Console.WriteLine(mask + String.Format(format, args));
        }
    }
}
