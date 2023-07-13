using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Window;

[WindowAttribute(100, false)]
public class UIHomeWindow : UIWindow
{
	private Text _version;

	public override void OnCreate()
	{
		_version = this.transform.Find("version").GetComponent<Text>();

		var loginBtn = this.transform.Find("Start").GetComponent<Button>();
		loginBtn.onClick.AddListener(OnClickLoginBtn);

		var aboutBtn = this.transform.Find("About").GetComponent<Button>();
		aboutBtn.onClick.AddListener(OnClicAboutBtn);
	}
	public override void OnDestroy()
	{
	}
	public override void OnRefresh()
	{
		var package = YooAsset.YooAssets.GetPackage("DefaultPackage");
		_version.text = "Ver : " + package.GetPackageVersion();
	}
	public override void OnUpdate()
	{
	}

	private void OnClickLoginBtn()
	{
		SceneEventDefine.ChangeToBattleScene.SendEventMessage();
	}
	private void OnClicAboutBtn()
	{
		UniWindow.OpenWindowAsync<UIAboutWindow>("UIAbout");
	}
}