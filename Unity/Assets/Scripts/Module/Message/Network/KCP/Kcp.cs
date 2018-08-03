using System;
using System.Runtime.InteropServices;

namespace ETModel
{

    
    public delegate int kcp_output(IntPtr buf, int len, IntPtr kcp, IntPtr user);

    public class Kcp
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        const string KcpDLL = "__Internal";
#else
        const string KcpDLL = "kcp";
#endif

        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern uint ikcp_check(IntPtr kcp, uint current);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr ikcp_create(uint conv, IntPtr user);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern void ikcp_flush(IntPtr kcp);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern uint ikcp_getconv(IntPtr ptr);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern int ikcp_input(IntPtr kcp, byte[] data, long size);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern int ikcp_nodelay(IntPtr kcp, int nodelay, int interval, int resend, int nc);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern int ikcp_peeksize(IntPtr kcp);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern int ikcp_recv(IntPtr kcp, byte[] buffer, int len);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern void ikcp_release(IntPtr kcp);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern int ikcp_send(IntPtr kcp, byte[] buffer, int len);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern void ikcp_setminrto(IntPtr ptr, int minrto);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern int ikcp_setmtu(IntPtr kcp, int mtu);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern void ikcp_setoutput(IntPtr kcp, kcp_output output);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern void ikcp_update(IntPtr kcp, uint current);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern int ikcp_waitsnd(IntPtr kcp);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        public static extern int ikcp_wndsize(IntPtr kcp, int sndwnd, int rcvwnd);
        
        public static uint KcpCheck(IntPtr kcp, uint current)
        {
            return ikcp_check(kcp, current);
        }
        
        public static IntPtr KcpCreate(uint conv, IntPtr user)
        {
            return ikcp_create(conv, user);
        }

        public static void KcpFlush(IntPtr kcp)
        {
            ikcp_flush(kcp);
        }

        public static uint KcpGetconv(IntPtr ptr)
        {
            return ikcp_getconv(ptr);
        }

        public static int KcpInput(IntPtr kcp, byte[] data, long size)
        {
            return ikcp_input(kcp, data, size);
        }

        public static int KcpNodelay(IntPtr kcp, int nodelay, int interval, int resend, int nc)
        {
            return ikcp_nodelay(kcp, nodelay, interval, resend, nc);
        }

        public static int KcpPeeksize(IntPtr kcp)
        {
            return ikcp_peeksize(kcp);
        }

        public static int KcpRecv(IntPtr kcp, byte[] buffer, int len)
        {
            return ikcp_recv(kcp, buffer, len);
        }

        public static void KcpRelease(IntPtr kcp)
        {
            ikcp_release(kcp);
        }

        public static int KcpSend(IntPtr kcp, byte[] buffer, int len)
        {
            return ikcp_send(kcp, buffer, len);
        }

        public static void KcpSetminrto(IntPtr ptr, int minrto)
        {
            ikcp_setminrto(ptr, minrto);
        }

        public static int KcpSetmtu(IntPtr kcp, int mtu)
        {
            return ikcp_setmtu(kcp, mtu);
        }

        public static void KcpSetoutput(IntPtr kcp, kcp_output output)
        {
            ikcp_setoutput(kcp, output);
        }

        public static void KcpUpdate(IntPtr kcp, uint current)
        {
            ikcp_update(kcp, current);
        }

        public static int KcpWaitsnd(IntPtr kcp)
        {
            return ikcp_waitsnd(kcp);
        }

        public static int KcpWndsize(IntPtr kcp, int sndwnd, int rcvwnd)
        {
            return ikcp_wndsize(kcp, sndwnd, rcvwnd);
        }
    }
}

