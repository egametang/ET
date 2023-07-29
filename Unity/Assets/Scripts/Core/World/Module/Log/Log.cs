using System;
using System.Diagnostics;

namespace ET
{
    public static class Log
    {
        [Conditional("DEBUG")]
        public static void Trace(string msg)
        {
            Logger.Instance.Trace(msg);
        }

        [Conditional("DEBUG")]
        public static void Debug(string msg)
        {
            Logger.Instance.Debug(msg);
        }

        public static void Info(string msg)
        {
            Logger.Instance.Info(msg);
        }

        [Conditional("DEBUG")]
        public static void Warning(string msg)
        {
            Logger.Instance.Warning(msg);
        }

        public static void Error(string msg)
        {
            Logger.Instance.Error(msg);
        }

        public static void Error(Exception msg)
        {
            Logger.Instance.Error(msg);
        }

        [Conditional("DEBUG")]
        public static void Console(string msg)
        {
            Logger.Instance.Console(msg);
        }
        
#if DOTNET
        [Conditional("DEBUG")]
        public static void Trace(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Trace(message.ToStringAndClear());
        }
        [Conditional("DEBUG")]
        public static void Warning(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Warning(message.ToStringAndClear());
        }

        public static void Info(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Info(message.ToStringAndClear());
        }
        [Conditional("DEBUG")]
        public static void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Debug(message.ToStringAndClear());
        }

        public static void Error(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Error(message.ToStringAndClear());
        }
        [Conditional("DEBUG")]
        public static void Console(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Console(message.ToStringAndClear());
        }
#endif
    }
}
