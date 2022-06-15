using System;
using System.Diagnostics;

namespace ET
{
    public static class Log
    {
        private const int TraceLevel = 1;
        private const int DebugLevel = 2;
        private const int InfoLevel = 3;
        private const int WarningLevel = 4;

        private static bool CheckLogLevel(int level)
        {
            if (Options.Instance == null)
            {
                return true;
            }
            return Options.Instance.LogLevel <= level;
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
            if (e.Data.Contains("StackTrace"))
            {
                Game.ILog.Error($"{e.Data["StackTrace"]}\n{e}");
                return;
            }
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
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(message);
            }
            Game.ILog.Debug(message);
        }
        
        public static void Console(string message, params object[] args)
        {
            string s = string.Format(message, args);
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(s);
            }
            Game.ILog.Debug(s);
        }
    }
}