using System.Collections;
using ET;
using YooAsset;

public class FsmDownloadWebFiles : IFsmNode
{
    public string Name { private set; get; } = nameof(FsmDownloadWebFiles);

    void IFsmNode.OnEnter()
    {
        PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.DownloadWebFiles);
        BeginDownload().Coroutine();
    }

    void IFsmNode.OnUpdate()
    {
    }

    void IFsmNode.OnExit()
    {
    }

    private async ETTask BeginDownload()
    {
        var downloader = PatchUpdater.Downloader;

        ETTask etTask = ETTask.Create();

        // 注册下载回调
        downloader.OnDownloadErrorCallback = PatchEventDispatcher.SendWebFileDownloadFailedMsg;
        downloader.OnDownloadProgressCallback = PatchEventDispatcher.SendDownloadProgressUpdateMsg;
        downloader.BeginDownload();
        
        downloader.Completed += _ => { etTask.SetResult(); };
        
        await etTask;

        // 检测下载结果
        if (downloader.Status != EOperationStatus.Succeed)
            return;

        FsmManager.Transition(nameof(FsmPatchDone));
    }
}