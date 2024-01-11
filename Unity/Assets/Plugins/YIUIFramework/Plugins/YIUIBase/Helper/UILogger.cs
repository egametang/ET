using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YIUIFramework
{
    /// <summary>
    /// 日志封装@l
    /// </summary>
    public static class Logger
    {
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        public static void Log(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public static void LogWarning(string format, params object[] args)
        {
            Debug.LogWarningFormat(format, args);
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }

        public static void LogError(string format, params object[] args)
        {
            Debug.LogErrorFormat(format, args);
        }

        public static void LogErrorContext(Object context, object message)
        {
            Debug.LogError(message, context);
        }

        public static void LogWarningContext(Object context, object message)
        {
            Debug.LogWarning(message, context);
        }

        public static void LogErrorContextFormat(Object context, string format, params object[] args)
        {
            Debug.LogErrorFormat(context, format, args);
        }

        public static void LogError(Object obj, object message)
        {
            #if UNITY_EDITOR
            SelectObj(obj);
            #endif
            Debug.LogError(message);
        }

        #if UNITY_EDITOR
        public static void SelectObj(Object obj)
        {
            Selection.activeObject = obj;
        }
        #endif
    }
}