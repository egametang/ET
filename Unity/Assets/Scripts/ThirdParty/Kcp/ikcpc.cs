using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static ET.IQUEUEHEAD;
using static ET.KCPBASIC;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8981

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace ET
{
    internal static unsafe class IKCP
    {
        private static void memcpy(void* dest, void* src, int n) => Unsafe.CopyBlock(dest, src, (uint)n);

        private static void memcpy(void* dest, void* src, uint n) => Unsafe.CopyBlock(dest, src, n);

        private static void* malloc(nint size) =>
#if !UNITY_2021_3_OR_NEWER || NET6_0_OR_GREATER
            NativeMemory.Alloc((nuint)size);
#else
            (void*)Marshal.AllocHGlobal(size);
#endif

        private static void* malloc(nuint size) =>
#if !UNITY_2021_3_OR_NEWER || NET6_0_OR_GREATER
            NativeMemory.Alloc(size);
#else
            (void*)Marshal.AllocHGlobal((nint)size);
#endif

        private static void free(void* ptr) =>
#if !UNITY_2021_3_OR_NEWER || NET6_0_OR_GREATER
            NativeMemory.Free(ptr);
#else
            Marshal.FreeHGlobal((nint)ptr);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte* ikcp_encode8u(byte* p, byte c)
        {
            *p++ = c;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte* ikcp_decode8u(byte* p, byte* c)
        {
            *c = *p++;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte* ikcp_encode16u(byte* p, ushort w)
        {
            memcpy(p, &w, 2);
            p += 2;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte* ikcp_decode16u(byte* p, ushort* w)
        {
            memcpy(w, p, 2);
            p += 2;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte* ikcp_encode32u(byte* p, uint l)
        {
            memcpy(p, &l, 4);
            p += 4;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte* ikcp_decode32u(byte* p, uint* l)
        {
            memcpy(l, p, 4);
            p += 4;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint _imin_(uint a, uint b) => a <= b ? a : b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint _imax_(uint a, uint b) => a >= b ? a : b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint _ibound_(uint lower, uint middle, uint upper) => _imin_(_imax_(lower, middle), upper);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int _iclamp_(int x, uint min, uint max) => x < min ? (int)min : x > max ? (int)max : x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint _iceilpow2_(uint x)
        {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            x++;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int _itimediff(uint later, uint earlier) => (int)(later - earlier);

        private static void* ikcp_malloc(nint size) => malloc(size);

        private static void* ikcp_malloc(nuint size) => malloc(size);

        private static void ikcp_free(void* ptr) => free(ptr);

        private static IKCPSEG* ikcp_segment_new(IKCPCB* kcp, int size) => (IKCPSEG*)ikcp_malloc(sizeof(IKCPSEG) + size);

        private static void ikcp_segment_delete(IKCPCB* kcp, IKCPSEG* seg) => ikcp_free(seg);

        private static void ikcp_output(KcpCallback output, byte[] data, int size)
        {
            if (size == 0)
                return;
            output(data, size);
        }

        public static IKCPCB* ikcp_create(uint conv)
        {
            var kcp = (IKCPCB*)ikcp_malloc(sizeof(IKCPCB));
            kcp->conv = conv;
            kcp->snd_una = 0;
            kcp->snd_nxt = 0;
            kcp->rcv_nxt = 0;
            kcp->ts_probe = 0;
            kcp->probe_wait = 0;
            kcp->snd_wnd = WND_SND;
            kcp->rcv_wnd = WND_RCV;
            kcp->rmt_wnd = WND_RCV;
            kcp->cwnd = 0;
            kcp->incr = 0;
            kcp->probe = 0;
            kcp->mtu = MTU_DEF;
            kcp->mss = kcp->mtu - OVERHEAD;
            kcp->stream = 0;
            iqueue_init(&kcp->snd_queue);
            iqueue_init(&kcp->rcv_queue);
            iqueue_init(&kcp->snd_buf);
            iqueue_init(&kcp->rcv_buf);
            kcp->nrcv_buf = 0;
            kcp->nsnd_buf = 0;
            kcp->nrcv_que = 0;
            kcp->nsnd_que = 0;
            kcp->state = 0;
            kcp->acklist = null;
            kcp->ackblock = 0;
            kcp->ackcount = 0;
            kcp->rx_srtt = 0;
            kcp->rx_rttval = 0;
            kcp->rx_rto = (int)RTO_DEF;
            kcp->rx_minrto = (int)RTO_MIN;
            kcp->current = 0;
            kcp->interval = INTERVAL;
            kcp->ts_flush = INTERVAL;
            kcp->nodelay = 0;
            kcp->updated = 0;
            kcp->ssthresh = THRESH_INIT;
            kcp->fastresend = 0;
            kcp->fastlimit = (int)FASTACK_LIMIT;
            kcp->nocwnd = 0;
            kcp->xmit = 0;
            return kcp;
        }

        public static void ikcp_release(IKCPCB* kcp)
        {
            if (kcp != null)
            {
                IKCPSEG* seg;
                while (!iqueue_is_empty(&kcp->snd_buf))
                {
                    seg = iqueue_entry(kcp->snd_buf.next);
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                }

                while (!iqueue_is_empty(&kcp->rcv_buf))
                {
                    seg = iqueue_entry(kcp->rcv_buf.next);
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                }

                while (!iqueue_is_empty(&kcp->snd_queue))
                {
                    seg = iqueue_entry(kcp->snd_queue.next);
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                }

                while (!iqueue_is_empty(&kcp->rcv_queue))
                {
                    seg = iqueue_entry(kcp->rcv_queue.next);
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                }

                if (kcp->acklist != null)
                    ikcp_free(kcp->acklist);
                kcp->nrcv_buf = 0;
                kcp->nsnd_buf = 0;
                kcp->nrcv_que = 0;
                kcp->nsnd_que = 0;
                kcp->ackcount = 0;
                kcp->acklist = null;
                ikcp_free(kcp);
            }
        }

        public static int ikcp_recv(IKCPCB* kcp, byte* buffer, int len)
        {
            if (iqueue_is_empty(&kcp->rcv_queue))
                return -1;
            var peeksize = ikcp_peeksize_internal(kcp);
            if (peeksize < 0)
                return -2;
            int recover;
            IQUEUEHEAD* p;
            IKCPSEG* seg;
            if (len < 0)
            {
                len = -len;
                if (peeksize > len)
                    return -3;
                recover = kcp->nrcv_que >= kcp->rcv_wnd ? 1 : 0;
                p = kcp->rcv_queue.next;
                for (len = 0; p != &kcp->rcv_queue;)
                {
                    seg = iqueue_entry(p);
                    p = p->next;
                    if (buffer != null)
                    {
                        memcpy(buffer, seg->data, seg->len);
                        buffer += seg->len;
                    }

                    len += (int)seg->len;
                    var fragment = (int)seg->frg;
                    if (fragment == 0)
                        break;
                }
            }
            else
            {
                if (peeksize > len)
                    return -3;
                recover = kcp->nrcv_que >= kcp->rcv_wnd ? 1 : 0;
                p = kcp->rcv_queue.next;
                for (len = 0; p != &kcp->rcv_queue;)
                {
                    seg = iqueue_entry(p);
                    p = p->next;
                    if (buffer != null)
                    {
                        memcpy(buffer, seg->data, seg->len);
                        buffer += seg->len;
                    }

                    len += (int)seg->len;
                    var fragment = (int)seg->frg;
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                    kcp->nrcv_que--;
                    if (fragment == 0)
                        break;
                }
            }

            while (!iqueue_is_empty(&kcp->rcv_buf))
            {
                seg = iqueue_entry(kcp->rcv_buf.next);
                if (seg->sn == kcp->rcv_nxt && kcp->nrcv_que < kcp->rcv_wnd)
                {
                    iqueue_del(&seg->node);
                    kcp->nrcv_buf--;
                    iqueue_add_tail(&seg->node, &kcp->rcv_queue);
                    kcp->nrcv_que++;
                    kcp->rcv_nxt++;
                }
                else
                {
                    break;
                }
            }

            if (kcp->nrcv_que < kcp->rcv_wnd && recover != 0)
                kcp->probe |= ASK_TELL;
            return len;
        }

        public static int ikcp_peeksize(IKCPCB* kcp) => iqueue_is_empty(&kcp->rcv_queue) ? -1 : ikcp_peeksize_internal(kcp);

        private static int ikcp_peeksize_internal(IKCPCB* kcp)
        {
            var seg = iqueue_entry(kcp->rcv_queue.next);
            if (seg->frg == 0)
                return (int)seg->len;
            if (kcp->nrcv_que < seg->frg + 1)
                return -1;
            IQUEUEHEAD* p;
            var length = 0;
            for (p = kcp->rcv_queue.next; p != &kcp->rcv_queue; p = p->next)
            {
                seg = iqueue_entry(p);
                length += (int)seg->len;
                if (seg->frg == 0)
                    break;
            }

            return length;
        }

        public static int ikcp_send(IKCPCB* kcp, byte* buffer, int len)
        {
            if (len < 0)
                return -1;
            IKCPSEG* seg;
            var sent = 0;
            if (kcp->stream != 0)
            {
                if (!iqueue_is_empty(&kcp->snd_queue))
                {
                    var old = iqueue_entry(kcp->snd_queue.prev);
                    if (old->len < kcp->mss)
                    {
                        var capacity = (int)kcp->mss - (int)old->len;
                        var extend = len < capacity ? len : capacity;
                        seg = ikcp_segment_new(kcp, (int)old->len + extend);
                        iqueue_add_tail(&seg->node, &kcp->snd_queue);
                        memcpy(seg->data, old->data, old->len);
                        if (buffer != null)
                        {
                            memcpy(seg->data + old->len, buffer, extend);
                            buffer += extend;
                        }

                        seg->len = old->len + (uint)extend;
                        seg->frg = 0;
                        len -= extend;
                        iqueue_del_init(&old->node);
                        ikcp_segment_delete(kcp, old);
                        sent = extend;
                    }
                }

                if (len <= 0)
                    return sent;
                int count;
                if (len <= (int)kcp->mss)
                {
                    count = 1;
                }
                else
                {
                    count = (int)((len + kcp->mss - 1) / kcp->mss);
                    if (count >= (int)kcp->rcv_wnd)
                        return sent > 0 ? sent : -2;
                    if (count == 0)
                        count = 1;
                }

                int i;
                for (i = 0; i < count; ++i)
                {
                    var size = len > (int)kcp->mss ? (int)kcp->mss : len;
                    seg = ikcp_segment_new(kcp, size);
                    if (buffer != null && len > 0)
                        memcpy(seg->data, buffer, size);
                    seg->len = (uint)size;
                    seg->frg = 0;
                    iqueue_init(&seg->node);
                    iqueue_add_tail(&seg->node, &kcp->snd_queue);
                    kcp->nsnd_que++;
                    if (buffer != null)
                        buffer += size;
                    len -= size;
                    sent += size;
                }
            }
            else
            {
                int count;
                if (len <= (int)kcp->mss)
                {
                    count = 1;
                }
                else
                {
                    count = (int)((len + kcp->mss - 1) / kcp->mss);
                    if (count > FRG_LIMIT || count >= (int)kcp->rcv_wnd)
                        return -2;
                    if (count == 0)
                        count = 1;
                }

                int i;
                for (i = 0; i < count; ++i)
                {
                    var size = len > (int)kcp->mss ? (int)kcp->mss : len;
                    seg = ikcp_segment_new(kcp, size);
                    if (buffer != null && len > 0)
                        memcpy(seg->data, buffer, size);
                    seg->len = (uint)size;
                    seg->frg = (uint)(count - i - 1);
                    iqueue_init(&seg->node);
                    iqueue_add_tail(&seg->node, &kcp->snd_queue);
                    kcp->nsnd_que++;
                    if (buffer != null)
                        buffer += size;
                    len -= size;
                    sent += size;
                }
            }

            return sent;
        }

        private static void ikcp_update_ack(IKCPCB* kcp, int rtt)
        {
            if (kcp->rx_srtt == 0)
            {
                kcp->rx_srtt = rtt;
                kcp->rx_rttval = rtt / 2;
            }
            else
            {
                var delta = rtt - kcp->rx_srtt;
                if (delta < 0)
                    delta = -delta;
                kcp->rx_rttval = (3 * kcp->rx_rttval + delta) / 4;
                kcp->rx_srtt = (7 * kcp->rx_srtt + rtt) / 8;
                if (kcp->rx_srtt < 1)
                    kcp->rx_srtt = 1;
            }

            var rto = (int)(kcp->rx_srtt + _imax_(kcp->interval, (uint)(4 * kcp->rx_rttval)));
            kcp->rx_rto = (int)_ibound_((uint)kcp->rx_minrto, (uint)rto, RTO_MAX);
        }

        private static void ikcp_shrink_buf(IKCPCB* kcp)
        {
            var p = kcp->snd_buf.next;
            if (p != &kcp->snd_buf)
            {
                var seg = iqueue_entry(p);
                kcp->snd_una = seg->sn;
            }
            else
            {
                kcp->snd_una = kcp->snd_nxt;
            }
        }

        private static void ikcp_parse_ack(IKCPCB* kcp, uint sn)
        {
            if (_itimediff(sn, kcp->snd_una) < 0 || _itimediff(sn, kcp->snd_nxt) >= 0)
                return;
            IQUEUEHEAD* p, next;
            for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = next)
            {
                var seg = iqueue_entry(p);
                next = p->next;
                if (sn == seg->sn)
                {
                    iqueue_del(p);
                    ikcp_segment_delete(kcp, seg);
                    kcp->nsnd_buf--;
                    break;
                }

                if (_itimediff(sn, seg->sn) < 0)
                    break;
            }
        }

        private static void ikcp_parse_una(IKCPCB* kcp, uint una)
        {
            IQUEUEHEAD* p, next;
            for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = next)
            {
                var seg = iqueue_entry(p);
                next = p->next;
                if (_itimediff(una, seg->sn) > 0)
                {
                    iqueue_del(p);
                    ikcp_segment_delete(kcp, seg);
                    kcp->nsnd_buf--;
                }
                else
                {
                    break;
                }
            }
        }

        private static void ikcp_parse_fastack(IKCPCB* kcp, uint sn, uint ts)
        {
            if (_itimediff(sn, kcp->snd_una) < 0 || _itimediff(sn, kcp->snd_nxt) >= 0)
                return;
            IQUEUEHEAD* p, next;
            for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = next)
            {
                var seg = iqueue_entry(p);
                next = p->next;
                if (_itimediff(sn, seg->sn) < 0)
                    break;
                if (sn != seg->sn)
                {
#if KCP_FASTACK_CONSERVE
                    seg->fastack++;
#else
                    if (_itimediff(ts, seg->ts) >= 0)
                        seg->fastack++;
#endif
                }
            }
        }

        private static int ikcp_ack_push(IKCPCB* kcp, uint sn, uint ts)
        {
            var newsize = kcp->ackcount + 1;
            if (newsize > kcp->ackblock)
            {
                var newblock = newsize <= 8 ? 8 : _iceilpow2_(newsize);
                var acklist = (uint*)ikcp_malloc(newblock << 3);
                if (kcp->acklist != null)
                {
                    uint x;
                    for (x = 0; x < kcp->ackcount; ++x)
                    {
                        acklist[x * 2] = kcp->acklist[x * 2];
                        acklist[x * 2 + 1] = kcp->acklist[x * 2 + 1];
                    }

                    ikcp_free(kcp->acklist);
                }

                kcp->acklist = acklist;
                kcp->ackblock = newblock;
            }

            var ptr = &kcp->acklist[kcp->ackcount * 2];
            ptr[0] = sn;
            ptr[1] = ts;
            kcp->ackcount++;
            return 0;
        }

        private static void ikcp_ack_get(IKCPCB* kcp, int p, uint* sn, uint* ts)
        {
            if (sn != null)
                sn[0] = kcp->acklist[p * 2];
            if (ts != null)
                ts[0] = kcp->acklist[p * 2 + 1];
        }

        private static void ikcp_parse_data(IKCPCB* kcp, IKCPSEG* newseg)
        {
            var sn = newseg->sn;
            if (_itimediff(sn, kcp->rcv_nxt + kcp->rcv_wnd) >= 0 || _itimediff(sn, kcp->rcv_nxt) < 0)
            {
                ikcp_segment_delete(kcp, newseg);
                return;
            }

            IQUEUEHEAD* p, prev;
            var repeat = 0;
            for (p = kcp->rcv_buf.prev; p != &kcp->rcv_buf; p = prev)
            {
                var seg = iqueue_entry(p);
                prev = p->prev;
                if (seg->sn == sn)
                {
                    repeat = 1;
                    break;
                }

                if (_itimediff(sn, seg->sn) > 0)
                    break;
            }

            if (repeat == 0)
            {
                iqueue_init(&newseg->node);
                iqueue_add(&newseg->node, p);
                kcp->nrcv_buf++;
            }
            else
            {
                ikcp_segment_delete(kcp, newseg);
            }

            while (!iqueue_is_empty(&kcp->rcv_buf))
            {
                var seg = iqueue_entry(kcp->rcv_buf.next);
                if (seg->sn == kcp->rcv_nxt && kcp->nrcv_que < kcp->rcv_wnd)
                {
                    iqueue_del(&seg->node);
                    kcp->nrcv_buf--;
                    iqueue_add_tail(&seg->node, &kcp->rcv_queue);
                    kcp->nrcv_que++;
                    kcp->rcv_nxt++;
                }
                else
                {
                    break;
                }
            }
        }

        public static int ikcp_input(IKCPCB* kcp, byte* data, int size)
        {
            if (data == null || size < (int)OVERHEAD)
                return -1;
            var prev_una = kcp->snd_una;
            uint maxack = 0, latest_ts = 0;
            var flag = 0;
            while (true)
            {
                uint ts, sn, len, una, conv;
                ushort wnd;
                byte cmd, frg;
                if (size < (int)OVERHEAD)
                    break;
                data = ikcp_decode32u(data, &conv);
                // TODO: disable conv
                // if (conv != kcp->conv)
                //     return -1;
                data = ikcp_decode8u(data, &cmd);
                data = ikcp_decode8u(data, &frg);
                data = ikcp_decode16u(data, &wnd);
                data = ikcp_decode32u(data, &ts);
                data = ikcp_decode32u(data, &sn);
                data = ikcp_decode32u(data, &una);
                data = ikcp_decode32u(data, &len);
                size -= (int)OVERHEAD;
                if (size < len || (int)len < 0)
                    return -2;
                if (cmd != CMD_PUSH && cmd != CMD_ACK && cmd != CMD_WASK && cmd != CMD_WINS)
                    return -3;
                kcp->rmt_wnd = wnd;
                ikcp_parse_una(kcp, una);
                ikcp_shrink_buf(kcp);
                if (cmd == CMD_ACK)
                {
                    if (_itimediff(kcp->current, ts) >= 0)
                        ikcp_update_ack(kcp, _itimediff(kcp->current, ts));
                    ikcp_parse_ack(kcp, sn);
                    ikcp_shrink_buf(kcp);
                    if (flag == 0)
                    {
                        flag = 1;
                        maxack = sn;
                        latest_ts = ts;
                    }
                    else
                    {
                        if (_itimediff(sn, maxack) > 0)
                        {
#if KCP_FASTACK_CONSERVE
                            maxack = sn;
                            latest_ts = ts;
#else
                            if (_itimediff(ts, latest_ts) > 0)
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
                    if (_itimediff(sn, kcp->rcv_nxt + kcp->rcv_wnd) < 0)
                    {
                        if (ikcp_ack_push(kcp, sn, ts) != 0)
                            return -4;
                        if (_itimediff(sn, kcp->rcv_nxt) >= 0)
                        {
                            var seg = ikcp_segment_new(kcp, (int)len);
                            seg->conv = conv;
                            seg->cmd = cmd;
                            seg->frg = frg;
                            seg->wnd = wnd;
                            seg->ts = ts;
                            seg->sn = sn;
                            seg->una = una;
                            seg->len = len;
                            if (len > 0)
                                memcpy(seg->data, data, len);
                            ikcp_parse_data(kcp, seg);
                        }
                    }
                }
                else if (cmd == CMD_WASK)
                {
                    kcp->probe |= ASK_TELL;
                }
                else if (cmd != CMD_WINS)
                {
                    return -3;
                }

                data += len;
                size -= (int)len;
            }

            if (flag != 0)
                ikcp_parse_fastack(kcp, maxack, latest_ts);
            if (_itimediff(kcp->snd_una, prev_una) > 0)
            {
                if (kcp->cwnd < kcp->rmt_wnd)
                {
                    var mss = kcp->mss;
                    if (kcp->cwnd < kcp->ssthresh)
                    {
                        kcp->cwnd++;
                        kcp->incr += mss;
                    }
                    else
                    {
                        if (kcp->incr < mss)
                            kcp->incr = mss;
                        kcp->incr += mss * mss / kcp->incr + mss / 16;
                        if ((kcp->cwnd + 1) * mss <= kcp->incr)
                            kcp->cwnd = (kcp->incr + mss - 1) / (mss > 0 ? mss : 1);
                    }

                    if (kcp->cwnd > kcp->rmt_wnd)
                    {
                        kcp->cwnd = kcp->rmt_wnd;
                        kcp->incr = kcp->rmt_wnd * mss;
                    }
                }
            }

            return 0;
        }

        private static byte* ikcp_encode_seg(byte* ptr, IKCPSEG* seg)
        {
            ptr = ikcp_encode32u(ptr, seg->conv);
            ptr = ikcp_encode8u(ptr, (byte)seg->cmd);
            ptr = ikcp_encode8u(ptr, (byte)seg->frg);
            ptr = ikcp_encode16u(ptr, (ushort)seg->wnd);
            ptr = ikcp_encode32u(ptr, seg->ts);
            ptr = ikcp_encode32u(ptr, seg->sn);
            ptr = ikcp_encode32u(ptr, seg->una);
            ptr = ikcp_encode32u(ptr, seg->len);
            return ptr;
        }

        private static int ikcp_wnd_unused(IKCPCB* kcp) => kcp->nrcv_que < kcp->rcv_wnd ? (int)(kcp->rcv_wnd - kcp->nrcv_que) : 0;

        public static void ikcp_flush(IKCPCB* kcp, KcpCallback output, byte[] bytes)
        {
            if (kcp->updated == 0)
                return;
            ikcp_flush_internal(kcp, output, bytes);
        }

        private static void ikcp_flush_internal(IKCPCB* kcp, KcpCallback output, byte[] bytes)
        {
            var current = kcp->current;
            fixed (byte* buffer = &bytes[REVERSED_HEAD])
            {
                var ptr = buffer;
                int size, i;
                IQUEUEHEAD* p;
                var change = 0;
                var lost = 0;
                IKCPSEG seg;
                seg.conv = kcp->conv;
                seg.cmd = CMD_ACK;
                seg.frg = 0;
                seg.wnd = (uint)ikcp_wnd_unused(kcp);
                seg.una = kcp->rcv_nxt;
                seg.len = 0;
                seg.sn = 0;
                seg.ts = 0;
                var count = (int)kcp->ackcount;
                for (i = 0; i < count; ++i)
                {
                    size = (int)(ptr - buffer);
                    if (size + (int)OVERHEAD > (int)kcp->mtu)
                    {
                        ikcp_output(output, bytes, size);
                        ptr = buffer;
                    }

                    ikcp_ack_get(kcp, i, &seg.sn, &seg.ts);
                    ptr = ikcp_encode_seg(ptr, &seg);
                }

                kcp->ackcount = 0;
                if (kcp->rmt_wnd == 0)
                {
                    if (kcp->probe_wait == 0)
                    {
                        kcp->probe_wait = PROBE_INIT;
                        kcp->ts_probe = kcp->current + kcp->probe_wait;
                    }
                    else
                    {
                        if (_itimediff(kcp->current, kcp->ts_probe) >= 0)
                        {
                            if (kcp->probe_wait < PROBE_INIT)
                                kcp->probe_wait = PROBE_INIT;
                            kcp->probe_wait += kcp->probe_wait / 2;
                            if (kcp->probe_wait > PROBE_LIMIT)
                                kcp->probe_wait = PROBE_LIMIT;
                            kcp->ts_probe = kcp->current + kcp->probe_wait;
                            kcp->probe |= ASK_SEND;
                        }
                    }
                }
                else
                {
                    kcp->ts_probe = 0;
                    kcp->probe_wait = 0;
                }

                if ((kcp->probe != 0) & (ASK_SEND != 0))
                {
                    seg.cmd = CMD_WASK;
                    size = (int)(ptr - buffer);
                    if (size + (int)OVERHEAD > (int)kcp->mtu)
                    {
                        ikcp_output(output, bytes, size);
                        ptr = buffer;
                    }

                    ptr = ikcp_encode_seg(ptr, &seg);
                }

                if ((kcp->probe != 0) & (ASK_TELL != 0))
                {
                    seg.cmd = CMD_WINS;
                    size = (int)(ptr - buffer);
                    if (size + (int)OVERHEAD > (int)kcp->mtu)
                    {
                        ikcp_output(output, bytes, size);
                        ptr = buffer;
                    }

                    ptr = ikcp_encode_seg(ptr, &seg);
                }

                kcp->probe = 0;
                var cwnd = _imin_(kcp->snd_wnd, kcp->rmt_wnd);
                if (kcp->nocwnd == 0)
                    cwnd = _imin_(kcp->cwnd, cwnd);
                while (_itimediff(kcp->snd_nxt, kcp->snd_una + cwnd) < 0)
                {
                    if (iqueue_is_empty(&kcp->snd_queue))
                        break;
                    var newseg = iqueue_entry(kcp->snd_queue.next);
                    iqueue_del(&newseg->node);
                    iqueue_add_tail(&newseg->node, &kcp->snd_buf);
                    kcp->nsnd_que--;
                    kcp->nsnd_buf++;
                    newseg->conv = kcp->conv;
                    newseg->cmd = CMD_PUSH;
                    newseg->wnd = seg.wnd;
                    newseg->ts = current;
                    newseg->sn = kcp->snd_nxt++;
                    newseg->una = kcp->rcv_nxt;
                    newseg->resendts = current;
                    newseg->rto = (uint)kcp->rx_rto;
                    newseg->fastack = 0;
                    newseg->xmit = 0;
                }

                var resent = kcp->fastresend > 0 ? (uint)kcp->fastresend : 4294967295;
                if (kcp->nodelay == 0)
                {
                    var rtomin = (uint)(kcp->rx_rto >> 3);
                    for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = p->next)
                    {
                        var segment = iqueue_entry(p);
                        var needsend = 0;
                        if (segment->xmit == 0)
                        {
                            needsend = 1;
                            segment->xmit++;
                            segment->rto = (uint)kcp->rx_rto;
                            segment->resendts = current + segment->rto + rtomin;
                        }
                        else if (_itimediff(current, segment->resendts) >= 0)
                        {
                            needsend = 1;
                            segment->xmit++;
                            kcp->xmit++;
                            segment->rto += _imax_(segment->rto, (uint)kcp->rx_rto);
                            segment->resendts = current + segment->rto;
                            lost = 1;
                        }
                        else if (segment->fastack >= resent)
                        {
                            if ((int)segment->xmit <= kcp->fastlimit || kcp->fastlimit == 0)
                            {
                                needsend = 1;
                                segment->xmit++;
                                segment->fastack = 0;
                                segment->resendts = current + segment->rto;
                                change++;
                            }
                        }

                        if (needsend != 0)
                        {
                            segment->ts = current;
                            segment->wnd = seg.wnd;
                            segment->una = kcp->rcv_nxt;
                            size = (int)(ptr - buffer);
                            var need = (int)(OVERHEAD + segment->len);
                            if (size + need > (int)kcp->mtu)
                            {
                                ikcp_output(output, bytes, size);
                                ptr = buffer;
                            }

                            ptr = ikcp_encode_seg(ptr, segment);
                            if (segment->len > 0)
                            {
                                memcpy(ptr, segment->data, segment->len);
                                ptr += segment->len;
                            }

                            if (segment->xmit >= DEADLINK)
                                kcp->state = -1;
                        }
                    }
                }
                else if (kcp->nodelay == 1)
                {
                    for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = p->next)
                    {
                        var segment = iqueue_entry(p);
                        var needsend = 0;
                        if (segment->xmit == 0)
                        {
                            needsend = 1;
                            segment->xmit++;
                            segment->rto = (uint)kcp->rx_rto;
                            segment->resendts = current + segment->rto;
                        }
                        else if (_itimediff(current, segment->resendts) >= 0)
                        {
                            needsend = 1;
                            segment->xmit++;
                            kcp->xmit++;
                            var step = (int)segment->rto;
                            segment->rto += (uint)(step / 2);
                            segment->resendts = current + segment->rto;
                            lost = 1;
                        }
                        else if (segment->fastack >= resent)
                        {
                            if ((int)segment->xmit <= kcp->fastlimit || kcp->fastlimit == 0)
                            {
                                needsend = 1;
                                segment->xmit++;
                                segment->fastack = 0;
                                segment->resendts = current + segment->rto;
                                change++;
                            }
                        }

                        if (needsend != 0)
                        {
                            segment->ts = current;
                            segment->wnd = seg.wnd;
                            segment->una = kcp->rcv_nxt;
                            size = (int)(ptr - buffer);
                            var need = (int)(OVERHEAD + segment->len);
                            if (size + need > (int)kcp->mtu)
                            {
                                ikcp_output(output, bytes, size);
                                ptr = buffer;
                            }

                            ptr = ikcp_encode_seg(ptr, segment);
                            if (segment->len > 0)
                            {
                                memcpy(ptr, segment->data, segment->len);
                                ptr += segment->len;
                            }

                            if (segment->xmit >= DEADLINK)
                                kcp->state = -1;
                        }
                    }
                }
                else
                {
                    for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = p->next)
                    {
                        var segment = iqueue_entry(p);
                        var needsend = 0;
                        if (segment->xmit == 0)
                        {
                            needsend = 1;
                            segment->xmit++;
                            segment->rto = (uint)kcp->rx_rto;
                            segment->resendts = current + segment->rto;
                        }
                        else if (_itimediff(current, segment->resendts) >= 0)
                        {
                            needsend = 1;
                            segment->xmit++;
                            kcp->xmit++;
                            var step = (int)segment->rto;
                            segment->rto += (uint)(step / 2);
                            segment->resendts = current + segment->rto;
                            lost = 1;
                        }
                        else if (segment->fastack >= resent)
                        {
                            if ((int)segment->xmit <= kcp->fastlimit || kcp->fastlimit == 0)
                            {
                                needsend = 1;
                                segment->xmit++;
                                segment->fastack = 0;
                                segment->resendts = current + segment->rto;
                                change++;
                            }
                        }

                        if (needsend != 0)
                        {
                            segment->ts = current;
                            segment->wnd = seg.wnd;
                            segment->una = kcp->rcv_nxt;
                            size = (int)(ptr - buffer);
                            var need = (int)(OVERHEAD + segment->len);
                            if (size + need > (int)kcp->mtu)
                            {
                                ikcp_output(output, bytes, size);
                                ptr = buffer;
                            }

                            ptr = ikcp_encode_seg(ptr, segment);
                            if (segment->len > 0)
                            {
                                memcpy(ptr, segment->data, segment->len);
                                ptr += segment->len;
                            }

                            if (segment->xmit >= DEADLINK)
                                kcp->state = -1;
                        }
                    }
                }

                size = (int)(ptr - buffer);
                if (size > 0)
                    ikcp_output(output, bytes, size);
                if (change != 0)
                {
                    var inflight = kcp->snd_nxt - kcp->snd_una;
                    kcp->ssthresh = inflight / 2;
                    if (kcp->ssthresh < THRESH_MIN)
                        kcp->ssthresh = THRESH_MIN;
                    kcp->cwnd = kcp->ssthresh + resent;
                    kcp->incr = kcp->cwnd * kcp->mss;
                }

                if (lost != 0)
                {
                    kcp->ssthresh = cwnd / 2;
                    if (kcp->ssthresh < THRESH_MIN)
                        kcp->ssthresh = THRESH_MIN;
                    kcp->cwnd = 1;
                    kcp->incr = kcp->mss;
                }

                if (kcp->cwnd < 1)
                {
                    kcp->cwnd = 1;
                    kcp->incr = kcp->mss;
                }
            }
        }

        public static void ikcp_update(IKCPCB* kcp, uint current, KcpCallback output, byte[] bytes)
        {
            kcp->current = current;
            if (kcp->updated == 0)
            {
                kcp->updated = 1;
                kcp->ts_flush = kcp->current;
            }

            var slap = _itimediff(kcp->current, kcp->ts_flush);
            if (slap >= 10000 || slap < -10000)
            {
                kcp->ts_flush = kcp->current;
                slap = 0;
            }

            if (slap >= 0)
            {
                kcp->ts_flush += kcp->interval;
                if (_itimediff(kcp->current, kcp->ts_flush) >= 0)
                    kcp->ts_flush = kcp->current + kcp->interval;
                ikcp_flush_internal(kcp, output, bytes);
            }
        }

        public static uint ikcp_check(IKCPCB* kcp, uint current)
        {
            if (kcp->updated == 0)
                return current;
            var ts_flush = kcp->ts_flush;
            if (_itimediff(current, ts_flush) >= 10000 || _itimediff(current, ts_flush) < -10000)
                ts_flush = current;
            if (_itimediff(current, ts_flush) >= 0)
                return current;
            var tm_packet = 2147483647;
            var tm_flush = _itimediff(ts_flush, current);
            IQUEUEHEAD* p;
            for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = p->next)
            {
                var seg = iqueue_entry(p);
                var diff = _itimediff(seg->resendts, current);
                if (diff <= 0)
                    return current;
                if (diff < tm_packet)
                    tm_packet = diff;
            }

            var minimal = (uint)(tm_packet < tm_flush ? tm_packet : tm_flush);
            if (minimal >= kcp->interval)
                minimal = kcp->interval;
            return current + minimal;
        }

        public static int ikcp_setmtu(IKCPCB* kcp, int mtu)
        {
            if (kcp->mtu == (uint)mtu)
                return 0;
            if (mtu < (int)OVERHEAD)
                return -1;
            kcp->mtu = (uint)mtu;
            kcp->mss = kcp->mtu - OVERHEAD;
            return 0;
        }

        public static void ikcp_interval(IKCPCB* kcp, int interval)
        {
            interval = _iclamp_(interval, INTERVAL_MIN, INTERVAL_LIMIT);
            kcp->interval = (uint)interval;
        }

        public static void ikcp_nodelay(IKCPCB* kcp, int nodelay, int interval, int resend, int nc)
        {
            nodelay = _iclamp_(nodelay, NODELAY_MIN, NODELAY_LIMIT);
            kcp->nodelay = (uint)nodelay;
            if (nodelay != 0)
                kcp->rx_minrto = (int)RTO_NDL;
            else
                kcp->rx_minrto = (int)RTO_MIN;
            interval = _iclamp_(interval, INTERVAL_MIN, INTERVAL_LIMIT);
            kcp->interval = (uint)interval;
            resend = _iclamp_(resend, 0, 4294967295);
            kcp->fastresend = resend;
            kcp->nocwnd = nc == 1 ? 1 : 0;
        }

        public static void ikcp_wndsize(IKCPCB* kcp, int sndwnd, int rcvwnd)
        {
            sndwnd = _iclamp_(sndwnd, WND_SND, 2147483647);
            rcvwnd = _iclamp_(rcvwnd, WND_RCV, 2147483647);
            kcp->snd_wnd = (uint)sndwnd;
            kcp->rcv_wnd = (uint)rcvwnd;
        }

        public static void ikcp_fastresendlimit(IKCPCB* kcp, int fastlimit)
        {
            fastlimit = _iclamp_(fastlimit, FASTACK_MIN, FASTACK_LIMIT);
            kcp->fastlimit = fastlimit;
        }

        public static void ikcp_streammode(IKCPCB* kcp, int stream) => kcp->stream = stream == 1 ? 1 : 0;

        public static void ikcp_minrto(IKCPCB* kcp, int minrto)
        {
            minrto = _iclamp_(minrto, INTERVAL_MIN, RTO_MAX);
            kcp->rx_minrto = minrto;
        }
    }
}