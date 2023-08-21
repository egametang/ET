using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using YooAsset;

namespace ET.Client
{
    [EntitySystemOf(typeof(ResourcesLoaderComponent))]
    [FriendOf(typeof(ResourcesLoaderComponent))]
    public static partial class ResourcesLoaderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self)
        {
            self.package = YooAssets.GetPackage("DefaultPackage");
        }
        
        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self, string packageName)
        {
            self.package = YooAssets.GetPackage(packageName);
        }
        
        [EntitySystem]
        private static void Destroy(this ResourcesLoaderComponent self)
        {
            foreach (var kv in self.handlers)
            {
                switch (kv.Value)
                {
                    case AssetOperationHandle handle:
                        handle.Release();
                        break;
                    case AllAssetsOperationHandle handle:
                        handle.Release();
                        break;
                    case SubAssetsOperationHandle handle:
                        handle.Release();
                        break;
                    case RawFileOperationHandle handle:
                        handle.Release();
                        break;
                    case SceneOperationHandle handle:
                        if (!handle.IsMainScene())
                        {
                            handle.UnloadAsync();
                        }
                        break;
                }
            }
        }

        public static async ETTask<T> LoadAssetAsync<T>(this ResourcesLoaderComponent self, string location) where T: UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Fiber().CoroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());
            
            OperationHandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadAssetAsync<T>(location);
            
                await handler.Task;

                self.handlers.Add(location, handler);
            }
            
            return (T)((AssetOperationHandle)handler).AssetObject;
        }
        
        public static async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(this ResourcesLoaderComponent self, string location) where T: UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Fiber().CoroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            OperationHandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadAllAssetsAsync<T>(location);
            
                await handler.Task;
                self.handlers.Add(location, handler);
            }

            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach(UnityEngine.Object assetObj in ((AllAssetsOperationHandle)handler).AllAssetObjects)
            {    
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }
            return dictionary;
        }
        
        public static async ETTask LoadSceneAsync(this ResourcesLoaderComponent self, string location, LoadSceneMode loadSceneMode)
        {
            using CoroutineLock coroutineLock = await self.Fiber().CoroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            OperationHandleBase handler;
            if (self.handlers.TryGetValue(location, out handler))
            {
                return;
            }

            handler = self.package.LoadSceneAsync(location);

            await handler.Task;
            self.handlers.Add(location, handler);
        }
    }
    
    /// <summary>
    /// 用来管理资源，生命周期跟随Parent，比如CurrentScene用到的资源应该用CurrentScene的ResourcesLoaderComponent来加载
    /// 这样CurrentScene释放后，它用到的所有资源都释放了
    /// </summary>
    [ComponentOf]
    public class ResourcesLoaderComponent: Entity, IAwake, IAwake<string>, IDestroy
    {
        public ResourcePackage package;
        public Dictionary<string, OperationHandleBase> handlers = new();
    }
}