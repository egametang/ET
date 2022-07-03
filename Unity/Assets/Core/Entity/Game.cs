using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public static class Game
    {
        public static ThreadSynchronizationContext ThreadSynchronizationContext => ThreadSynchronizationContext.Instance;

        public static TimeInfo TimeInfo => TimeInfo.Instance;
        
        public static EventSystem EventSystem => EventSystem.Instance;

        private static Scene scene;
        public static Scene Scene
        {
            get
            {
                if (scene != null)
                {
                    return scene;
                }
#if UNITY_EDITOR
                var editorAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Unity.Editor");
                if (!UnityEngine.Application.isPlaying)
                {
                    editorAssembly.GetType("ET.EditorEntityLauncher").GetMethod("InitEditorEnv").Invoke(null, null);
                }
#endif
                scene = EntitySceneFactory.CreateScene(0, SceneType.Process, "Process");
#if UNITY_EDITOR
                if (!UnityEngine.Application.isPlaying)
                {
                    editorAssembly.GetType("ET.EditorEntityLauncher").GetMethod("InitEventSystemInEditorMode").Invoke(null, new object[] { scene });
                }
#endif
                return scene;
            }
        }

        public static ObjectPool ObjectPool => ObjectPool.Instance;

        public static IdGenerater IdGenerater => IdGenerater.Instance;

        public static Options Options => Options.Instance;

        public static List<Action> FrameFinishCallback = new List<Action>();

        public static ILog ILog { get; set; }

        public static void Update()
        {
            ThreadSynchronizationContext.Update();
            TimeInfo.Update();
            EventSystem.Update();
        }
        
        public static void LateUpdate()
        {
            EventSystem.LateUpdate();
        }

        public static void FrameFinish()
        {
            foreach (Action action in FrameFinishCallback)
            {
                action.Invoke();
            }
            FrameFinishCallback.Clear();
        }

        public static void Close()
        {
            scene?.Dispose();
            scene = null;
            MonoPool.Instance.Dispose();
            EventSystem.Instance.Dispose();
            IdGenerater.Instance.Dispose();
        }
    }
}