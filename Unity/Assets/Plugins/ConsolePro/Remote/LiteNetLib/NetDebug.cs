using System;
using System.Diagnostics;

namespace FlyingWormConsole3.LiteNetLib
{
    public class InvalidPacketException : ArgumentException
    {
        public InvalidPacketException(string message) : base(message)
        {
        }
    }

    public class TooBigPacketException : InvalidPacketException
    {
        public TooBigPacketException(string message) : base(message)
        {
        }
    }

    public enum NetLogLevel
    {
        Warning,
        Error,
        Trace,
        Info
    }

    /// <summary>
    /// Interface to implement for your own logger
    /// </summary>
    public interface INetLogger
    {
        void WriteNet(NetLogLevel level, string str, params object[] args);
    }

    /// <summary>
    /// Static class for defining your own LiteNetLib logger instead of Console.WriteLine
    /// or Debug.Log if compiled with UNITY flag
    /// </summary>
    public static class NetDebug
    {
        public static INetLogger Logger = null;
        private static readonly object DebugLogLock = new object();
        private static void WriteLogic(NetLogLevel logLevel, string str, params object[] args)
        {
            lock (DebugLogLock)
            {
                if (Logger == null)
                {
#if UNITY_4 || UNITY_5 || UNITY_5_3_OR_NEWER
                    UnityEngine.Debug.Log(string.Format(str, args));
#else
                    Console.WriteLine(str, args);
#endif
                }
                else
                {
                    Logger.WriteNet(logLevel, str, args);
                }
            }
        }

        [Conditional("DEBUG_MESSAGES")]
        internal static void Write(string str, params object[] args)
        {
            WriteLogic(NetLogLevel.Trace, str, args);
        }

        [Conditional("DEBUG_MESSAGES")]
        internal static void Write(NetLogLevel level, string str, params object[] args)
        {
            WriteLogic(level, str, args);
        }

        [Conditional("DEBUG_MESSAGES"), Conditional("DEBUG")]
        internal static void WriteForce(string str, params object[] args)
        {
            WriteLogic(NetLogLevel.Trace, str, args);
        }

        [Conditional("DEBUG_MESSAGES"), Conditional("DEBUG")]
        internal static void WriteForce(NetLogLevel level, string str, params object[] args)
        {
            WriteLogic(level, str, args);
        }

        internal static void WriteError(string str, params object[] args)
        {
            WriteLogic(NetLogLevel.Error, str, args);
        }
    }
}
