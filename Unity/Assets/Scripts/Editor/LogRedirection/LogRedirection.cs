using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;

namespace ET
{
    /// <summary>
    /// 日志重定向相关的实用函数。
    /// </summary>
    internal static class LogRedirection
    {
        [OnOpenAsset(0)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (line <= 0)
            {
                return false;
            }
            var stackTrace = GetStackTrace();
            if (!string.IsNullOrEmpty(stackTrace))
            {
                // 使用正则表达式匹配at的哪个脚本的哪一行
                Match logMatches = Regex.Match(stackTrace, @"\(at (.+)\)",
                    RegexOptions.IgnoreCase);
                if (!logMatches.Success)
                {
                    Match compileErrorMatch = Regex.Match(stackTrace, @"(.*?)\(([0-9]+),([0-9]+)\): error");
                    if (compileErrorMatch.Success)
                    {
                        OpenIDE(compileErrorMatch.Groups[1].Value, Convert.ToInt32(compileErrorMatch.Groups[2].Value), Convert.ToInt32(compileErrorMatch.Groups[3].Value));
                    }
                }
                while (logMatches.Success)
                {
                    var pathLine = logMatches.Groups[1].Value;

                    if (!pathLine.Contains("Log.cs") && 
                        !pathLine.Contains("UnityLogger.cs") &&
                        !pathLine.Contains("YooLogger.cs:"))
                    {
                        var splitIndex = pathLine.LastIndexOf(":", StringComparison.Ordinal);
                        // 脚本路径
                        var path = pathLine.Substring(0, splitIndex);
                        // 行号
                        line = Convert.ToInt32(pathLine.Substring(splitIndex + 1));
                        OpenIDE(path, line);
                        break;
                    }

                    logMatches = logMatches.NextMatch();
                }

                return true;
            }

            return false;
        }

        private static void OpenIDE(string path, int line, int column = 0)
        {
            var fullPath = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("Assets", StringComparison.Ordinal));
            fullPath = $"{fullPath}{path}";
#if UNITY_STANDALONE_WIN
                        fullPath = fullPath.Replace('/', '\\');
#endif
            // 跳转到目标代码的特定行
            InternalEditorUtility.OpenFileAtLineExternal(fullPath, line, column);
        }

        /// <summary>
        /// 获取当前日志窗口选中的日志的堆栈信息。
        /// </summary>
        /// <returns>选中日志的堆栈信息实例。</returns>
        private static string GetStackTrace()
        {
            // 通过反射获取ConsoleWindow类
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            // 获取窗口实例
            var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow",
                BindingFlags.Static |
                BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                var consoleInstance = fieldInfo.GetValue(null);
                if (consoleInstance != null)
                    if (EditorWindow.focusedWindow == (EditorWindow)consoleInstance)
                    {
                        // 获取m_ActiveText成员
                        fieldInfo = consoleWindowType.GetField("m_ActiveText",
                            BindingFlags.Instance |
                            BindingFlags.NonPublic);
                        // 获取m_ActiveText的值
                        if (fieldInfo != null)
                        {
                            var activeText = fieldInfo.GetValue(consoleInstance).ToString();
                            return activeText;
                        }
                    }
            }

            return null;
        }
    }
}
