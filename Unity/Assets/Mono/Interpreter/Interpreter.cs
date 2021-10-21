using System.Runtime.InteropServices;

namespace ET
{
    public static class Interpreter
    {
        private const string InterpreterDll = "Interpreter";
        
        [DllImport(InterpreterDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void InterpreterInit([In][MarshalAs(UnmanagedType.LPStr)] string reloadDir, [In][MarshalAs(UnmanagedType.LPStr)] string exeName);
    }
}