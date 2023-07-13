using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Window;
using YooAsset;

[WindowAttribute(100, false)]
public class UIAboutWindow : UIWindow
{
	private Text _info;
	private RawFileOperationHandle _handle;

	public override void OnCreate()
	{
		var maskBtn = this.transform.Find("mask").GetComponent<Button>();
		maskBtn.onClick.AddListener(OnClicMaskBtn);

		_info = this.transform.Find("bg/info").GetComponent<Text>();
		_handle = YooAssets.LoadRawFileAsync("about");
		_handle.Completed += _handle_Completed;
	}
	public override void OnDestroy()
	{
		_handle.Release();
	}
	public override void OnRefresh()
	{
	}
	public override void OnUpdate()
	{
	}

	private void _handle_Completed(RawFileOperationHandle obj)
	{
		_info.text = _handle.GetRawFileText();
	}

	private void OnClicMaskBtn()
	{
		UniWindow.CloseWindow<UIAboutWindow>();
	}
}