using System;

namespace ET
{
    public static class Log
    {
        public static void Trace(string msg)
        {
#if UNITY
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
                return;
            }
#endif
            Logger.Instance.Trace(msg);
        }

        public static void Debug(string msg)
        {
#if UNITY
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
                return;
            }
#endif
            Logger.Instance.Debug(msg);
        }

        public static void Info(string msg)
        {
#if UNITY
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
                return;
            }
#endif
            Logger.Instance.Info(msg);
        }

        public static void TraceInfo(string msg)
        {
#if UNITY
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
                return;
            }
#endif
            Logger.Instance.Trace(msg);
        }

        public static void Warning(string msg)
        {
#if UNITY
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.LogWarning(msg);
                return;
            }
#endif
            Logger.Instance.Warning(msg);
        }

        public static void Error(string msg)
        {
#if UNITY
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.LogError(msg);
                return;
            }
#endif      
            Logger.Instance.Error(msg);
        }

        public static void Error(Exception msg)
        {
#if UNITY     
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.LogError(msg);
                return;
            }
 #endif          
            Logger.Instance.Error(msg);
        }

        public static void Console(string msg)
        {
#if UNITY
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Debug.Log(msg);
                return;
            }
#endif  
            Logger.Instance.Console(msg);
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
