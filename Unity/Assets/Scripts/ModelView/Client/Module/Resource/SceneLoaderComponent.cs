using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace ET.Client
{
    [EntitySystemOf(typeof(SceneLoaderComponent))]
    [FriendOf(typeof(SceneLoaderComponent))]
    public static partial class SceneLoaderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SceneLoaderComponent self)
        {
        }
        
        [EntitySystem]
        private static void Destroy(this SceneLoaderComponent self)
        {
            if (self.scenes == null)
            {
                return;
            }
            
            foreach (var kv in self.scenes)
            {
                Addressables.UnloadSceneAsync(kv.Value);
            }

            self.scenes = null;
        }

        public static bool TryGetValue(this SceneLoaderComponent self, string assetsName, out AsyncOperationHandle<SceneInstance> o)
        {
            return self.scenes.TryGetValue(assetsName, out o);
        }

        public static async ETTask LoadSceneAsync(this SceneLoaderComponent self, string assetsName)
        {
            if (self.scenes == null)
            {
                throw new Exception($"SceneLoaderComponent disposed, load scene fail: {assetsName}");
            }
            AsyncOperationHandle<SceneInstance> o = Addressables.LoadSceneAsync(assetsName);
            self.scenes[assetsName] = o;
            await o.Task;
        }
    }
    
    [ComponentOf]
    public class SceneLoaderComponent: Entity, IAwake, IDestroy
    {
        public Dictionary<string, AsyncOperationHandle<SceneInstance>> scenes = new();
    }
}