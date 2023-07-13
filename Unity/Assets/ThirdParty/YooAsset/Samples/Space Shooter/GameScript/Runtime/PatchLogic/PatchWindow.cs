using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Event;

public class PatchWindow : MonoBehaviour
{
	/// <summary>
	/// 对话框封装类
	/// </summary>
	private class MessageBox
	{
		private GameObject _cloneObject;
		private Text _content;
		private Button _btnOK;
		private System.Action _clickOK;

		public bool ActiveSelf
		{
			get
			{
				return _cloneObject.activeSelf;
			}
		}

		public void Create(GameObject cloneObject)
		{
			_cloneObject = cloneObject;
			_content = cloneObject.transform.Find("txt_content").GetComponent<Text>();
			_btnOK = cloneObject.transform.Find("btn_ok").GetComponent<Button>();
			_btnOK.onClick.AddListener(OnClickYes);
		}
		public void Show(string content, System.Action clickOK)
		{
			_content.text = content;
			_clickOK = clickOK;
			_cloneObject.SetActive(true);
			_cloneObject.transform.SetAsLastSibling();
		}
		public void Hide()
		{
			_content.text = string.Empty;
			_clickOK = null;
			_cloneObject.SetActive(false);
		}
		private void OnClickYes()
		{
			_clickOK?.Invoke();
			Hide();
		}
	}


	private readonly EventGroup _eventGroup = new EventGroup();
	private readonly List<MessageBox> _msgBoxList = new List<MessageBox>();

	// UGUI相关
	private GameObject _messageBoxObj;
	private Slider _slider;
	private Text _tips;


	void Awake()
	{
		_slider = transform.Find("UIWindow/Slider").GetComponent<Slider>();
		_tips = transform.Find("UIWindow/Slider/txt_tips").GetComponent<Text>();
		_tips.text = "Initializing the game world !";
		_messageBoxObj = transform.Find("UIWindow/MessgeBox").gameObject;
		_messageBoxObj.SetActive(false);

		_eventGroup.AddListener<PatchEventDefine.InitializeFailed>(OnHandleEventMessage);
		_eventGroup.AddListener<PatchEventDefine.PatchStatesChange>(OnHandleEventMessage);
		_eventGroup.AddListener<PatchEventDefine.FoundUpdateFiles>(OnHandleEventMessage);
		_eventGroup.AddListener<PatchEventDefine.DownloadProgressUpdate>(OnHandleEventMessage);
		_eventGroup.AddListener<PatchEventDefine.PackageVersionUpdateFailed>(OnHandleEventMessage);
		_eventGroup.AddListener<PatchEventDefine.PatchManifestUpdateFailed>(OnHandleEventMessage);
		_eventGroup.AddListener<PatchEventDefine.WebFileDownloadFailed>(OnHandleEventMessage);
	}
	void OnDestroy()
	{
		_eventGroup.RemoveAllListener();
	}

	/// <summary>
	/// 接收事件
	/// </summary>
	private void OnHandleEventMessage(IEventMessage message)
	{
		if (message is PatchEventDefine.InitializeFailed)
		{
			System.Action callback = () =>
			{
				UserEventDefine.UserTryInitialize.SendEventMessage();
			};
			ShowMessageBox($"Failed to initialize package !", callback);
		}
		else if (message is PatchEventDefine.PatchStatesChange)
		{
			var msg = message as PatchEventDefine.PatchStatesChange;
			_tips.text = msg.Tips;
		}
		else if (message is PatchEventDefine.FoundUpdateFiles)
		{
			var msg = message as PatchEventDefine.FoundUpdateFiles;
			System.Action callback = () =>
			{
				UserEventDefine.UserBeginDownloadWebFiles.SendEventMessage();
			};
			float sizeMB = msg.TotalSizeBytes / 1048576f;
			sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
			string totalSizeMB = sizeMB.ToString("f1");
			ShowMessageBox($"Found update patch files, Total count {msg.TotalCount} Total szie {totalSizeMB}MB", callback);
		}
		else if (message is PatchEventDefine.DownloadProgressUpdate)
		{
			var msg = message as PatchEventDefine.DownloadProgressUpdate;
			_slider.value = (float)msg.CurrentDownloadCount / msg.TotalDownloadCount;
			string currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
			string totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
			_tips.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
		}
		else if (message is PatchEventDefine.PackageVersionUpdateFailed)
		{
			System.Action callback = () =>
			{
				UserEventDefine.UserTryUpdatePackageVersion.SendEventMessage();
			};
			ShowMessageBox($"Failed to update static version, please check the network status.", callback);
		}
		else if (message is PatchEventDefine.PatchManifestUpdateFailed)
		{
			System.Action callback = () =>
			{
				UserEventDefine.UserTryUpdatePatchManifest.SendEventMessage();
			};
			ShowMessageBox($"Failed to update patch manifest, please check the network status.", callback);
		}
		else if (message is PatchEventDefine.WebFileDownloadFailed)
		{
			var msg = message as PatchEventDefine.WebFileDownloadFailed;
			System.Action callback = () =>
			{
				UserEventDefine.UserTryDownloadWebFiles.SendEventMessage();
			};
			ShowMessageBox($"Failed to download file : {msg.FileName}", callback);
		}
		else
		{
			throw new System.NotImplementedException($"{message.GetType()}");
		}
	}

	/// <summary>
	/// 显示对话框
	/// </summary>
	private void ShowMessageBox(string content, System.Action ok)
	{
		// 尝试获取一个可用的对话框
		MessageBox msgBox = null;
		for (int i = 0; i < _msgBoxList.Count; i++)
		{
			var item = _msgBoxList[i];
			if (item.ActiveSelf == false)
			{
				msgBox = item;
				break;
			}
		}

		// 如果没有可用的对话框，则创建一个新的对话框
		if (msgBox == null)
		{
			msgBox = new MessageBox();
			var cloneObject = GameObject.Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
			msgBox.Create(cloneObject);
			_msgBoxList.Add(msgBox);
		}

		// 显示对话框
		msgBox.Show(content, ok);
	}
}