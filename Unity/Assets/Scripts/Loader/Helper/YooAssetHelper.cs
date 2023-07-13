using System;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace ET
{
    public static class YooAssetHelper
    {
        // 有了这个方法，就可以直接await Unity的AssetOperationHandle了
        public static async ETTask GetAwaiter(this AssetOperationHandle asyncOperation)
        {
            ETTask task = ETTask.Create(true);
            asyncOperation.Completed += _ => { task.SetResult(); };
            await task;
        }
        
        public static async ETTask GetAwaiter(this SceneOperationHandle asyncOperation)
        {
            ETTask task = ETTask.Create(true);
            asyncOperation.Completed += _ => { task.SetResult(); };
            await task;
        }
        
    }
}