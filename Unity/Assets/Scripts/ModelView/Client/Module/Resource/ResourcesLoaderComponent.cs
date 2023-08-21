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
                kv.Value.Dispose();
            }
        }

        public static async ETTask<T> LoadAssetAsync<T>(this ResourcesLoaderComponent self, string location) where T: UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Fiber().CoroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());
            
            AssetOperationHandle assetOperationHandle;
            ResourceHandler handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                assetOperationHandle = self.package.LoadAssetAsync<T>(location);
            
                await assetOperationHandle.Task;

                handler = new ResourceHandler(assetOperationHandle);
                self.handlers.Add(location, handler);
            }
            else
            {
                assetOperationHandle = handler.GetHandler<AssetOperationHandle>();
            }
            
            return (T)assetOperationHandle.AssetObject;
        }
        
        public static async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(this ResourcesLoaderComponent self, string location) where T: UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Fiber().CoroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            AllAssetsOperationHandle allAssetsOperationHandle;
            ResourceHandler handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                allAssetsOperationHandle = self.package.LoadAllAssetsAsync<T>(location);
            
                await allAssetsOperationHandle.Task;

                handler = new ResourceHandler(allAssetsOperationHandle);
                self.handlers.Add(location, handler);
            }
            else
            {
                allAssetsOperationHandle = handler.GetHandler<AllAssetsOperationHandle>();
            }

            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach(UnityEngine.Object assetObj in allAssetsOperationHandle.AllAssetObjects)
            {    
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }
            return dictionary;
        }
        
        public static async ETTask LoadSceneAsync(this ResourcesLoaderComponent self, string location, LoadSceneMode loadSceneMode)
        {
            using CoroutineLock coroutineLock = await self.Fiber().CoroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            ResourceHandler handler;
            if (self.handlers.TryGetValue(location, out handler))
            {
                return;
            }

            SceneOperationHandle sceneOperationHandle = self.package.LoadSceneAsync(location);

            await sceneOperationHandle.Task;

            handler = new ResourceHandler(sceneOperationHandle);
            self.handlers.Add(location, handler);
        }
    }
    
    public struct ResourceHandler: IDisposable
    {
        private enum ResourceHandlerType
        {
            AssetOperationHandle,
            AllAssetsOperationHandle,
            RawFileOperationHandle,
            SubAssetsOperationHandle,
            SceneOperationHandle,
        }

        private readonly ResourceHandlerType type;

        private readonly OperationHandleBase handler;

        public ResourceHandler(AssetOperationHandle handler)
        {
            this.type = ResourceHandlerType.AssetOperationHandle;
            this.handler = handler;
        }
        
        public ResourceHandler(AllAssetsOperationHandle handler)
        {
            this.type = ResourceHandlerType.AllAssetsOperationHandle;
            this.handler = handler;
        }
        
        public ResourceHandler(RawFileOperationHandle handler)
        {
            this.type = ResourceHandlerType.RawFileOperationHandle;
            this.handler = handler;
        }
        
        public ResourceHandler(SubAssetsOperationHandle handler)
        {
            this.type = ResourceHandlerType.SubAssetsOperationHandle;
            this.handler = handler;
        }
        
        public ResourceHandler(SceneOperationHandle handler)
        {
            this.type = ResourceHandlerType.SceneOperationHandle;
            this.handler = handler;
        }


        public T GetHandler<T>() where T: OperationHandleBase
        {
            return this.handler as T;
        }

        public void Dispose()
        {
            switch (this.type)
            {
                case ResourceHandlerType.AssetOperationHandle:
                    ((AssetOperationHandle)handler).Release();
                    break;
                case ResourceHandlerType.AllAssetsOperationHandle:
                    ((AllAssetsOperationHandle)handler).Release();
                    break;
                case ResourceHandlerType.RawFileOperationHandle:
                    ((RawFileOperationHandle)handler).Release();
                    break;
                case ResourceHandlerType.SubAssetsOperationHandle:
                    ((SubAssetsOperationHandle)handler).Release();
                    break;
                case ResourceHandlerType.SceneOperationHandle:
                    ((SceneOperationHandle)handler).UnloadAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        public Dictionary<string, ResourceHandler> handlers = new();
    }
}