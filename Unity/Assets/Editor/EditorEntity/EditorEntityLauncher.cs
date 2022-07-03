using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class EditorEntityLauncher
    {
        [MenuItem("Test/InitEditorEnv")]
        public static void InitEditorEnv()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Log.Error(e.ExceptionObject.ToString());
            };
            System.Threading.SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
            ETTask.ExceptionHandler = Log.Error;
            Game.ILog = new UnityLogger();
            Options.Instance = new Options();
        }

        public static void InitEventSystemInEditorMode(Scene gameScene)
        {
            List<Assembly> assemblies = new() { typeof(EditorEntityLauncher).Assembly };
            foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (item.GetName().Name is "Code")
                {
                    assemblies.Add(item);
                }
            }
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies());
            Game.EventSystem.Add(types);
            MongoHelper.Register(Game.EventSystem.GetTypes());
            EditorApplication.update += () =>
            {
                Game.Update();
                Game.LateUpdate();
                Game.FrameFinish();
            };
            gameScene.AddComponent<TimerComponent>();
        }
    }
}