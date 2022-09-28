﻿using System;
using System.Runtime.InteropServices;

namespace ET
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int KcpOutput(IntPtr buf, int len, IntPtr kcp, IntPtr user);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void KcpLog(IntPtr buf, int len, IntPtr kcp, IntPtr user);

    public static class Kcp
    {
        public const int OneM = 1024 * 1024;
        public const int InnerMaxWaitSize = 1024 * 1024;
        public const int OuterMaxWaitSize = 1024 * 1024;
        
        
        private static KcpOutput KcpOutput;
        private static KcpLog KcpLog;
        
#if UNITY_IPHONE && !UNITY_EDITOR
        const string KcpDLL = "__Internal";
#else
        const string KcpDLL = "kcp";
#endif
        
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern uint ikcp_check(IntPtr kcp, uint current);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern IntPtr ikcp_create(uint conv, IntPtr user);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern void ikcp_flush(IntPtr kcp);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern uint ikcp_getconv(IntPtr ptr);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int ikcp_input(IntPtr kcp, byte[] data, int offset, int size);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int ikcp_nodelay(IntPtr kcp, int nodelay, int interval, int resend, int nc);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int ikcp_peeksize(IntPtr kcp);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int ikcp_recv(IntPtr kcp, byte[] buffer, int index, int len);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern void ikcp_release(IntPtr kcp);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int ikcp_send(IntPtr kcp, byte[] buffer, int offset, int len);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern void ikcp_setminrto(IntPtr ptr, int minrto);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int ikcp_setmtu(IntPtr kcp, int mtu);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern void ikcp_setoutput(KcpOutput output);
        
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern void ikcp_setlog(KcpLog log);
        
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern void ikcp_update(IntPtr kcp, uint current);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int ikcp_waitsnd(IntPtr kcp);
        [DllImport(KcpDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int ikcp_wndsize(IntPtr kcp, int sndwnd, int rcvwnd);
        
        public static uint KcpCheck(IntPtr kcp, uint current)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            uint ret = ikcp_check(kcp, current);
            return ret;
        }
        
        public static IntPtr KcpCreate(uint conv, IntPtr user)
        {
            return ikcp_create(conv, user);
        }

        public static void KcpFlush(IntPtr kcp)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            ikcp_flush(kcp);
        }

        public static uint KcpGetconv(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            return ikcp_getconv(ptr);
        }

        public static int KcpInput(IntPtr kcp, byte[] buffer, int offset, int len)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            if (offset + len > buffer.Length)
            {
                throw new Exception($"kcp error, KcpInput {buffer.Length} {offset} {len}");
            }
            int ret = ikcp_input(kcp, buffer, offset, len);
            return ret;
        }

        public static int KcpNodelay(IntPtr kcp, int nodelay, int interval, int resend, int nc)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            return ikcp_nodelay(kcp, nodelay, interval, resend, nc);
        }

        public static int KcpPeeksize(IntPtr kcp)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            int ret = ikcp_peeksize(kcp);
            return ret;
        }

        public static int KcpRecv(IntPtr kcp, byte[] buffer, int index, int len)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }

            if (buffer.Length < index + len)
            {
                throw new Exception($"kcp error, KcpRecv error: {index} {len}");
            }
            
            int ret = ikcp_recv(kcp, buffer, index, len);
            return ret;
        }

        public static void KcpRelease(IntPtr kcp)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            ikcp_release(kcp);
        }

        public static int KcpSend(IntPtr kcp, byte[] buffer, int offset, int len)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }

            if (offset + len > buffer.Length)
            {
                throw new Exception($"kcp error, KcpSend {buffer.Length} {offset} {len}");
            }
            
            int ret = ikcp_send(kcp, buffer, offset, len);
            return ret;
        }

        public static void KcpSetminrto(IntPtr kcp, int minrto)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            ikcp_setminrto(kcp, minrto);
        }

        public static int KcpSetmtu(IntPtr kcp, int mtu)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            return ikcp_setmtu(kcp, mtu);
        }

        public static void KcpSetoutput(KcpOutput output)
        {
            KcpOutput = output;
            ikcp_setoutput(KcpOutput);
        }
        
        public static void KcpSetLog(KcpLog kcpLog)
        {
            KcpLog = kcpLog;
            ikcp_setlog(KcpLog);
        }

        public static void KcpUpdate(IntPtr kcp, uint current)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            ikcp_update(kcp, current);
        }

        public static int KcpWaitsnd(IntPtr kcp)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            int ret = ikcp_waitsnd(kcp);
            return ret;
        }

        public static int KcpWndsize(IntPtr kcp, int sndwnd, int rcvwnd)
        {
            if (kcp == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            return ikcp_wndsize(kcp, sndwnd, rcvwnd);
        }
    }
}

