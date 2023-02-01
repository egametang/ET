using System;

namespace ET
{
    public static class Log
    {
        public static void Trace(string msg)
        {
            Logger.Instance.Trace(msg);
        }

        public static void Debug(string msg)
        {
            Logger.Instance.Debug(msg);
        }

        public static void Info(string msg)
        {
            Logger.Instance.Info(msg);
        }

        public static void TraceInfo(string msg)
        {
            Logger.Instance.Trace(msg);
        }

        public static void Warning(string msg)
        {
            Logger.Instance.Warning(msg);
        }

        public static void Error(string msg)
        {
            Logger.Instance.Error(msg);
        }

        public static void Error(Exception e)
        {
            Logger.Instance.Error(e);
        }

        public static void Console(string message)
        {
            Logger.Instance.Console(message);
        }
        
#if DOTNET
        public static void Trace(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Trace(message.ToStringAndClear());
        }

        public static void Warning(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Warning(message.ToStringAndClear());
        }

        public static void Info(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Info(message.ToStringAndClear());
        }

        public static void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Debug(message.ToStringAndClear());
        }

        public static void Error(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Error(message.ToStringAndClear());
        }
        
        public static void Console(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Logger.Instance.Console(message.ToStringAndClear());
        }
#endif
    }
}