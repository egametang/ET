using System;
using UnityEngine;
using Object = UnityEngine.Object;
using ET;
using ET.Client;

namespace YIUIFramework
{
    /// <summary>
    /// 异步加载
    /// </summary>
    internal static partial class YIUILoadHelper
    {
        /// <summary>
        /// 异步加载资源对象
        /// 返回类型
        /// </summary>
        internal static async ETTask<T> LoadAssetAsync<T>(string pkgName, string resName) where T : Object
        {
            var load    = LoadHelper.GetLoad(pkgName, resName);
            var loadObj = load.Object;
            if (loadObj != null)
            {
                load.AddRefCount();
                return (T)loadObj;
            }

            if (load.WaitAsync)
            {
                await YIUIMgrComponent.Inst.Fiber().Root.GetComponent<TimerComponent>().WaitUntil(() => !load.WaitAsync);

                loadObj = load.Object;
                if (loadObj != null)
                {
                    load.AddRefCount();
                    return (T)loadObj;
                }
                else
                {
                    Debug.LogError($"错误这个时候不应该是null");
                    return null;
                }
            }

            load.SetWaitAsync(true);

            var (obj, hashCode) = await YIUILoadDI.LoadAssetAsyncFunc(pkgName, resName, typeof (T));
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
            return (T)obj;
        }

        /// <summary>
        /// 异步加载资源对象
        /// 回调类型
        /// </summary>
        internal static void LoadAssetAsync<T>(string pkgName, string resName, Action<T> action) where T : Object
        {
            LoadAssetAsyncAction(pkgName, resName, action).Coroutine();
        }

        private static async ETTask LoadAssetAsyncAction<T>(string pkgName, string resName, Action<T> action)
                where T : Object
        {
            var asset = await LoadAssetAsync<T>(pkgName, resName);
            if (asset == null)
            {
                Debug.LogError($"异步加载对象失败 {pkgName} {resName}");
                return;
            }

            action?.Invoke(asset);
        }
    }
}