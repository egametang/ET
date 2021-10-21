using System;
using System.Runtime.InteropServices;

namespace ET
{
    public static class Interpreter
    {
        private const string InterpreterDll = "Interpreter";
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Log(IntPtr buf, int len);
        
        private static Log log;
        
        [DllImport(InterpreterDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void interpreter_set_log(Log log);
        
        [DllImport(InterpreterDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void interpreter_init([In][MarshalAs(UnmanagedType.LPStr)] string reloadDir, [In][MarshalAs(UnmanagedType.LPStr)] string exeName);

        public static void InterpreterSetLog(Log plog)
        {
            log = plog;
            interpreter_set_log(log);
        }

        public static void InterpreterInit([In][MarshalAs(UnmanagedType.LPStr)]string reloadDir, [In][MarshalAs(UnmanagedType.LPStr)]string exeName)
        {
            interpreter_init(reloadDir, exeName);
        }
    }
}