using System.Collections;
using System.Collections.Generic;
using ET;
using UnityEngine;
using YooAsset;

public class FsmCreateDownloader : IFsmNode
{
	public string Name { private set; get; } = nameof(FsmCreateDownloader);

	void IFsmNode.OnEnter()
	{
		PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.CreateDownloader);
		CreateDownloader();
	}
	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}

	void CreateDownloader()
	{
		ETTask etTask = ETTask.Create();
		Debug.Log("创建补丁下载器.");
		int downloadingMaxNum = 10;
		int failedTryAgain = 3;
		PatchUpdater.Downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);
		if (PatchUpdater.Downloader.TotalDownloadCount == 0)
		{
			Debug.Log("没有发现需要下载的资源");
			FsmManager.Transition(nameof(FsmPatchDone));
		}
		else
		{
			Debug.Log($"一共发现了{PatchUpdater.Downloader.TotalDownloadCount}个资源需要更新下载。");

			// 发现新更新文件后，挂起流程系统
			// 注意：开发者需要在下载前检测磁盘空间不足
			int totalDownloadCount = PatchUpdater.Downloader.TotalDownloadCount;
			long totalDownloadBytes = PatchUpdater.Downloader.TotalDownloadBytes;
			PatchEventDispatcher.SendFoundUpdateFilesMsg(totalDownloadCount, totalDownloadBytes);
		}
	}
}