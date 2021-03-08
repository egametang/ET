using System;
using System.Diagnostics;
using System.IO;
using System.Net;

#if NOT_CLIENT
using NLog;
#endif

namespace ET
{
    public static class Log
    {
        public const int TraceLevel = 1;
        public const int DebugLevel = 2;
        public const int InfoLevel = 3;
        public const int WarningLevel = 4;
        
        public static ILog ILog { get; }

        static Log()
        {
#if SERVER
            ILog = new NLogger("Server");
#else
            ILog = new UnityLogger();
#endif
        }

        public static bool CheckLogLevel(int level)
        {
            if (Game.Options == null)
            {
                return true;
            }
            
            return Game.Options.LogLevel <= level;
        }
        
        public static Action<string, object[]> DebugCallback;
        public static Action<string> ErrorCallback;

        public static void Trace(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            DebugCallback?.Invoke(msg, null);
            StackTrace st = new StackTrace(1, true);
            ILog.Trace($"{msg}\n{st}");
        }

        public static void Debug(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            DebugCallback?.Invoke(msg, null);
            ILog.Debug(msg);
        }

        public static void Info(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            ILog.Info(msg);
        }

        public static void TraceInfo(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(1, true);
            ILog.Trace($"{msg}\n{st}");
        }

        public static void Warning(string msg)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }

            ILog.Warning(msg);
        }

        public static void Error(string msg)
        {
            StackTrace st = new StackTrace(1, true);
            ErrorCallback?.Invoke($"{msg}\n{st}");
            ILog.Error($"{msg}\n{st}");
        }

        public static void Error(Exception e)
        {
            string str = e.ToString();
            ErrorCallback?.Invoke(str);
            ILog.Error(str);
        }

        public static void Trace(string message, params object[] args)
        {
            if (!CheckLogLevel(TraceLevel))
            {
                return;
            }
            DebugCallback?.Invoke(message, args);
            StackTrace st = new StackTrace(1, true);
            ILog.Trace($"{string.Format(message, args)}\n{st}");
        }

        public static void Warning(string message, params object[] args)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }
            ILog.Warning(string.Format(message, args));
        }

        public static void Info(string message, params object[] args)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            ILog.Info(string.Format(message, args));
        }

        public static void Debug(string message, params object[] args)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            DebugCallback?.Invoke(message, args);
            ILog.Debug(string.Format(message, args));

        }

        public static void Error(string message, params object[] args)
        {
            StackTrace st = new StackTrace(1, true);
            string s = string.Format(message, args) + '\n' + st;
            ErrorCallback?.Invoke(s);
            ILog.Error(s);
        }
    }
}