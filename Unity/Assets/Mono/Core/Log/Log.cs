using System;
using System.Diagnostics;
using System.IO;
using System.Net;

#if NOT_UNITY
using NLog;
#endif

namespace ET
{
    public static class Log
    {
        public static ILog ILog { get; set; }
        
        private const int TraceLevel = 1;
        private const int DebugLevel = 2;
        private const int InfoLevel = 3;
        private const int WarningLevel = 4;

        private static bool CheckLogLevel(int level)
        {
            return Options.Instance.LogLevel <= level;
        }
        
        public static void Trace(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(1, true);
            ILog.Trace($"{msg}\n{st}");
        }

        public static void Debug(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
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
            ILog.Error($"{msg}\n{st}");
        }

        public static void Error(Exception e)
        {
            if (e.Data.Contains("StackTrace"))
            {
                ILog.Error($"{e.Data["StackTrace"]}\n{e}");
                return;
            }
            string str = e.ToString();
            ILog.Error(str);
        }

        public static void Trace(string message, params object[] args)
        {
            if (!CheckLogLevel(TraceLevel))
            {
                return;
            }
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
            ILog.Debug(string.Format(message, args));

        }

        public static void Error(string message, params object[] args)
        {
            StackTrace st = new StackTrace(1, true);
            string s = string.Format(message, args) + '\n' + st;
            ILog.Error(s);
        }
        
        public static void Console(string message)
        {
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(message);
            }
            ILog.Debug(message);
        }
        
        public static void Console(string message, params object[] args)
        {
            string s = string.Format(message, args);
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(s);
            }
            ILog.Debug(s);
        }
    }
}