
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;
using MonoHook;
using HybridCLR.Editor.BuildProcessors;
using System.IO;

namespace HybridCLR.MonoHook
{
#if UNITY_2021_1_OR_NEWER && (UNITY_WEBGL || UNITY_WEIXINMINIGAME)
    [InitializeOnLoad]
    public class PatchScriptingAssembliesJsonHook
    {
        private static MethodHook _hook;

        static PatchScriptingAssembliesJsonHook()
        {
            if (_hook == null)
            {
                Type type = typeof(UnityEditor.EditorApplication);
                MethodInfo miTarget = type.GetMethod("BuildMainWindowTitle", BindingFlags.Static | BindingFlags.NonPublic);

                MethodInfo miReplacement = new Func<string>(BuildMainWindowTitle).Method;
                MethodInfo miProxy = new Func<string>(BuildMainWindowTitleProxy).Method;

                _hook = new MethodHook(miTarget, miReplacement, miProxy);
                _hook.Install();
            }
        }

        private static string BuildMainWindowTitle()
        {
        var cacheDir = $"{Application.dataPath}/../Library/PlayerDataCache";
        if (Directory.Exists(cacheDir))
            {
                foreach (var tempJsonPath in Directory.GetDirectories(cacheDir, "*", SearchOption.TopDirectoryOnly))
                {
                    string dirName = Path.GetFileName(tempJsonPath);
 #if UNITY_WEIXINMINIGAME
                    if (!dirName.Contains("WeixinMiniGame"))
                    {
                        continue;
                    }
#else
                    if (!dirName.Contains("WebGL"))
                    {
                        continue;
                    }
#endif

                    var patcher = new PatchScriptingAssemblyList();
                    patcher.PathScriptingAssembilesFile(tempJsonPath);
                }
            }

            string newTitle = BuildMainWindowTitleProxy();
            return newTitle;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static string BuildMainWindowTitleProxy()
        {
            // 为满足MonoHook要求的最小代码长度而特地加入的无用填充代码，
            UnityEngine.Debug.Log(12345.ToString());
            return string.Empty;
        }
    }
#endif
}
