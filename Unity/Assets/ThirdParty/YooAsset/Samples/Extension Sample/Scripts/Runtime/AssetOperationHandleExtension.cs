using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public static class AssetOperationHandleExtension
{
    /*
    /// <summary>
    /// 获取资源对象
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    /// <param name="asset"></param>
    public static AssetOperationHandle GetAssetObject<TAsset>(this AssetOperationHandle thisHandle, out TAsset asset) where TAsset : UnityEngine.Object
    {
        if (thisHandle.Status != EOperationStatus.Succeed)
        {
            var assetInfo = thisHandle.GetAssetInfo();
            Debug.LogWarning($"The {assetInfo.AssetPath}[{assetInfo.AssetType}] is not success. Error[{thisHandle.LastError}]");
        }
        asset = thisHandle.AssetObject as TAsset;
        return thisHandle;
    }
    */

    /// <summary>
    /// 等待异步执行完毕
    /// </summary>
    public static AssetOperationHandle WaitForAsyncOperationComplete(this AssetOperationHandle thisHandle)
    {
        thisHandle.WaitForAsyncComplete();
        return thisHandle;
    }
}