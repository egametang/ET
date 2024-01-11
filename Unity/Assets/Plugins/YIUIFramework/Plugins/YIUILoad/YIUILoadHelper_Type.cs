using System;
using UnityEngine;
using Object = UnityEngine.Object;
using ET;
using ET.Client;

namespace YIUIFramework
{
    /// <summary>
    /// 不使用泛型 使用type加载的方式
    /// </summary>
    internal static partial class YIUILoadHelper
    {
        internal static Object LoadAsset(string pkgName, string resName, Type assetType)
        {
            var load    = LoadHelper.GetLoad(pkgName, resName);
            var loadObj = load.Object;
            if (loadObj != null)
            {
                load.AddRefCount();
                return loadObj;
            }

            var (obj, hashCode) = YIUILoadDI.LoadAssetFunc(pkgName, resName, assetType);
            if (obj == null)
            {
                load.RemoveRefCount();
                return null;
            }

            if (!LoadHelper.AddLoadHandle(obj, load))
            {
                return null;
            }

            load.ResetHandle(obj, hashCode);
            load.AddRefCount();
            return obj;
        }

        internal static async ETTask<Object> LoadAssetAsync(string pkgName, string resName, Type assetType)
        {
            var load    = LoadHelper.GetLoad(pkgName, resName);
            var loadObj = load.Object;
            if (loadObj != null)
            {
                load.AddRefCount();
                return loadObj;
            }

            if (load.WaitAsync)
            {
                await YIUIMgrComponent.Inst.Fiber().Root.GetComponent<TimerComponent>().WaitUntil(() => !load.WaitAsync);

                loadObj = load.Object;
                if (loadObj != null)
                {
                    load.AddRefCount();
                    return loadObj;
                }
                else
                {
                    Debug.LogError($"错误这个时候不应该是null");
                }
            }

            load.SetWaitAsync(true);

            var (obj, hashCode) = await YIUILoadDI.LoadAssetAsyncFunc(pkgName, resName, assetType);
            if (obj == null)
            {
                load.RemoveRefCount();
                return null;
            }

            if (!LoadHelper.AddLoadHandle(obj, load))
            {
                return null;
            }

            load.ResetHandle(obj, hashCode);
            load.AddRefCount();
            load.SetWaitAsync(false);
            return obj;
        }

        internal static void LoadAssetAsync(string pkgName, string resName, Type assetType, Action<Object> action)
        {
            LoadAssetAsyncAction(pkgName, resName, assetType, action).Coroutine();
        }

        private static async ETTask LoadAssetAsyncAction(
        string         pkgName,
        string         resName,
        Type           assetType,
        Action<Object> action)
        {
            var asset = await LoadAssetAsync(pkgName, resName, assetType);
            if (asset == null)
            {
                Debug.LogError($"异步加载对象失败 {pkgName} {resName}");
                return;
            }

            action?.Invoke(asset);
        }
    }
}