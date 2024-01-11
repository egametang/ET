using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using ET;

namespace YIUIFramework
{
    /// <summary>
    /// UI用 加载器
    /// 扩展 GameObject快捷方法 需成对使用
    /// </summary>
    internal static partial class YIUILoadHelper
    {
        private static Dictionary<Object, Object> g_ObjectMap = new Dictionary<Object, Object>();

        /// <summary>
        /// 同步加载 并实例化
        /// </summary>
        internal static GameObject LoadAssetInstantiate(string pkgName, string resName)
        {
            var asset = LoadAsset<GameObject>(pkgName, resName);
            if (asset == null) return null;
            var obj = Object.Instantiate(asset);
            g_ObjectMap.Add(obj, asset);
            return obj;
        }

        /// <summary>
        /// 异步加载 并实例化
        /// </summary>
        internal static async ETTask<GameObject> LoadAssetAsyncInstantiate(string pkgName, string resName)
        {
            var asset = await LoadAssetAsync<GameObject>(pkgName, resName);
            if (asset == null) return null;
            var obj = Object.Instantiate(asset);
            g_ObjectMap.Add(obj, asset);
            return obj;
        }

        /// <summary>
        /// 异步加载资源对象
        /// 回调类型
        /// </summary>
        internal static async void LoadAssetAsyncInstantiate(string pkgName, string resName, Action<Object> action)
        {
            var obj = await LoadAssetAsyncInstantiate(pkgName, resName);
            if (obj == null)
            {
                Debug.LogError($"异步加载对象失败 {pkgName} {resName}");
                return;
            }

            action?.Invoke(obj);
        }

        /// <summary>
        /// 释放由 实例化出来的GameObject
        /// </summary>
        internal static void ReleaseInstantiate(Object gameObject)
        {
            if (!g_ObjectMap.TryGetValue(gameObject, out var asset)) return;
            g_ObjectMap.Remove(gameObject);
            Release(asset);
        }
    }
}