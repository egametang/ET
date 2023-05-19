using System.Collections;
using UnityEngine;
using YooAsset;

namespace ET
{
    public static class YooAssetHelper
    {
        public static ETTask GetAwaiter(this AsyncOperationBase asyncOperationBase)
        {
            ETTask task = ETTask.Create(true);
            asyncOperationBase.Completed += _ => { task.SetResult(); };
            return task;
        }
            
        public static ETTask GetAwaiter(this AssetOperationHandle assetOperationHandle)
        {
            ETTask task = ETTask.Create(true);
            assetOperationHandle.Completed += _ => { task.SetResult(); };
            return task;
        }
            
        public static ETTask GetAwaiter(this SubAssetsOperationHandle subAssetsOperationHandle)
        {
            ETTask task = ETTask.Create(true);
            subAssetsOperationHandle.Completed += _ => { task.SetResult(); };
            return task;
        }
            
        public static ETTask GetAwaiter(this SceneOperationHandle sceneOperationHandle)
        {
            ETTask task = ETTask.Create(true);
            sceneOperationHandle.Completed += _ => { task.SetResult(); };
            return task;
        }
        
        public static ETTask GetAwaiter(this RawFileOperationHandle assetOperationHandle)
        {
            ETTask task = ETTask.Create(true);
            assetOperationHandle.Completed += _ => { task.SetResult(); };
            return task;
        }
    }
}