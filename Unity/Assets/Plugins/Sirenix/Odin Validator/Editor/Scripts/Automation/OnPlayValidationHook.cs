//-----------------------------------------------------------------------
// <copyright file="OnPlayValidationHook.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinValidator.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;

    using Callback =
#if UNITY_2017_2_OR_NEWER
        System.Action<UnityEditor.PlayModeStateChange>;
#else
        UnityEditor.EditorApplication.CallbackFunction;
#endif

    public class OnPlayValidationHook : IValidationHook
    {
        private static Dictionary<Action, Callback> SubscriptionMap = new Dictionary<Action, Callback>();

        public string Name { get { return "On Play"; } }

        public void Hook(Action run)
        {
            if (SubscriptionMap.ContainsKey(run)) return;

#if UNITY_2017_2_OR_NEWER
            Callback sub = (e) =>
#else
            Callback sub = () =>
#endif
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                    run();
            };

            SubscriptionMap[run] = sub;

#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += sub;
#else
            EditorApplication.playmodeStateChanged += sub;
#endif
        }

        public void Unhook(Action run)
        {
            Callback sub;
            if (!SubscriptionMap.TryGetValue(run, out sub))
                return;

#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged -= sub;
#else
            EditorApplication.playmodeStateChanged -= sub;
#endif
            SubscriptionMap.Remove(run);
        }

        public void StopTriggeringEvent()
        {
            EditorApplication.isPlaying = false;
        }
    }

}
