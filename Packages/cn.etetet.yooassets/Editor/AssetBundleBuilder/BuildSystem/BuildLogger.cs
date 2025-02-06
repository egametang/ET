using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset.Editor
{
    internal static class BuildLogger
    {
        private static bool _enableLog = true;

        public static void InitLogger(bool enableLog)
        {
            _enableLog = enableLog;
        }

        public static void Log(string message)
        {
            if (_enableLog)
            {
                Debug.Log(message);
            }
        }
        public static void Warning(string message)
        {
            Debug.LogWarning(message);
        }
        public static void Error(string message)
        {
            Debug.LogError(message);
        }

        public static string GetErrorMessage(ErrorCode code, string message)
        {
            return $"[ErrorCode{(int)code}] {message}";
        }
    }
}