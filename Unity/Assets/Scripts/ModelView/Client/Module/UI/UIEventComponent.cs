using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
	/// <summary>
	/// 管理所有UI GameObject
	/// </summary>
	[ComponentOf(typeof(VProcess))]
	public class UIEventComponent: SingletonEntity<UIEventComponent>, IAwake
	{
		public Dictionary<string, AUIEvent> UIEvents = new Dictionary<string, AUIEvent>();
		
		public Dictionary<int, Transform> UILayers = new Dictionary<int, Transform>();
	}
}