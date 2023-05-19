//-----------------------------------------------------------------------
// <copyright file="OnBuildValidationHook.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

#if UNITY_EDITOR && UNITY_5_6_OR_NEWER
    using UnityEditor.Build;

#if UNITY_2018_1_OR_NEWER
    using UnityEditor.Build.Reporting;
#endif

#if UNITY_2018_1_OR_NEWER
    public class BuildEventHookTrigger : IPreprocessBuildWithReport
#else
    public class BuildEventHookTrigger : IPreprocessBuild
#endif
    {
        public int callbackOrder { get { return -2000; } }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            OnBuildValidationHook.ExecuteHook();
        }

#if UNITY_2018_1_OR_NEWER

        public void OnPreprocessBuild(BuildReport report)
        {
            OnBuildValidationHook.ExecuteHook();
        }

#endif
    }

    public class OnBuildValidationHook : IValidationHook
    {
        public static void ExecuteHook()
        {
            foreach (var hook in Hooks)
            {
                hook();
            }
        }

        private static List<Action> Hooks = new List<Action>();

        public string Name { get { return "On Build"; } }

        public void Hook(Action run)
        {
            if (!Hooks.Contains(run))
                Hooks.Add(run);
        }

        public void Unhook(Action run)
        {
            Hooks.RemoveAll(n => n == run);
        }

        public void StopTriggeringEvent()
        {
            throw new BuildFailedException("'On Build' validation hook throwing exception to stop build process");
        }
    }
#else
    public class OnBuildValidationHook : IValidationHook
    {
        public string Name { get { return "On Build"; } }

        public void Hook(Action run)
        {
            Debug.LogWarning("'On Build' validation hook only works in Unity version 5.6 and up; event has not been hooked.");
        }

        public void Unhook(Action run)
        {
        }

        public void StopTriggeringEvent()
        {
        }
    }
#endif

}
