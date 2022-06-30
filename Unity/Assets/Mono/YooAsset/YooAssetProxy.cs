using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using YooAsset;

namespace ET
{
    public static class YooAssetProxy
    {
        #region Extension

        public static T GetAsset<T>(this AssetOperationHandle assetOperationHandle)
            where T : UnityEngine.Object
        {
            return assetOperationHandle.AssetObject as T;
        }

        public static T GetSubAsset<T>(this SubAssetsOperationHandle assetOperationHandle, string subAssetName)
            where T : UnityEngine.Object
        {
            return assetOperationHandle.GetSubAssetObject<T>(subAssetName);
        }

        public static byte[] GetRawBytes(this RawFileOperation rawFileOperation)
        {
            return rawFileOperation.LoadFileData();
        }

        public static string GetRawString(this RawFileOperation rawFileOperation)
        {
            return rawFileOperation.LoadFileText();
        }

        #endregion

        #region API

        public static ETTask<AssetOperationHandle> LoadAssetAsync<T>(string path) where T : UnityEngine.Object
        {
            ETTask<AssetOperationHandle> result = ETTask<AssetOperationHandle>.Create();
            AssetOperationHandle assetOperationHandle = YooAssets.LoadAssetAsync<T>(path);
            assetOperationHandle.Completed += handle => { result.SetResult(handle); };
            return result;
        }

        public static ETTask<SubAssetsOperationHandle> LoadSubAssetsAsync<T>(string mainAssetPath, string subAssetName)
            where T : UnityEngine.Object
        {
            ETTask<SubAssetsOperationHandle> result = ETTask<SubAssetsOperationHandle>.Create();
            SubAssetsOperationHandle subAssetsOperationHandle = YooAssets.LoadSubAssetsAsync<T>(mainAssetPath);
            subAssetsOperationHandle.Completed += handle => { result.SetResult(handle); };
            return result;
        }

        public static ETTask<SceneOperationHandle> LoadSceneAsync(string scenePath,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            ETTask<SceneOperationHandle> result = ETTask<SceneOperationHandle>.Create();
            SceneOperationHandle sceneOperationHandle = YooAssets.LoadSceneAsync(scenePath, loadSceneMode, true);
            sceneOperationHandle.Completed += handle => { result.SetResult(sceneOperationHandle); };
            return result;
        }

        public static ETTask<byte[]> GetRawFileAsync(string path)
        {
            ETTask<byte[]> result = ETTask<byte[]>.Create();
            RawFileOperation rawFileOperation = YooAssets.GetRawFileAsync(path);
            rawFileOperation.Completed += handle => { result.SetResult(rawFileOperation.LoadFileData()); };
            return result;
        }

        public static void UnloadUnusedAssets()
        {
            YooAssets.UnloadUnusedAssets();
        }

        public static List<string> GetAssetPathsByTag(string tag)
        {
            AssetInfo[] assetInfos = YooAssets.GetAssetInfos(tag);
            List<string> result = new List<string>(16);
            foreach (var assetInfo in assetInfos)
            {
                result.Add(assetInfo.Address);
            }

            return result;
        }

        public static void StartYooAssetEngine(YooAssets.EPlayMode playMode, Action initCompletedCallback)
        {
            // 编辑器下的模拟模式
            if (playMode == YooAssets.EPlayMode.EditorSimulateMode)
            {
                var createParameters = new YooAssets.EditorSimulateModeParameters();
                createParameters.LocationServices = new AddressLocationServices();
                YooAssets.InitializeAsync(createParameters).Completed += _ => { initCompletedCallback?.Invoke(); };
            }

            // 单机运行模式
            if (playMode == YooAssets.EPlayMode.OfflinePlayMode)
            {
                var createParameters = new YooAssets.OfflinePlayModeParameters();
                createParameters.LocationServices = new AddressLocationServices();
                YooAssets.InitializeAsync(createParameters).Completed += _ => { initCompletedCallback?.Invoke(); };
            }

            // 联机运行模式
            if (playMode == YooAssets.EPlayMode.HostPlayMode)
            {
                var createParameters = new YooAssets.HostPlayModeParameters();
                createParameters.LocationServices = new AddressLocationServices();
                createParameters.DecryptionServices = null;
                createParameters.ClearCacheWhenDirty = false;
                createParameters.DefaultHostServer = GetHostServerURL();
                createParameters.FallbackHostServer = GetHostServerURL();

                string GetHostServerURL()
                {
                    string hostServerIP = "http://127.0.0.1:8088";
                    string gameVersion = "1";
                    return $"{hostServerIP}/StandaloneWindows64/{gameVersion}";
                }

                // 如果是资源热更模式，则需要等待热更完毕后再Invoke回调
                YooAssets.InitializeAsync(createParameters).Completed += _ =>
                {
                    // 运行补丁流程
                    PatchUpdater.Run(initCompletedCallback);
                };
            }
        }

        #endregion
    }
}