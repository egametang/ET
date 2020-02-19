using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
	[ObjectSystem]
	public class UIViewComponentAwakeSystem : AwakeSystem<UIViewComponent>
	{
		public override void Awake(UIViewComponent self)
		{
		}
	}
	
	/// <summary>
	/// 管理所有UI
	/// </summary>
	public class UIViewComponent: Entity
	{
	}
}