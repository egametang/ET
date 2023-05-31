using System;
using System.Runtime.InteropServices;

namespace ET
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int KcpOutput(IntPtr buf, int len, IntPtr kcp, IntPtr user);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void KcpLog(IntPtr buf, int len, IntPtr kcp, IntPtr user);

    public class Kcp
    {
        private IntPtr intPtr;

        public Kcp(uint conv, IntPtr user)
        {
            this.intPtr = ikcp_create(conv, user);
        }

        public long Id
        {
            get
            {
                return this.intPtr.ToInt64();
            }
        }

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
        
        public uint Check(uint current)
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            uint ret = ikcp_check(this.intPtr, current);
            return ret;
        }
        
        public void Flush()
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            ikcp_flush(this.intPtr);
        }

        public uint GetConv()
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            return ikcp_getconv(this.intPtr);
        }

        public int Input(byte[] buffer, int offset, int len)
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            if (offset + len > buffer.Length)
            {
                throw new Exception($"kcp error, KcpInput {buffer.Length} {offset} {len}");
            }
            int ret = ikcp_input(this.intPtr, buffer, offset, len);
            return ret;
        }

        public int Nodelay(int nodelay, int interval, int resend, int nc)
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            return ikcp_nodelay(this.intPtr, nodelay, interval, resend, nc);
        }

        public int Peeksize()
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            int ret = ikcp_peeksize(this.intPtr);
            return ret;
        }

        public int Recv(byte[] buffer, int index, int len)
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }

            if (buffer.Length < index + len)
            {
                throw new Exception($"kcp error, KcpRecv error: {index} {len}");
            }
            
            int ret = ikcp_recv(this.intPtr, buffer, index, len);
            return ret;
        }

        public void Release()
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            ikcp_release(this.intPtr);
            this.intPtr = IntPtr.Zero;
        }

        public int Send(byte[] buffer, int offset, int len)
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }

            if (offset + len > buffer.Length)
            {
                throw new Exception($"kcp error, KcpSend {buffer.Length} {offset} {len}");
            }
            
            int ret = ikcp_send(this.intPtr, buffer, offset, len);
            return ret;
        }

        public void SetMinrto(int minrto)
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            ikcp_setminrto(this.intPtr, minrto);
        }

        public int SetMtu(int mtu)
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            return ikcp_setmtu(this.intPtr, mtu);
        }

        public static void SetOutput(KcpOutput output)
        {
            KcpOutput = output;
            ikcp_setoutput(KcpOutput);
        }
        
        public static void SetLog(KcpLog kcpLog)
        {
            KcpLog = kcpLog;
            ikcp_setlog(KcpLog);
        }

        public void Update(uint current)
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            ikcp_update(this.intPtr, current);
        }

        public int Waitsnd()
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            int ret = ikcp_waitsnd(this.intPtr);
            return ret;
        }

        public int SetWndSize(int sndwnd, int rcvwnd)
        {
            if (this.intPtr == IntPtr.Zero)
            {
                throw new Exception($"kcp error, kcp point is zero");
            }
            return ikcp_wndsize(this.intPtr, sndwnd, rcvwnd);
        }
    }
}

