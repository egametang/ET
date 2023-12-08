using System;
using System.IO;
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
            string stackTrace = GetStackTrace();
            if (string.IsNullOrEmpty(stackTrace))
            {
                return false;
            }

            // 编译错误不重定向
            Match compileErrorMatch = Regex.Match(stackTrace, @"(.*?)\(([0-9]+),([0-9]+)\): error");
            if (compileErrorMatch.Success)
            {
                return false;
            }

            Regex logFileRegex = new(@"((Log\.cs)|(UnityLogger\.cs)|(YooLogger\.cs))");
            Match stackLineMatch = Regex.Match(stackTrace, $@"\(at (.+):{line}\)");
            if (!stackLineMatch.Success)
            {
                // 没堆栈 不重定向
                return false;
            }
            if (stackLineMatch.Success)
            {
                string codePath = stackLineMatch.Groups[1].Value;
                if (!logFileRegex.IsMatch(codePath))
                {
                    // 不是相关文件不重定向
                    return false;
                }
            }

            // 重定向
            stackLineMatch = Regex.Match(stackTrace, @"\(at (.+):([0-9]+)\)");
            while (stackLineMatch.Success)
            {
                string codePath = stackLineMatch.Groups[1].Value;
                if (!logFileRegex.IsMatch(codePath))
                {
                    int matchLine = int.Parse(stackLineMatch.Groups[2].Value);
                    OpenIDE(codePath, matchLine);
                    return true;
                }
                stackLineMatch = stackLineMatch.NextMatch();
            }

            return false;
        }

        private static void OpenIDE(string path, int line, int column = 0)
        {
            if (!Path.IsPathFullyQualified(path))
            {
                path = Path.GetFullPath(path);
            }
            // 跳转到目标代码的特定行
            InternalEditorUtility.OpenFileAtLineExternal(path, line, column);
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
