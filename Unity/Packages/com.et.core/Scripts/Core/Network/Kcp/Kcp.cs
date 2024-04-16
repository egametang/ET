// Kcp based on https://github.com/skywind3000/kcp
// Kept as close to original as possible.
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ET
{
    public partial class Kcp
    {
        // original Kcp has a define option, which is not defined by default:
        // #define FASTACK_CONSERVE

        public const int RTO_NDL = 30;             // no delay min rto
        public const int RTO_MIN = 100;            // normal min rto
        public const int RTO_DEF = 200;            // default RTO
        public const int RTO_MAX = 60000;          // maximum RTO
        public const int CMD_PUSH = 81;            // cmd: push data
        public const int CMD_ACK  = 82;            // cmd: ack
        public const int CMD_WASK = 83;            // cmd: window probe (ask)
        public const int CMD_WINS = 84;            // cmd: window size (tell/insert)
        public const int ASK_SEND = 1;             // need to send CMD_WASK
        public const int ASK_TELL = 2;             // need to send CMD_WINS
        public const int WND_SND = 32;             // default send window
        public const int WND_RCV = 128;            // default receive window. must be >= max fragment size
        public const int MTU_DEF = 1200;           // default MTU (reduced to 1200 to fit all cases: https://en.wikipedia.org/wiki/Maximum_transmission_unit ; steam uses 1200 too!)
        public const int ACK_FAST = 3;
        public const int INTERVAL = 100;
        public const int OVERHEAD = 24;
        public const int FRG_MAX = byte.MaxValue;  // kcp encodes 'frg' as byte. so we can only ever send up to 255 fragments.
        public const int DEADLINK = 20;            // default maximum amount of 'xmit' retransmissions until a segment is considered lost
        public const int THRESH_INIT = 2;
        public const int THRESH_MIN = 2;
        public const int PROBE_INIT = 7000;        // 7 secs to probe window size
        public const int PROBE_LIMIT = 120000;     // up to 120 secs to probe window
        public const int FASTACK_LIMIT = 5;        // max times to trigger fastack
        public const int RESERVED_BYTE = 5; // 包头预留字节数 供et网络层使用

        // kcp members.
        internal int state;
        readonly uint conv;          // conversation
        internal uint mtu;
        internal uint mss;           // maximum segment size := MTU - OVERHEAD
        internal uint snd_una;       // unacknowledged. e.g. snd_una is 9 it means 8 has been confirmed, 9 and 10 have been sent
        internal uint snd_nxt;       // forever growing send counter for sequence numbers
        internal uint rcv_nxt;       // forever growing receive counter for sequence numbers
        internal uint ssthresh;      // slow start threshold
        internal int rx_rttval;      // average deviation of rtt, used to measure the jitter of rtt
        internal int rx_srtt;        // smoothed round trip time (a weighted average of rtt)
        internal int rx_rto;
        internal int rx_minrto;
        internal uint snd_wnd;       // send window
        internal uint rcv_wnd;       // receive window
        internal uint rmt_wnd;       // remote window
        internal uint cwnd;          // congestion window
        internal uint probe;
        internal uint interval;
        internal uint ts_flush;      // last flush timestamp in milliseconds
        internal uint xmit;
        internal uint nodelay;       // not a bool. original Kcp has '<2 else' check.
        internal bool updated;
        internal uint ts_probe;      // probe timestamp
        internal uint probe_wait;
        internal uint dead_link;     // maximum amount of 'xmit' retransmissions until a segment is considered lost
        internal uint incr;
        internal uint current;       // current time (milliseconds). set by Update.

        internal int fastresend;
        internal int fastlimit;
        internal bool nocwnd;        // congestion control, negated. heavily restricts send/recv window sizes.
        internal readonly Queue<SegmentStruct> snd_queue = new Queue<SegmentStruct>(16); // send queue
        internal readonly Queue<SegmentStruct> rcv_queue = new Queue<SegmentStruct>(16); // receive queue
        // snd_buffer needs index removals.
        // C# LinkedList allocates for each entry, so let's keep List for now.
        internal readonly List<SegmentStruct> snd_buf = new List<SegmentStruct>(16);   // send buffer
        // rcv_buffer needs index insertions and backwards iteration.
        // C# LinkedList allocates for each entry, so let's keep List for now.
        internal readonly List<SegmentStruct> rcv_buf = new List<SegmentStruct>(16);   // receive buffer
        internal readonly List<AckItem> acklist = new List<AckItem>(16);

        private ArrayPool<byte> kcpSegmentArrayPool;

        // memory buffer
        // size depends on MTU.
        // MTU can be changed at runtime, which resizes the buffer.
        internal byte[] buffer;

        // output function of type <buffer, size>
        readonly Action<byte[], int> output;

        // segment pool to avoid allocations in C#.
        // this is not part of the original C code.
        // readonly Pool<Segment> SegmentPool = new Pool<Segment>(
        //     // create new segment
        //     () => new Segment(),
        //     // reset segment before reuse
        //     (segment) => segment.Reset(),
        //     // initial capacity
        //     32
        // );

        // ikcp_create
        // create a new kcp control object, 'conv' must equal in two endpoint
        // from the same connection.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Kcp(uint conv, Action<byte[], int> output)
        {
            this.conv   = conv;
            this.output = output;
            snd_wnd = WND_SND;
            rcv_wnd = WND_RCV;
            rmt_wnd = WND_RCV;
            mtu = MTU_DEF;
            mss = mtu - OVERHEAD;
            rx_rto    = RTO_DEF;
            rx_minrto = RTO_MIN;
            interval  = INTERVAL;
            ts_flush  = INTERVAL;
            ssthresh  = THRESH_INIT;
            fastlimit = FASTACK_LIMIT;
            dead_link = DEADLINK;
            buffer = new byte[(mtu + OVERHEAD) * 3 + RESERVED_BYTE];
        }

        // ikcp_segment_new
        // we keep the original function and add our pooling to it.
        // this way we'll never miss it anywhere.
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Segment SegmentNew() => SegmentPool.Take();

        // ikcp_segment_delete
        // we keep the original function and add our pooling to it.
        // this way we'll never miss it anywhere.
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // void SegmentDelete(Segment seg) => SegmentPool.Return(seg);

        // calculate how many packets are waiting to be sent
        public int WaitSnd => snd_buf.Count + snd_queue.Count;

        // ikcp_wnd_unused
        // returns the remaining space in receive window (rcv_wnd - rcv_queue)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort WndUnused()
        {
            if (rcv_queue.Count < rcv_wnd)
                return (ushort)(rcv_wnd - (uint)rcv_queue.Count);
            return 0;
        }
        
        public int Receive(Span<byte> data)
        {
            // kcp's ispeek feature is not supported.
            // this makes 'merge fragment' code significantly easier because
            // we can iterate while queue.Count > 0 and dequeue each time.
            // if we had to consider ispeek then count would always be > 0 and
            // we would have to remove only after the loop.
            //
            int len = data.Length;

            if (rcv_queue.Count == 0)
                return -1;

            int peeksize = PeekSize();

            if (peeksize < 0)
                return -2;

            if (peeksize > len)
                return -3;

            bool recover = rcv_queue.Count >= rcv_wnd;

            // merge fragment.
            
            len = 0;
            ref byte dest = ref MemoryMarshal.GetReference(data);
            
            // original KCP iterates rcv_queue and deletes if !ispeek.
            // removing from a c# queue while iterating is not possible, but
            // we can change to 'while Count > 0' and remove every time.
            // (we can remove every time because we removed ispeek support!)
            
            while (rcv_queue.Count > 0)
            {
                // unlike original kcp, we dequeue instead of just getting the
                // entry. this is fine because we remove it in ANY case.
                SegmentStruct seg = rcv_queue.Dequeue();

                // copy segment data into our buffer
                
                ref byte source = ref MemoryMarshal.GetReference(seg.WrittenBuffer);
                Unsafe.CopyBlockUnaligned(ref dest,ref source,(uint)seg.WrittenCount);
                dest = ref Unsafe.Add(ref dest, seg.WrittenCount);

                len += seg.WrittenCount;
                uint fragment = seg.SegHead.frg;

                // note: ispeek is not supported in order to simplify this loop

                // unlike original kcp, we don't need to remove seg from queue
                // because we already dequeued it.
                // simply delete it
                seg.Dispose();
                if (fragment == 0)
                    break;
            }
            

            // move available data from rcv_buf -> rcv_queue
            int removed = 0;
#if NET7_0_OR_GREATER
            foreach (ref SegmentStruct seg in CollectionsMarshal.AsSpan(this.rcv_buf))
#else
            foreach (SegmentStruct seg in rcv_buf)
#endif
            {
                if (seg.SegHead.sn == rcv_nxt && rcv_queue.Count < rcv_wnd)
                {
                    // can't remove while iterating. remember how many to remove
                    // and do it after the loop.
                    // note: don't return segment. we only add it to rcv_queue
                    ++removed;
                    // add
                    rcv_queue.Enqueue(seg);
                    // increase sequence number for next segment
                    rcv_nxt++;
                }
                else
                {
                    break;
                }
            }
            rcv_buf.RemoveRange(0, removed);

            // fast recover
            if (rcv_queue.Count < rcv_wnd && recover)
            {
                // ready to send back CMD_WINS in flush
                // tell remote my window size
                probe |= ASK_TELL;
            }

            return len;
        }

        // ikcp_peeksize
        // check the size of next message in the recv queue.
        // returns -1 if there is no message, or if the message is still incomplete.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PeekSize()
        {
            int length = 0;

            // empty queue?
            if (rcv_queue.Count == 0) return -1;

            // peek the first segment
            SegmentStruct seq = rcv_queue.Peek();

            // seg.frg is 0 if the message requires no fragmentation.
            // in that case, the segment's size is the final message size.
            if (seq.SegHead.frg == 0) return seq.WrittenCount;

            // check if all fragment parts were received yet.
            // seg.frg is the n-th fragment, but in reverse.
            // this way the first received segment tells us how many fragments there are for the message.
            // for example, if a message contains 3 segments:
            //   first segment:  .frg is 2 (index in reverse)
            //   second segment: .frg is 1 (index in reverse)
            //   third segment:  .frg is 0 (index in reverse)
            if (rcv_queue.Count < seq.SegHead.frg + 1) return -1;

            // recv_queue contains all the fragments necessary to reconstruct the message.
            // sum all fragment's sizes to get the full message size.
            foreach (SegmentStruct seg in rcv_queue)
            {
                length += seg.WrittenCount;
                if (seg.SegHead.frg == 0) break;
            }

            return length;
        }

        // ikcp_send
        // splits message into MTU sized fragments, adds them to snd_queue.
        public int Send(ReadOnlySpan<byte> data)
        {
            // fragment count
            int count;
            int len = data.Length;
            int offset = 0;

            // streaming mode: removed. we never want to send 'hello' and
            // receive 'he' 'll' 'o'. we want to always receive 'hello'.

            // calculate amount of fragments necessary for 'len'
            if (len <= mss) count = 1;
            else count = (int)((len + mss - 1) / mss);

            // IMPORTANT kcp encodes 'frg' as 1 byte.
            // so we can only support up to 255 fragments.
            // (which limits max message size to around 288 KB)
            // this is difficult to debug. let's make this 100% obvious.
            if (count > FRG_MAX)
                ThrowFrgCountException(len,count);

            // original kcp uses WND_RCV const instead of rcv_wnd runtime:
            // https://github.com/skywind3000/kcp/pull/291/files
            // which always limits max message size to 144 KB:
            //if (count >= WND_RCV) return -2;
            // using configured rcv_wnd uncorks max message size to 'any':
            if (count >= rcv_wnd) return -2;

            if (count == 0) count = 1;

            ref byte dataRef = ref MemoryMarshal.GetReference(data);

            // fragment
            for (int i = 0; i < count; i++)
            {
                int size = len > (int)mss ? (int)mss : len;
                SegmentStruct seg = new SegmentStruct(size,this.kcpSegmentArrayPool);
                
                if (len > 0)
                {
                    Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(seg.FreeBuffer),ref dataRef,(uint)size);
                    dataRef = ref Unsafe.Add(ref dataRef, size);
                    seg.Advance(size);
                }
                
                seg.SegHead.frg = (byte)(count - i - 1);
                snd_queue.Enqueue(seg);
                offset += size;
                len -= size;
            }

            return 0;
        }

        // ikcp_update_ack
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateAck(int rtt) // round trip time
        {
            // https://tools.ietf.org/html/rfc6298
            if (rx_srtt == 0)
            {
                rx_srtt = rtt;
                rx_rttval = rtt / 2;
            }
            else
            {
                int delta = rtt - rx_srtt;
                if (delta < 0) delta = -delta;
                rx_rttval = (3 * rx_rttval + delta) / 4;
                rx_srtt   = (7 * rx_srtt + rtt) / 8;
                if (rx_srtt < 1) rx_srtt = 1;
            }
            int rto = rx_srtt + Math.Max((int)interval, 4 * rx_rttval);
            rx_rto = Utils.Clamp(rto, rx_minrto, RTO_MAX);
        }

        // ikcp_shrink_buf
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ShrinkBuf()
        {
            if (snd_buf.Count > 0)
            {
                SegmentStruct seg = snd_buf[0];
                snd_una = seg.SegHead.sn;
            }
            else
            {
                snd_una = snd_nxt;
            }
        }

        // ikcp_parse_ack
        // removes the segment with 'sn' from send buffer
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ParseAck(uint sn)
        {
            if (Utils.TimeDiff(sn, snd_una) < 0 || Utils.TimeDiff(sn, snd_nxt) >= 0)
                return;
            // for-int so we can erase while iterating
            bool needRemove = false;
            int removeIndex = 0;
#if NET7_0_OR_GREATER
            foreach (ref var seg in CollectionsMarshal.AsSpan(snd_buf))
#else
            foreach (var seg in snd_buf)
#endif
            {
                // is this the segment?
                if (sn == seg.SegHead.sn)
                {
                    // remove and return
                    needRemove = true;
                    // SegmentDelete(seg);
                    seg.Dispose();
                    break;
                }
                if (Utils.TimeDiff(sn, seg.SegHead.sn) < 0)
                {
                    break;
                }

                removeIndex++;
            }

            if (needRemove)
            {
                snd_buf.RemoveAt(removeIndex);
            }
        }

        // ikcp_parse_una
        // removes all unacknowledged segments with sequence numbers < una from send buffer
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ParseUna(uint una)
        {
            int removed = 0;
#if NET7_0_OR_GREATER
            foreach (ref SegmentStruct seg in CollectionsMarshal.AsSpan(snd_buf))
#else
            foreach (SegmentStruct seg in snd_buf)
#endif
            {
                // if (Utils.TimeDiff(una, seg.sn) > 0)
                if (seg.SegHead.sn < una)
                {
                    // can't remove while iterating. remember how many to remove
                    // and do it after the loop.
                    ++removed;
                    // SegmentDelete(seg);
                    seg.Dispose();
                }
                else
                {
                    break;
                }
            }
            snd_buf.RemoveRange(0, removed);
        }

        // ikcp_parse_fastack
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ParseFastack(uint sn, uint ts) // serial number, timestamp
        {
            // sn needs to be between snd_una and snd_nxt
            // if !(snd_una <= sn && sn < snd_nxt) return;

            // if (Utils.TimeDiff(sn, snd_una) < 0)
            if (sn < snd_una)
                return;

            // if (Utils.TimeDiff(sn, snd_nxt) >= 0)
            if (sn >= snd_nxt)
                return;
#if NET7_0_OR_GREATER
            foreach (ref var seg in CollectionsMarshal.AsSpan(snd_buf))
            {
                // if (Utils.TimeDiff(sn, seg.sn) < 0)
                if (sn < seg.SegHead.sn)
                {
                    break;
                }
                else if (sn != seg.SegHead.sn)
                {
#if !FASTACK_CONSERVE
                    seg.fastack++;
#else
                    if (Utils.TimeDiff(ts, seg.SegHead.ts) >= 0)
                    {
                        seg.fastack++;
                    }
#endif
                }
            }
#else
            for (int i = 0; i < this.snd_buf.Count; i++)
            {
                SegmentStruct seg = this.snd_buf[i];
                // if (Utils.TimeDiff(sn, seg.sn) < 0)
                if (sn < seg.SegHead.sn)
                {
                    break;
                }
                else if (sn != seg.SegHead.sn)
                {
    #if !FASTACK_CONSERVE
                    seg.fastack++;
                    this.snd_buf[i] = seg;
    #else
                    if (Utils.TimeDiff(ts, seg.SegHead.ts) >= 0)
                    {
                        seg.fastack++;
                        this.snd_buf[i] = seg;
                    }
    #endif
                }
            }
#endif
            


        }

        // ikcp_ack_push
        // appends an ack.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AckPush(uint sn, uint ts) // serial number, timestamp
        {
            acklist.Add(new AckItem{ serialNumber = sn, timestamp = ts });
        }

        // ikcp_parse_data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ParseData(ref SegmentStruct newseg)
        {
            uint sn = newseg.SegHead.sn;

            if (Utils.TimeDiff(sn, rcv_nxt + rcv_wnd) >= 0 ||
                Utils.TimeDiff(sn, rcv_nxt) < 0)
            {
                newseg.Dispose();
                return;
            }

            InsertSegmentInReceiveBuffer(ref newseg);
            MoveReceiveBufferReadySegmentsToQueue();
        }

        // inserts the segment into rcv_buf, ordered by seg.sn.
        // drops the segment if one with the same seg.sn already exists.
        // goes through receive buffer in reverse order for performance.
        //
        // note: see KcpTests.InsertSegmentInReceiveBuffer test!
        // note: 'insert or delete' can be done in different ways, but let's
        //       keep consistency with original C kcp.
        internal void InsertSegmentInReceiveBuffer(ref SegmentStruct newseg)
        {
            bool repeat = false; // 'duplicate'

            // original C iterates backwards, so we need to do that as well.
            // note if rcv_buf.Count == 0, i becomes -1 and no looping happens.
            int i;
#if NET7_0_OR_GREATER
            Span<SegmentStruct> arr = CollectionsMarshal.AsSpan(rcv_buf);
            for (i = arr.Length-1; i>=0 ; i--)
            {
                ref SegmentStruct seg =ref arr[i];
#else
            for (i = rcv_buf.Count - 1; i >= 0; i--)
            {
                SegmentStruct seg = rcv_buf[i];
#endif
                if (seg.SegHead.sn == newseg.SegHead.sn)
                {
                    // duplicate segment found. nothing will be added.
                    repeat = true;
                    break;
                }
                if (Utils.TimeDiff(newseg.SegHead.sn, seg.SegHead.sn) > 0)
                {
                    // this entry's sn is < newseg.sn, so let's stop
                    break;
                }
            }
            
            // no duplicate? then insert.
            if (!repeat)
            {
                rcv_buf.Insert(i + 1, newseg);
            }
            // duplicate. just delete it.
            else
            {
                newseg.Dispose();
            }
        }

        // move ready segments from rcv_buf -> rcv_queue.
        // moves only the ready segments which are in rcv_nxt sequence order.
        // some may still be missing an inserted later.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MoveReceiveBufferReadySegmentsToQueue()
        {
            int removed = 0;
#if NET7_0_OR_GREATER
            foreach (ref var seg in CollectionsMarshal.AsSpan(rcv_buf))
#else
            foreach (var seg in rcv_buf)
#endif
            {
                // move segments while they are in 'rcv_nxt' sequence order.
                // some may still be missing and inserted later, in this case it stops immediately
                // because segments always need to be received in the exact sequence order.
                if (seg.SegHead.sn == rcv_nxt && rcv_queue.Count < rcv_wnd)
                {
                    // can't remove while iterating. remember how many to remove
                    // and do it after the loop.
                    ++removed;
                    rcv_queue.Enqueue(seg);
                    // increase sequence number for next segment
                    rcv_nxt++;
                }
                else
                {
                    break;
                }
            }
            rcv_buf.RemoveRange(0, removed);
        }

        // ikcp_input
        // used when you receive a low level packet (e.g. UDP packet)
        // => original kcp uses offset=0, we made it a parameter so that high
        //    level can skip the channel byte more easily
        public int Input(Span<byte> data)
        {
            int offset = 0;
            int size = data.Length;
            uint prev_una = snd_una;
            uint maxack = 0;
            uint latest_ts = 0;
            int flag = 0;

            if (data == null || size < OVERHEAD) return -1;

            while (true)
            {
                // enough data left to decode segment (aka OVERHEAD bytes)?
                if (size < OVERHEAD) break;

                var segHead = Unsafe.ReadUnaligned<SegmentHead>(ref MemoryMarshal.GetReference(data.Slice(offset)));
                offset += Unsafe.SizeOf<SegmentHead>();
                uint conv_ = segHead.conv;
                byte cmd = segHead.cmd;
                byte frg = segHead.frg;
                ushort wnd = segHead.wnd;
                uint ts = segHead.ts;
                uint sn = segHead.sn;
                uint una = segHead.una;
                uint len = segHead.len;

                // reduce remaining size by what was read
                size -= OVERHEAD;

                // enough remaining to read 'len' bytes of the actual payload?
                // note: original kcp casts uint len to int for <0 check.
                if (size < len || (int)len < 0) return -2;

                // validate command type
                if (cmd != CMD_PUSH && cmd != CMD_ACK &&
                    cmd != CMD_WASK && cmd != CMD_WINS)
                    return -3;

                rmt_wnd = wnd;
                ParseUna(una);
                ShrinkBuf();

                if (cmd == CMD_ACK)
                {
                    if (Utils.TimeDiff(current, ts) >= 0)
                    {
                        UpdateAck(Utils.TimeDiff(current, ts));
                    }
                    ParseAck(sn);
                    ShrinkBuf();
                    if (flag == 0)
                    {
                        flag = 1;
                        maxack = sn;
                        latest_ts = ts;
                    }
                    else
                    {
                        if (Utils.TimeDiff(sn, maxack) > 0)
                        {
#if !FASTACK_CONSERVE
                            maxack = sn;
                            latest_ts = ts;
#else
                            if (Utils.TimeDiff(ts, latest_ts) > 0)
                            {
                                maxack = sn;
                                latest_ts = ts;
                            }
#endif
                        }
                    }
                }
                else if (cmd == CMD_PUSH)
                {
                    if (Utils.TimeDiff(sn, rcv_nxt + rcv_wnd) < 0)
                    {
                        AckPush(sn, ts);
                        if (Utils.TimeDiff(sn, rcv_nxt) >= 0)
                        {
                            SegmentStruct seg = new SegmentStruct((int) len,this.kcpSegmentArrayPool);
                            seg.SegHead = new SegmentHead()
                            {
                                conv = conv_,
                                cmd = cmd,
                                frg = frg,
                                wnd = wnd,
                                ts = ts,
                                sn = sn,
                                una = una,
                            };
                            if (len > 0)
                            {
                                data.Slice(offset,(int)len).CopyTo(seg.FreeBuffer);
                                seg.Advance((int)len);
                            }
                            ParseData(ref seg);
                        }
                    }
                }
                else if (cmd == CMD_WASK)
                {
                    // ready to send back CMD_WINS in flush
                    // tell remote my window size
                    probe |= ASK_TELL;
                }
                else if (cmd == CMD_WINS)
                {
                    // do nothing
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
                ParseFastack(maxack, latest_ts);
            }

            // cwnd update when packet arrived
            if (Utils.TimeDiff(snd_una, prev_una) > 0)
            {
                if (cwnd < rmt_wnd)
                {
                    if (cwnd < ssthresh)
                    {
                        cwnd++;
                        incr += mss;
                    }
                    else
                    {
                        if (incr < mss) incr = mss;
                        incr += (mss * mss) / incr + (mss / 16);
                        if ((cwnd + 1) * mss <= incr)
                        {
                            cwnd = (incr + mss - 1) / ((mss > 0) ? mss : 1);
                        }
                    }
                    if (cwnd > rmt_wnd)
                    {
                        cwnd = rmt_wnd;
                        incr = rmt_wnd * mss;
                    }
                }
            }

            return 0;
        }

        // flush helper function
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MakeSpace(ref int size, int space)
        {
            if (size + space > mtu)
            {
                output(buffer, size);
                size = 0;
            }
        }

        // flush helper function
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void FlushBuffer(int size)
        {
            // flush buffer up to 'offset' (<= MTU)
            if (size > 0)
            {
                output(buffer, size);
            }
        }

        // ikcp_flush
        // flush remain ack segments.
        // flush may output multiple <= MTU messages from MakeSpace / FlushBuffer.
        // the amount of messages depends on the sliding window.
        // configured by send/receive window sizes + congestion control.
        // with congestion control, the window will be extremely small(!).
        public void Flush()
        {
            int size  = 0;     // amount of bytes to flush. 'buffer ptr' in C.
            bool lost = false; // lost segments

            // update needs to be called before flushing
            if (!updated) return;

            // kcp only stack allocates a segment here for performance, leaving
            // its data buffer null because this segment's data buffer is never
            // used. that's fine in C, but in C# our segment is a class so we
            // need to allocate and most importantly, not forget to deallocate
            // it before returning.
            SegmentStruct seg = new SegmentStruct((int)this.mtu,this.kcpSegmentArrayPool);
            seg.SegHead.conv = conv;
            seg.SegHead.cmd = CMD_ACK;
            seg.SegHead.wnd = WndUnused();
            seg.SegHead.una = rcv_nxt;

            // flush acknowledges
#if NET7_0_OR_GREATER
            foreach (ref AckItem ack  in CollectionsMarshal.AsSpan(acklist))
#else
            foreach (AckItem ack  in acklist)
#endif
            {
                MakeSpace(ref size, OVERHEAD);
                // ikcp_ack_get assigns ack[i] to seg.sn, seg.ts
                seg.SegHead.sn = ack.serialNumber;
                seg.SegHead.ts = ack.timestamp;
                seg.Encode(buffer.AsSpan(size + RESERVED_BYTE), ref size);
            }
            acklist.Clear();

            // probe window size (if remote window size equals zero)
            if (rmt_wnd == 0)
            {
                if (probe_wait == 0)
                {
                    probe_wait = PROBE_INIT;
                    ts_probe = current + probe_wait;
                }
                else
                {
                    if (Utils.TimeDiff(current, ts_probe) >= 0)
                    {
                        if (probe_wait < PROBE_INIT)
                            probe_wait = PROBE_INIT;
                        probe_wait += probe_wait / 2;
                        if (probe_wait > PROBE_LIMIT)
                            probe_wait = PROBE_LIMIT;
                        ts_probe = current + probe_wait;
                        probe |= ASK_SEND;
                    }
                }
            }
            else
            {
                ts_probe = 0;
                probe_wait = 0;
            }

            // flush window probing commands
            if ((probe & ASK_SEND) != 0)
            {
                seg.SegHead.cmd = CMD_WASK;
                MakeSpace(ref size, OVERHEAD);
                seg.Encode(buffer.AsSpan(size + RESERVED_BYTE), ref size);
            }

            // flush window probing commands
            if ((probe & ASK_TELL) != 0)
            {
                seg.SegHead.cmd = CMD_WINS;
                MakeSpace(ref size, OVERHEAD);
                seg.Encode(buffer.AsSpan(size + RESERVED_BYTE), ref size);
            }

            probe = 0;

            // calculate the window size which is currently safe to send.
            // it's send window, or remote window, whatever is smaller.
            // for our max
            uint cwnd_ = Math.Min(snd_wnd, rmt_wnd);

            // double negative: if congestion window is enabled:
            // limit window size to cwnd.
            //
            // note this may heavily limit window sizes.
            // for our max message size test with super large windows of 32k,
            // 'congestion window' limits it down from 32.000 to 2.
            if (!nocwnd) cwnd_ = Math.Min(cwnd, cwnd_);

            // move cwnd_ 'window size' messages from snd_queue to snd_buf
            //   'snd_nxt' is what we want to send.
            //   'snd_una' is what hasn't been acked yet.
            //   copy up to 'cwnd_' difference between them (sliding window)
            while (Utils.TimeDiff(snd_nxt, snd_una + cwnd_) < 0)
            {
                if (snd_queue.Count == 0) break;

                SegmentStruct newseg = snd_queue.Dequeue();

                newseg.SegHead.conv = conv;
                newseg.SegHead.cmd = CMD_PUSH;
                newseg.SegHead.wnd = seg.SegHead.wnd;
                newseg.SegHead.ts = current;
                newseg.SegHead.sn = snd_nxt;
                snd_nxt += 1; // increase sequence number for next segment
                newseg.SegHead.una = rcv_nxt;
                newseg.resendts = current;
                newseg.rto = rx_rto;
                newseg.fastack = 0;
                newseg.xmit = 0;
                snd_buf.Add(newseg);
            }

            // calculate resent
            uint resent = fastresend > 0 ? (uint)fastresend : 0xffffffff;
            uint rtomin = nodelay == 0 ? (uint)rx_rto >> 3 : 0;
            
            // flush data segments
            int change = 0;
#if NET7_0_OR_GREATER
            var sndBufArr = CollectionsMarshal.AsSpan(this.snd_buf);
            for (int i = 0; i < sndBufArr.Length; i++)
            {
                ref SegmentStruct segment = ref sndBufArr[i];
#else
            for (int i = 0; i < this.snd_buf.Count; i++)
            {
                SegmentStruct segment = this.snd_buf[i];
#endif
                bool needsend = false;

                // initial transmit
                if (segment.xmit == 0)
                {
                    needsend = true;
                    segment.xmit++;
                    segment.rto = this.rx_rto;
                    segment.resendts = this.current + (uint) segment.rto + rtomin;
                }
                // RTO
                else if (Utils.TimeDiff(this.current, segment.resendts) >= 0)
                {
                    needsend = true;
                    segment.xmit++;
                    this.xmit++;
                    if (this.nodelay == 0)
                    {
                        segment.rto += Math.Max(segment.rto, this.rx_rto);
                    }
                    else
                    {
                        int step = (this.nodelay < 2)? segment.rto : this.rx_rto;
                        segment.rto += step / 2;
                    }

                    segment.resendts = this.current + (uint) segment.rto;
                    lost = true;
                }
                // fast retransmit
                else if (segment.fastack >= resent)
                {
                    if (segment.xmit <= this.fastlimit || this.fastlimit <= 0)
                    {
                        needsend = true;
                        segment.xmit++;
                        segment.fastack = 0;
                        segment.resendts = this.current + (uint) segment.rto;
                        change++;
                    }
                }
                
                if (needsend)
                {
                    segment.SegHead.ts = this.current;
                    segment.SegHead.wnd = seg.SegHead.wnd;
                    segment.SegHead.una = this.rcv_nxt;

                    int need = OVERHEAD + segment.WrittenCount;
                    this.MakeSpace(ref size, need);

                    segment.Encode(this.buffer.AsSpan(size + RESERVED_BYTE),ref size);

                    if (segment.WrittenCount > 0)
                    {
                        segment.WrittenBuffer.CopyTo(this.buffer.AsSpan(size + RESERVED_BYTE));
                        
                        size += segment.WrittenCount;
                    }

                    // dead link happens if a message was resent N times, but an
                    // ack was still not received.
                    if (segment.xmit >= this.dead_link)
                    {
                        this.state = -1;
                    }
                }
#if !NET7_0_OR_GREATER
                this.snd_buf[i] = segment;
#endif
            }

            // kcp stackallocs 'seg'. our C# segment is a class though, so we
            // need to properly delete and return it to the pool now that we are
            // done with it.
            // SegmentDelete(seg);

            seg.Dispose();
            
            // flush remaining segments
            FlushBuffer(size);

            // update ssthresh
            // rate halving, https://tools.ietf.org/html/rfc6937
            if (change > 0)
            {
                uint inflight = snd_nxt - snd_una;
                ssthresh = inflight / 2;
                if (ssthresh < THRESH_MIN)
                    ssthresh = THRESH_MIN;
                cwnd = ssthresh + resent;
                incr = cwnd * mss;
            }

            // congestion control, https://tools.ietf.org/html/rfc5681
            if (lost)
            {
                // original C uses 'cwnd', not kcp->cwnd!
                ssthresh = cwnd_ / 2;
                if (ssthresh < THRESH_MIN)
                    ssthresh = THRESH_MIN;
                cwnd = 1;
                incr = mss;
            }

            if (cwnd < 1)
            {
                cwnd = 1;
                incr = mss;
            }
        }

        // ikcp_update
        // update state (call it repeatedly, every 10ms-100ms), or you can ask
        // Check() when to call it again (without Input/Send calling).
        //
        // 'current' - current timestamp in millisec. pass it to Kcp so that
        // Kcp doesn't have to do any stopwatch/deltaTime/etc. code
        //
        // time as uint, likely to minimize bandwidth.
        // uint.max = 4294967295 ms = 1193 hours = 49 days
        public void Update(uint currentTimeMilliSeconds)
        {
            current = currentTimeMilliSeconds;

            // not updated yet? then set updated and last flush time.
            if (!updated)
            {
                updated = true;
                ts_flush = current;
            }

            // slap is time since last flush in milliseconds
            int slap = Utils.TimeDiff(current, ts_flush);

            // hard limit: if 10s elapsed, always flush no matter what
            if (slap >= 10000 || slap < -10000)
            {
                ts_flush = current;
                slap = 0;
            }

            // last flush is increased by 'interval' each time.
            // so slap >= is a strange way to check if interval has elapsed yet.
            if (slap >= 0)
            {
                // increase last flush time by one interval
                ts_flush += interval;

                // if last flush is still behind, increase it to current + interval
                // if (Utils.TimeDiff(current, ts_flush) >= 0) // original kcp.c
                if (current >= ts_flush)                       // less confusing
                {
                    ts_flush = current + interval;
                }
                Flush();
            }
        }

        // ikcp_check
        // Determine when should you invoke update
        // Returns when you should invoke update in millisec, if there is no
        // input/send calling. you can call update in that time, instead of
        // call update repeatly.
        //
        // Important to reduce unnecessary update invoking. use it to schedule
        // update (e.g. implementing an epoll-like mechanism, or optimize update
        // when handling massive kcp connections).
        public uint Check(uint current_)
        {
            uint ts_flush_ = ts_flush;
            // int tm_flush = 0x7fffffff; original kcp: useless assignment
            int tm_packet = 0x7fffffff;

            if (!updated)
            {
                return current_;
            }

            if (Utils.TimeDiff(current_, ts_flush_) >= 10000 ||
                Utils.TimeDiff(current_, ts_flush_) < -10000)
            {
                ts_flush_ = current_;
            }

            if (Utils.TimeDiff(current_, ts_flush_) >= 0)
            {
                return current_;
            }

            int tm_flush = Utils.TimeDiff(ts_flush_, current_);
#if NET7_0_OR_GREATER
            foreach (ref SegmentStruct seg in CollectionsMarshal.AsSpan(this.snd_buf))
#else
            foreach (SegmentStruct seg in snd_buf)
#endif
            {
                int diff = Utils.TimeDiff(seg.resendts, current_);
                if (diff <= 0)
                {
                    return current_;
                }
                if (diff < tm_packet) tm_packet = diff;
            }

            uint minimal = (uint)(tm_packet < tm_flush ? tm_packet : tm_flush);
            if (minimal >= interval) minimal = interval;

            return current_ + minimal;
        }

        // ikcp_setmtu
        // Change MTU (Maximum Transmission Unit) size.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMtu(uint mtu)
        {
            if (mtu < 50 || mtu < OVERHEAD)
                this.ThrowMTUException();

            buffer = new byte[(mtu + OVERHEAD) * 3 + RESERVED_BYTE];
            this.mtu = mtu;
            mss = mtu - OVERHEAD;
        }

        // ikcp_interval
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInterval(uint interval)
        {
            // clamp interval between 10 and 5000
            if      (interval > 5000) interval = 5000;
            else if (interval < 10)   interval = 10;
            this.interval = interval;
        }

        // ikcp_nodelay
        // configuration: https://github.com/skywind3000/kcp/blob/master/README.en.md#protocol-configuration
        //   nodelay : Whether nodelay mode is enabled, 0 is not enabled; 1 enabled.
        //   interval ：Protocol internal work interval, in milliseconds, such as 10 ms or 20 ms.
        //   resend ：Fast retransmission mode, 0 represents off by default, 2 can be set (2 ACK spans will result in direct retransmission)
        //   nc ：Whether to turn off flow control, 0 represents “Do not turn off” by default, 1 represents “Turn off”.
        // Normal Mode: ikcp_nodelay(kcp, 0, 40, 0, 0);
        // Turbo Mode： ikcp_nodelay(kcp, 1, 10, 2, 1);
        public void SetNoDelay(uint nodelay, uint interval = INTERVAL, int resend = 0, bool nocwnd = false)
        {
            this.nodelay = nodelay;
            if (nodelay != 0)
            {
                rx_minrto = RTO_NDL;
            }
            else
            {
                rx_minrto = RTO_MIN;
            }

            // clamp interval between 10 and 5000
            if (interval > 5000) interval = 5000;
            else if (interval < 10) interval = 10;
            this.interval = interval;

            if (resend >= 0)
            {
                fastresend = resend;
            }

            this.nocwnd = nocwnd;
        }

        // ikcp_wndsize
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWindowSize(uint sendWindow, uint receiveWindow)
        {
            if (sendWindow > 0)
            {
                snd_wnd = sendWindow;
            }

            if (receiveWindow > 0)
            {
                // must >= max fragment size
                rcv_wnd = Math.Max(receiveWindow, WND_RCV);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMinrto(int minrto)
        {
            this.rx_minrto = minrto;
        }

        public void SetArrayPool(ArrayPool<byte> arrayPool)
        {
            this.kcpSegmentArrayPool = arrayPool;
        }

        [DoesNotReturn]
        private void ThrowMTUException()
        {
            throw new ArgumentException("MTU must be higher than 50 and higher than OVERHEAD");
        }

        [DoesNotReturn]
        private void ThrowFrgCountException(int len, int count)
        {
            throw new Exception($"Send len={len} requires {count} fragments, but kcp can only handle up to {FRG_MAX} fragments.");
        }
    }
}
