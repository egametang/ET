using System.Collections;
using System.Collections.Generic;
using ET;
using UnityEngine;
using YooAsset;

public class FsmUpdateManifest : IFsmNode
{
    public string Name { private set; get; } = nameof(FsmUpdateManifest);

    void IFsmNode.OnEnter()
    {
        PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.UpdateManifest);
        UpdateManifest().Coroutine();
    }

    void IFsmNode.OnUpdate()
    {
    }

    void IFsmNode.OnExit()
    {
    }

    private async ETTask UpdateManifest()
    {
        // 更新补丁清单
        ETTask etTask = ETTask.Create();
        var operation = YooAssets.UpdateManifestAsync(PatchUpdater.ResourceVersion, 30);
        operation.Completed += _ => { etTask.SetResult(); };
        
        await etTask;

        if (operation.Status == EOperationStatus.Succeed)
        {
            FsmManager.Transition(nameof(FsmCreateDownloader));
        }
        else
        {
            Debug.LogWarning(operation.Error);
            PatchEventDispatcher.SendPatchManifestUpdateFailedMsg();
        }
    }
}