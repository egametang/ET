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
        private const int TraceLevel = 1;
        private const int DebugLevel = 2;
        private const int InfoLevel = 3;
        private const int WarningLevel = 4;

        static Log()
        {
#if !NOT_UNITY
            Game.ILog = new UnityLogger();
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
        
        public static void Trace(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(1, true);
            Game.ILog.Trace($"{msg}\n{st}");
        }

        public static void Debug(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            Game.ILog.Debug(msg);
        }

        public static void Info(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            Game.ILog.Info(msg);
        }

        public static void TraceInfo(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(1, true);
            Game.ILog.Trace($"{msg}\n{st}");
        }

        public static void Warning(string msg)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }

            Game.ILog.Warning(msg);
        }

        public static void Error(string msg)
        {
            StackTrace st = new StackTrace(1, true);
            Game.ILog.Error($"{msg}\n{st}");
        }

        public static void Error(Exception e)
        {
            string str = e.ToString();
            Game.ILog.Error(str);
        }

        public static void Trace(string message, params object[] args)
        {
            if (!CheckLogLevel(TraceLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(1, true);
            Game.ILog.Trace($"{string.Format(message, args)}\n{st}");
        }

        public static void Warning(string message, params object[] args)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }
            Game.ILog.Warning(string.Format(message, args));
        }

        public static void Info(string message, params object[] args)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            Game.ILog.Info(string.Format(message, args));
        }

        public static void Debug(string message, params object[] args)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            Game.ILog.Debug(string.Format(message, args));

        }

        public static void Error(string message, params object[] args)
        {
            StackTrace st = new StackTrace(1, true);
            string s = string.Format(message, args) + '\n' + st;
            Game.ILog.Error(s);
        }
        
        public static void Console(string message)
        {
            if (Game.Options.Console == 1)
            {
                System.Console.WriteLine(message);
            }
            Game.ILog.Debug(message);
        }
        
        public static void Console(string message, params object[] args)
        {
            string s = string.Format(message, args);
            if (Game.Options.Console == 1)
            {
                System.Console.WriteLine(s);
            }
            Game.ILog.Debug(s);
        }
    }
}