using System;
using System.Collections.Generic;
using System.IO;
using YooAsset;

namespace ET
{
    public class YooAssetManager : Singleton<YooAssetManager>
    {
        public async ETTask<T> LoadByAddressAsync<T>(string address) where T : UnityEngine.Object
        {
            var handle = YooAssets.LoadAssetSync<T>(address);
            await handle.Task;
            T result = handle.AssetObject as T;
            handle.Release();
            return result;
        }
        public async ETTask<byte[]> LoadRawFileBytesAsync(string address)
        {
            var handle = YooAssets.GetRawFileAsync(address);
            await handle.Task;
            byte[] result = handle.LoadFileData();
            return result;
        }
    }
}