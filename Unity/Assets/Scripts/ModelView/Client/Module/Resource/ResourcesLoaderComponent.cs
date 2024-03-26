using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using YooAsset;

namespace ET.Client
{
    [EntitySystemOf(typeof(ResourcesLoaderComponent))]
    [FriendOf(typeof(ResourcesLoaderComponent))]
    public static partial class ResourcesLoaderComponentSystem
    {
        #region 生命周期
        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self)
        {
            self.package = YooAssets.GetPackage(ConstDefine.YooAssetDefaultPackageName);
        }

        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self, string packageName)
        {
            self.package = YooAssets.GetPackage(packageName);
        }

        [EntitySystem]
        private static void Destroy(this ResourcesLoaderComponent self)
        {
            OnRecycle(self);
        }

        
        /// <summary>
        /// 回收资源方法
        /// </summary>
        public static void OnRecycle(this ResourcesLoaderComponent self)
        {
            foreach (var kv in self.handlers)
            {
                switch (kv.Value)
                {
                    case AssetHandle handle:
                        handle.Release();
                        break;
                    case AllAssetsHandle handle:
                        handle.Release();
                        break;
                    case SubAssetsHandle handle:
                        handle.Release();
                        break;
                    case RawFileHandle handle:
                        handle.Release();
                        break;
                    case SceneHandle handle:
                        if (!handle.IsMainScene())
                        {
                            handle.UnloadAsync();
                        }
                        break;
                }
            }
            
            //清空列表
            self.handlers.Clear();
            //必要：上面循环完成将已创建出的资源引用清零，这里卸载引用计数为零的资源
            self.package.UnloadUnusedAssets();
        }
        #endregion

        #region 资源加载
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="location">
        /// 在未开启可寻址模式下，location代表的是资源对象的完整路径。
        /// 在开启可寻址模式下，location代表的是资源对象可寻址地址。
        /// 其它地方同理。
        /// </param>
        /// <returns></returns>
        public static async ETTask<T> LoadAssetAsync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadAssetAsync<T>(location);

                await handler.Task;

                self.handlers.Add(location, handler);
            }

            return (T)((AssetHandle)handler).AssetObject;
        }        
        
        /// <summary>
        /// 同步加载资源
        /// </summary>
        public static T LoadAssetSync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadAssetSync<T>(location);
                self.handlers.Add(location, handler);
            }
            return (T)((AssetHandle)handler).AssetObject;
        }

        /// <summary>
        /// 同步加载资源带回调
        /// </summary>
        public static void LoadAssetSync<T>(this ResourcesLoaderComponent self, string location,Action<T> callback) where T : UnityEngine.Object
        {
            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadAssetSync<T>(location);
                self.handlers.Add(location, handler);
            }
            callback?.Invoke((T)((AssetHandle)handler).AssetObject);
        }
        
        /// <summary>
        /// 同步直接创建完整资源对象
        /// </summary>
        public static GameObject CreateGameObjectSync(this ResourcesLoaderComponent self,string location, Transform trans = null)
        {
            var obj = self.LoadAssetSync<GameObject>(location);
            return UnityEngine.Object.Instantiate(obj, trans);
        }
        
        /// <summary>
        /// 异步直接创建完整资源对象
        /// </summary>
        public static async ETTask<GameObject> CreateGameObjectAsync(this ResourcesLoaderComponent self,string location, Transform trans = null)
        {
            var obj = await self.LoadAssetAsync<GameObject>(location);
            return UnityEngine.Object.Instantiate(obj, trans);
        }
        #endregion

        #region 子资源加载
        /// <summary>
        /// 异步加载子资源
        /// </summary>
        public static async ETTask<T> LoadSubAssetsAsync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadSubAssetsAsync<T>(location);

                await handler.Task;

                self.handlers.Add(location, handler);
            }

            return (T)((AssetHandle)handler).AssetObject;
        }        
        
        /// <summary>
        /// 同步加载子资源
        /// </summary>
        public static T LoadSubAssetsSync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadSubAssetsSync<T>(location);
                self.handlers.Add(location, handler);
            }
            return (T)((AssetHandle)handler).AssetObject;
        }

        /// <summary>
        /// 同步子加载资源带回调
        /// </summary>
        public static void LoadSubAssetsSync<T>(this ResourcesLoaderComponent self, string location,Action<T> callback) where T : UnityEngine.Object
        {
            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadSubAssetsSync<T>(location);
                self.handlers.Add(location, handler);
            }
            callback?.Invoke((T)((AssetHandle)handler).AssetObject);
        }
        #endregion

        #region 原生资源加载
        /// <summary>
        /// 异步加载原生资源
        /// </summary>
        public static async ETTask<T> LoadRawFileAsync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadRawFileAsync(location);

                await handler.Task;

                self.handlers.Add(location, handler);
            }

            return (T)((AssetHandle)handler).AssetObject;
        }        
        
        /// <summary>
        /// 同步加载原生资源
        /// </summary>
        public static T LoadRawFileSync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadRawFileSync(location);
                self.handlers.Add(location, handler);
            }
            return (T)((AssetHandle)handler).AssetObject;
        }

        /// <summary>
        /// 同步原生加载资源带回调
        /// </summary>
        public static void LoadRawFileSync(this ResourcesLoaderComponent self, string location,Action<UnityEngine.Object> callback) 
        {
            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadRawFileSync(location);
                self.handlers.Add(location, handler);
            }
            callback?.Invoke(((AssetHandle)handler).AssetObject);
        }
        #endregion

        /// <summary>
        /// 同步加载Sprite
        /// </summary>
        public static Sprite GetSpriteSync(this ResourcesLoaderComponent self,string atlasLocation, string spriteName)
        {
            Sprite sp = null;
            SpriteAtlas atlas = self.LoadAssetSync<SpriteAtlas>(atlasLocation);
            if (atlas != null)
            {
                sp = atlas.GetSprite(spriteName);
            }
            return sp;
        }
        
        /// <summary>
        /// 异步加载Sprite
        /// </summary>
        public static async ETTask<Sprite> GetSpriteAsync(this ResourcesLoaderComponent self,string atlasName, string spriteName)
        {
            Sprite sp = null;
            SpriteAtlas atlas = await self.LoadAssetAsync<SpriteAtlas>(atlasName);
            if (atlas != null)
            {
                sp = atlas.GetSprite(spriteName);
            }
            return sp;
        }

        #region 加载全部资源
        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        public static async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadAllAssetsAsync<T>(location);
                await handler.Task;
                self.handlers.Add(location, handler);
            }

            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (UnityEngine.Object assetObj in ((AllAssetsHandle)handler).AllAssetObjects)
            {
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }

            return dictionary;
        }
        #endregion

        #region 场景加载
        /// <summary>
        /// 异步加载场景
        /// </summary>
        public static async ETTask LoadSceneAsync(this ResourcesLoaderComponent self, string location, LoadSceneMode loadSceneMode)
        {
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (self.handlers.TryGetValue(location, out handler))
            {
                return;
            }

            handler = self.package.LoadSceneAsync(location);

            await handler.Task;
            self.handlers.Add(location, handler);
        }
        #endregion
    }

    /// <summary>
    /// 用来管理资源，生命周期跟随Parent，比如CurrentScene用到的资源应该用CurrentScene的ResourcesLoaderComponent来加载
    /// 这样CurrentScene释放后，它用到的所有资源都释放了
    /// </summary>
    [ComponentOf]
    public class ResourcesLoaderComponent : Entity, IAwake, IAwake<string>, IDestroy
    {
        public ResourcePackage package;
        public Dictionary<string, HandleBase> handlers = new();
    }
}