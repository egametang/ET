using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Window;
using UniFramework.Utility;

[WindowAttribute(1000, true)]
public class UILoadingWindow : UIWindow
{
	private Text _info;
	private int _countdown;
	private UniTimer _timer = UniTimer.CreatePepeatTimer(0, 0.2f);

	public override void OnCreate()
	{
		_info = this.transform.Find("info").GetComponent<Text>();
	}
	public override void OnDestroy()
	{
	}
	public override void OnRefresh()
	{
		_info.text = "Loading";
		_timer.Reset();
		_countdown = 0;
	}
	public override void OnUpdate()
	{
		if(_timer.Update(Time.deltaTime))
		{
			_countdown++;
			if (_countdown > 6)
				_countdown = 0;

			string tips = "Loading";
			for(int i=0; i<_countdown; i++)
			{
				tips += ".";
			}
			_info.text = tips;
		}
	}
}