using System;
using System.Runtime.InteropServices;

namespace ET
{
    public static class Interpreter
    {
        private const string InterpreterDll = "Interpreter";
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LogCallback(IntPtr buf, int len);
        
        private static LogCallback logCallback;
        
        [DllImport(InterpreterDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void interpreter_set_log(LogCallback logCallback);
        
        [DllImport(InterpreterDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void interpreter_log([In][MarshalAs(UnmanagedType.LPStr)] string str);
        
        [DllImport(InterpreterDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void interpreter_init([In][MarshalAs(UnmanagedType.LPStr)] string reloadDir, [In][MarshalAs(UnmanagedType.LPStr)] string exeName);

        public static void InterpreterSetLog(LogCallback plog)
        {
            logCallback = plog;
            interpreter_set_log(logCallback);
        }
        
        public static void InterpreterLog(string str)
        {
            interpreter_log(str);
        }

        public static void InterpreterInit([In][MarshalAs(UnmanagedType.LPStr)]string reloadDir, [In][MarshalAs(UnmanagedType.LPStr)]string exeName)
        {
            interpreter_init(reloadDir, exeName);
        }
    }
}