﻿using System.Collections.Generic;
using System.IO;

namespace ET
{
    public static class AssetsBundleHelper
    {
        public static Dictionary<string, UnityEngine.Object> LoadBundle(string assetBundleName)
        {
            assetBundleName = assetBundleName.ToLower();

            Dictionary<string, UnityEngine.Object> objects = new Dictionary<string, UnityEngine.Object>();
            if (!Define.IsAsync)
            {
                if (Define.IsEditor)
                {
                    string[] realPath = null;
                    realPath = Define.GetAssetPathsFromAssetBundle(assetBundleName);
                    foreach (string s in realPath)
                    {
                        //string assetName = Path.GetFileNameWithoutExtension(s);
                        UnityEngine.Object resource = Define.LoadAssetAtPath(s);
                        objects.Add(resource.name, resource);
                    }
                }
                return objects;
            }

            string p = Path.Combine(PathHelper.AppHotfixResPath, assetBundleName);
            UnityEngine.AssetBundle assetBundle = null;
            if (File.Exists(p))
            {
                assetBundle = UnityEngine.AssetBundle.LoadFromFile(p);
            }
            else
            {
                p = Path.Combine(PathHelper.AppResPath, assetBundleName);
                assetBundle = UnityEngine.AssetBundle.LoadFromFile(p);
            }

            if (assetBundle == null)
            {
                // 获取资源的时候会抛异常，这个地方不直接抛异常，因为有些地方需要Load之后判断是否Load成功
                UnityEngine.Debug.LogWarning($"assets bundle not found: {assetBundleName}");
                return objects;
            }

            UnityEngine.Object[] assets = assetBundle.LoadAllAssets();
            foreach (UnityEngine.Object asset in assets)
            {
                objects.Add(asset.name, asset);
            }
            assetBundle.Unload(false);
            return objects;
        }
    }
}