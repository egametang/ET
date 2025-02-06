
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
#if UNITY_2022 || UNITY_2023_1_OR_NEWER
    [InitializeOnLoad]
    public class GetIl2CppFolderHook
    {
        private static MethodHook _hook;

        static GetIl2CppFolderHook()
        {
            if (_hook == null)
            {
                Type type = typeof(UnityEditor.EditorApplication).Assembly.GetType("UnityEditorInternal.IL2CPPUtils");
                MethodInfo miTarget = type.GetMethod("GetIl2CppFolder", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null,
                    new Type[] { typeof(bool).MakeByRefType() }, null);

                MethodInfo miReplacement = new StripAssembliesDel(OverrideMethod).Method;
                MethodInfo miProxy = new StripAssembliesDel(PlaceHolderMethod).Method;

                _hook = new MethodHook(miTarget, miReplacement, miProxy);
                _hook.Install();
            }
        }

        private delegate string StripAssembliesDel(out bool isDevelopmentLocation);

        private static string OverrideMethod(out bool isDevelopmentLocation)
        {
            //Debug.Log("[GetIl2CppFolderHook] OverrideMethod");
            string result = PlaceHolderMethod(out isDevelopmentLocation);
            isDevelopmentLocation = false;
            return result;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static string PlaceHolderMethod(out bool isDevelopmentLocation)
        {
            Debug.LogError("[GetIl2CppFolderHook] PlaceHolderMethod");
            isDevelopmentLocation = false;
            return null;
        }
    }
#endif
}
