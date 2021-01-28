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
        
#if SERVER
		private static readonly ILog logger = new NLogger("Server");
#elif ROBOT
        private static readonly ILog logger = new NLogger("Robot");
#elif UNITY_EDITOR
        private static readonly ILog logger = new UnityLogger();
#elif UNITY_STANDALONE_WIN
        //这里都切换成为第三方插件的输出，会自动输出文件
        //private static readonly ILog logger = new UnityLogger();
        private static readonly ILog logger = new FileLogger("./Log.txt");
#else
        private static readonly ILog logger = new UnityLogger();
#endif

        public static ILog Logger => logger;

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
            logger.Trace($"{msg}\n{st}");
        }

        public static void Debug(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            DebugCallback?.Invoke(msg, null);
            logger.Debug(msg);
        }

        public static void Info(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            logger.Info(msg);
        }

        public static void TraceInfo(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(1, true);
            logger.Trace($"{msg}\n{st}");
        }

        public static void Warning(string msg)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }

            logger.Warning(msg);
        }

        public static void Error(string msg)
        {
            StackTrace st = new StackTrace(1, true);
            ErrorCallback?.Invoke($"{msg}\n{st}");
            logger.Error($"{msg}\n{st}");
        }

        public static void Error(Exception e)
        {
            string str = e.ToString();
            ErrorCallback?.Invoke(str);
            logger.Error(str);
        }

        public static void Trace(string message, params object[] args)
        {
            if (!CheckLogLevel(TraceLevel))
            {
                return;
            }
            DebugCallback?.Invoke(message, args);
            StackTrace st = new StackTrace(1, true);
            logger.Trace($"{string.Format(message, args)}\n{st}");
        }

        public static void Warning(string message, params object[] args)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }
            logger.Warning(string.Format(message, args));
        }

        public static void Info(string message, params object[] args)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            logger.Info(string.Format(message, args));
        }

        public static void Debug(string message, params object[] args)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            DebugCallback?.Invoke(message, args);
            logger.Debug(string.Format(message, args));

        }

        public static void Error(string message, params object[] args)
        {
            StackTrace st = new StackTrace(1, true);
            string s = string.Format(message, args) + '\n' + st;
            ErrorCallback?.Invoke(s);
            logger.Error(s);
        }
    }
}