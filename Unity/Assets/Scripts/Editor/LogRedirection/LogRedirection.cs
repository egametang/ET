using System;
using System.Diagnostics;
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

            Regex logFileRegex = new(@"((Log\.cs)|(UnityLogger\.cs)|(YooLogger\.cs))");
            string codePath = AssetDatabase.GetAssetPath(instanceID);
            if (logFileRegex.IsMatch(codePath))
            {
                var content = GetStackTrace();
                // 如果有手动输入的地址，就跳过
                var hrefMath = Regex.Match(content, @"<a href=""(.*?)"" line=""(\w+)\"">.*?</a>");
                if (hrefMath.Success)
                {
                    OpenIDE(hrefMath.Groups[1].Value, int.Parse(hrefMath.Groups[2].Value));
                    return true;
                }
                
                Match stackLineMatch = Regex.Match(content, @"\(at (.+):([0-9]+)\)");
                while (stackLineMatch.Success)
                {
                    codePath = stackLineMatch.Groups[1].Value;
                    if (!logFileRegex.IsMatch(codePath))
                    {
                        int matchLine = int.Parse(stackLineMatch.Groups[2].Value);
                        OpenIDE(codePath, matchLine);
                        return true;
                    }
                    stackLineMatch = stackLineMatch.NextMatch();
                }
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
                {
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
            }

            return null;
        }
    }
}
