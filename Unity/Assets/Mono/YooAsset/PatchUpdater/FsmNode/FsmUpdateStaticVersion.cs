using System.Collections;
using System.Collections.Generic;
using ET;
using UnityEngine;
using YooAsset;

internal class FsmUpdateStaticVersion : IFsmNode
{
    public string Name { private set; get; } = nameof(FsmUpdateStaticVersion);

    void IFsmNode.OnEnter()
    {
        PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.UpdateStaticVersion);
        GetStaticVersion().Coroutine();
    }

    void IFsmNode.OnUpdate()
    {
    }

    void IFsmNode.OnExit()
    {
    }

    private async ETTask GetStaticVersion()
    {
        ETTask etTask = ETTask.Create();
        // 更新资源版本号
        var operation = YooAssets.UpdateStaticVersionAsync(30);
        operation.Completed += _ => { etTask.SetResult(); };

        await etTask;

        if (operation.Status == EOperationStatus.Succeed)
        {
            Debug.Log($"Found static version : {operation.ResourceVersion}");
            PatchUpdater.ResourceVersion = operation.ResourceVersion;
            FsmManager.Transition(nameof(FsmUpdateManifest));
        }
        else
        {
            Debug.LogWarning(operation.Error);
            PatchEventDispatcher.SendStaticVersionUpdateFailedMsg();
        }
    }
}