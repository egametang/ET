using System;

namespace ET
{
    public static class Log
    {
#if UNITY
        public static void Trace(string msg)
        {
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
            }
            else
            {
                Logger.Instance.Trace(msg);
            }
        }

        public static void Debug(string msg)
        {
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
            }
            else
            {
                Logger.Instance.Debug(msg);
            }
        }

        public static void Info(string msg)
        {
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
            }
            else
            {
                Logger.Instance.Info(msg);
            }
        }

        public static void TraceInfo(string msg)
        {
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
            }
            else
            {
                Logger.Instance.Trace(msg);
            }
        }

        public static void Warning(string msg)
        {
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.LogWarning(msg);
            }
            else
            {
                Logger.Instance.Warning(msg);
            }
        }

        public static void Error(string msg)
        {
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.LogError(msg);
            }
            else
            {
                Logger.Instance.Error(msg);
            }
        }

        public static void Error(Exception msg)
        {
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.LogError(msg);
            }
            else
            {
                Logger.Instance.Error(msg);
            }
        }

        public static void Console(string msg)
        {
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
            }
            else
            {
                Logger.Instance.Console(msg);
            }
        }
#else
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

        public static void Error(Exception msg)
        {
            Logger.Instance.Error(msg);
        }

        public static void Console(string msg)
        {
            Logger.Instance.Console(msg);
        }
#endif
        
        
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
