//-----------------------------------------------------------------------
// <copyright file="OnProjectStartupValidationHook.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector.Editor;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

#if UNITY_5_6_OR_NEWER

    public class OnProjectStartupValidationHook : IValidationHook
    {
        private static bool HookExecutedThisReload = false;
        private static List<Action> HookedEvents = new List<Action>();

        [InitializeOnLoadMethod]
        private static void ScheduleHook()
        {
            if (HookHasExecuted) return;

            // This is the time people have to hook into the event

            int counter = 0;

            Action count = null;

            count = () =>
            {
                if (HookHasExecuted) return;
                if (counter >= 10)
                {
                    //Debug.LogWarning("Executing project start hook");
                    HookHasExecuted = true;
                    HookExecutedThisReload = true;

                    for (int i = 0; i < HookedEvents.Count; i++)
                    {
                        HookedEvents[i]();
                    }

                    HookedEvents.Clear();
                }
                else
                {
                    counter++;
                    UnityEditorEventUtility.DelayAction(count);
                }
            };

            UnityEditorEventUtility.DelayAction(count);
        }
        
        public static bool HookHasExecuted
        {
            get
            {
                return SessionState.GetBool("OdinValidator_ProjectStartHookHasExecuted", false);
            }
            set
            {
                SessionState.SetBool("OdinValidator_ProjectStartHookHasExecuted", value);
            }
        }

        public string Name { get { return "On Project Startup"; } }

        public void Hook(Action run)
        {
            if (HookExecutedThisReload)
            {
                Debug.LogWarning("Action was hooked too late to be run as part of project startup. The project startup hook has a few frames of delay to allow for subscriptions to happen, but that time has passed now; you should hook during or as soon after InitializeOnLoad as you can. This message is fine if you just enabled the hook in the automation settings; it will execute next time the project opens.");
                return;
            }

            if (HookHasExecuted)
            {
                // Return silently in this case, since we don't want to spam people
                return;
            }

            if (!HookedEvents.Contains(run))
            {
                HookedEvents.Add(run);
            }
        }

        public void Unhook(Action run)
        {
            if (HookExecutedThisReload)
            {
                Debug.LogWarning("Action was unhooked too late, the hook has already run.");
                return;
            }

            HookedEvents.RemoveAll(n => n == run);
        }

        public void StopTriggeringEvent()
        {
            throw new ProjectStartupValidationFailedException();
        }
    }

    public class ProjectStartupValidationFailedException : Exception
    {
    }
#else

    public class OnProjectStartupValidationHook : IValidationHook
    {
        public string Name { get { return "On Project Startup"; } }

        public void Hook(Action run)
        {
            Debug.LogWarning("'On Project Startup' validation hook only works in Unity version 5.6 and up; event has not been hooked.");
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
