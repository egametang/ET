using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using YooAsset;

namespace ET
{
    public class YooAssetManager : Singleton<YooAssetManager>
    {
        public async UniTask<T> Load<T>(string location) where T : UnityEngine.Object
        {
            var handle = YooAssets.LoadAssetSync<T>(location);
            await handle.Task;
            T result = handle.AssetObject as T;
            handle.Release();
            return result;
        }
        public async UniTask<byte[]> LoadRawFileBytesAsync(string location)
        {
            var handle = YooAssets.GetRawFileAsync(location);
            await handle.Task;
            byte[] result = handle.LoadFileData();
            return result;
        }
        public async UniTask LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            await YooAssets.LoadSceneAsync(location, sceneMode, activateOnLoad, priority);
        }
    }
}