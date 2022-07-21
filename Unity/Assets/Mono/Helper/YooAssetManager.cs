using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using YooAsset;

namespace ET
{
    public class YooAssetManager : Singleton<YooAssetManager>
    {
        public async UniTask<T> LoadByAddressAsync<T>(string address) where T : UnityEngine.Object
        {
            var handle = YooAssets.LoadAssetSync<T>(address);
            await handle.Task;
            T result = handle.AssetObject as T;
            handle.Release();
            return result;
        }
        public async UniTask<byte[]> LoadRawFileBytesAsync(string address)
        {
            var handle = YooAssets.GetRawFileAsync(address);
            await handle.Task;
            byte[] result = handle.LoadFileData();
            return result;
        }
    }
}