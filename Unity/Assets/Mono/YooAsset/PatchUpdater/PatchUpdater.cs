using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using ET;

public static class PatchUpdater
{
	private static bool _isRun = false;

	/// <summary>
	/// 下载器
	/// </summary>
	public static PatchDownloaderOperation Downloader { set; get; }

	/// <summary>
	/// 资源版本
	/// </summary>
	public static int ResourceVersion { set; get; }

	private static readonly EventGroup _eventGroup = new EventGroup();

	private static Action s_PatchUpdateCompleted;
	
	/// <summary>
	/// 开启初始化流程
	/// </summary>
	public static void Run(Action patchUpdateCompleted)
	{
		if (_isRun == false)
		{
			_isRun = true;
			
			s_PatchUpdateCompleted = patchUpdateCompleted;
			
			_eventGroup.AddListener<PatchEventMessageDefine.PatchStatesChange>(OnHandleEvent);
			_eventGroup.AddListener<PatchEventMessageDefine.FoundUpdateFiles>(OnHandleEvent);
			_eventGroup.AddListener<PatchEventMessageDefine.DownloadProgressUpdate>(OnHandleEvent);
			_eventGroup.AddListener<PatchEventMessageDefine.StaticVersionUpdateFailed>(OnHandleEvent);
			_eventGroup.AddListener<PatchEventMessageDefine.PatchManifestUpdateFailed>(OnHandleEvent);
			_eventGroup.AddListener<PatchEventMessageDefine.WebFileDownloadFailed>(OnHandleEvent);

			// 注意：按照先后顺序添加流程节点
			FsmManager.AddNode(new FsmPatchInit());
			FsmManager.AddNode(new FsmUpdateStaticVersion());
			FsmManager.AddNode(new FsmUpdateManifest());
			FsmManager.AddNode(new FsmCreateDownloader());
			FsmManager.AddNode(new FsmDownloadWebFiles());
			FsmManager.AddNode(new FsmPatchDone());
			
			FsmManager.Run(nameof(FsmPatchInit));
		}
		else
		{
			Debug.LogWarning("补丁更新已经正在进行中!");
		}
	}

	/// <summary>
	/// 处理请求操作
	/// </summary>
	private static void HandleOperation(EPatchOperation operation)
	{
		if (operation == EPatchOperation.BeginDownloadWebFiles)
		{
			FsmManager.Transition(nameof(FsmDownloadWebFiles));
		}
		else if(operation == EPatchOperation.TryUpdateStaticVersion)
		{
			FsmManager.Transition(nameof(FsmUpdateStaticVersion));
		}
		else if (operation == EPatchOperation.TryUpdatePatchManifest)
		{
			FsmManager.Transition(nameof(FsmUpdateManifest));
		}
		else if (operation == EPatchOperation.TryDownloadWebFiles)
		{
			FsmManager.Transition(nameof(FsmCreateDownloader));
		}
		else
		{
			throw new NotImplementedException($"{operation}");
		}
	}
	
	    /// <summary>
    /// 接收事件
    /// </summary>
    private static void OnHandleEvent(IEventMessage msg)
    {
        if (msg is PatchEventMessageDefine.PatchStatesChange)
        {
            var message = msg as PatchEventMessageDefine.PatchStatesChange;
            if (message.CurrentStates == EPatchStates.UpdateStaticVersion)
                Log.Info("Update static version.");
            else if (message.CurrentStates == EPatchStates.UpdateManifest)
                Log.Info("Update patch manifest.");
            else if (message.CurrentStates == EPatchStates.CreateDownloader)
                Log.Info("Check download contents.");
            else if (message.CurrentStates == EPatchStates.DownloadWebFiles)
                Log.Info("Downloading patch files.");
            else if (message.CurrentStates == EPatchStates.PatchDone)
            {
	            s_PatchUpdateCompleted?.Invoke();
            }
            else
                throw new NotImplementedException(message.CurrentStates.ToString());
        }
        else if (msg is PatchEventMessageDefine.FoundUpdateFiles)
        {
            var message = msg as PatchEventMessageDefine.FoundUpdateFiles;

            float sizeMB = message.TotalSizeBytes / 1048576f;
            sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
            string totalSizeMB = sizeMB.ToString("f1");
            ET.Log.Info($"Found update patch files, Total count {message.TotalCount} Total szie {totalSizeMB}MB");
            
            PatchUpdater.HandleOperation(EPatchOperation.BeginDownloadWebFiles); 
        }
        else if (msg is PatchEventMessageDefine.DownloadProgressUpdate)
        {
            var message = msg as PatchEventMessageDefine.DownloadProgressUpdate;
            string currentSizeMB = (message.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
            string totalSizeMB = (message.TotalDownloadSizeBytes / 1048576f).ToString("f1");
            string text =
                $"{message.CurrentDownloadCount}/{message.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
        }
        else if (msg is PatchEventMessageDefine.StaticVersionUpdateFailed)
        {
            System.Action callback = () => { PatchUpdater.HandleOperation(EPatchOperation.TryUpdateStaticVersion); };
            ET.Log.Info($"Failed to update static version, please check the network status.", callback);
        }
        else if (msg is PatchEventMessageDefine.PatchManifestUpdateFailed)
        {
            System.Action callback = () => { PatchUpdater.HandleOperation(EPatchOperation.TryUpdatePatchManifest); };
            ET.Log.Info($"Failed to update patch manifest, please check the network status.", callback);
        }
        else if (msg is PatchEventMessageDefine.WebFileDownloadFailed)
        {
            var message = msg as PatchEventMessageDefine.WebFileDownloadFailed;
            System.Action callback = () => { PatchUpdater.HandleOperation(EPatchOperation.TryDownloadWebFiles); };
            ET.Log.Info($"Failed to download file : {message.FileName}", callback);
        }
        else
        {
            throw new System.NotImplementedException($"{msg.GetType()}");
        }
    }
}