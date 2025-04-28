using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace ET
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IQUEUEHEAD
    {
        public IQUEUEHEAD* next;
        public IQUEUEHEAD* prev;
    }

    public static unsafe partial class KCP
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_init(IQUEUEHEAD* head)
        {
            head->next = head;
            head->prev = head;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* iqueue_entry<T>(IQUEUEHEAD* ptr) where T : unmanaged => ((T*)(((byte*)((T*)ptr))));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_add(IQUEUEHEAD* node, IQUEUEHEAD* head)
        {
            node->prev = head;
            node->next = head->next;
            head->next->prev = node;
            head->next = node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_add_tail(IQUEUEHEAD* node, IQUEUEHEAD* head)
        {
            node->prev = head->prev;
            node->next = head;
            head->prev->next = node;
            head->prev = node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_del(IQUEUEHEAD* entry)
        {
            entry->next->prev = entry->prev;
            entry->prev->next = entry->next;
            entry->next = (IQUEUEHEAD*)0;
            entry->prev = (IQUEUEHEAD*)0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_del_init(IQUEUEHEAD* entry)
        {
            iqueue_del(entry);
            iqueue_init(entry);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool iqueue_is_empty(IQUEUEHEAD* entry) => entry == entry->next;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IKCPSEG
    {
        public IQUEUEHEAD node;
        public uint conv;
        public uint cmd;
        public uint frg;
        public uint wnd;
        public uint ts;
        public uint sn;
        public uint una;
        public uint len;
        public uint resendts;
        public uint rto;
        public uint fastack;
        public uint xmit;
        public fixed byte data[1];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IKCPCB
    {
        public uint conv, mtu, mss, state;
        public uint snd_una, snd_nxt, rcv_nxt;
        public uint ts_recent, ts_lastack, ssthresh;
        public int rx_rttval, rx_srtt, rx_rto, rx_minrto;
        public uint snd_wnd, rcv_wnd, rmt_wnd, cwnd, probe;
        public uint current, interval, ts_flush, xmit;
        public uint nrcv_buf, nsnd_buf;
        public uint nrcv_que, nsnd_que;
        public uint nodelay, updated;
        public uint ts_probe, probe_wait;
        public uint dead_link, incr;
        public IQUEUEHEAD snd_queue;
        public IQUEUEHEAD rcv_queue;
        public IQUEUEHEAD snd_buf;
        public IQUEUEHEAD rcv_buf;
        public uint* acklist;
        public uint ackcount;
        public uint ackblock;
        public int fastresend;
        public int fastlimit;
        public int nocwnd, stream;
    }

    public static partial class KCP
    {
        public const uint IKCP_LOG_OUTPUT = 1;
        public const uint IKCP_LOG_INPUT = 2;
        public const uint IKCP_LOG_SEND = 4;
        public const uint IKCP_LOG_RECV = 8;
        public const uint IKCP_LOG_IN_DATA = 16;
        public const uint IKCP_LOG_IN_ACK = 32;
        public const uint IKCP_LOG_IN_PROBE = 64;
        public const uint IKCP_LOG_IN_WINS = 128;
        public const uint IKCP_LOG_OUT_DATA = 256;
        public const uint IKCP_LOG_OUT_ACK = 512;
        public const uint IKCP_LOG_OUT_PROBE = 1024;
        public const uint IKCP_LOG_OUT_WINS = 2048;
    }
}