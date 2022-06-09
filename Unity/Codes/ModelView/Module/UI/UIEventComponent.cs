﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
	/// <summary>
	/// 管理所有UI GameObject
	/// </summary>
	[ComponentOf(typeof(Scene))]
	public class UIEventComponent: Entity, IAwake
	{
		public static UIEventComponent Instance;
		
		public Dictionary<string, AUIEvent> UIEvents = new Dictionary<string, AUIEvent>();
		
		public Dictionary<int, Transform> UILayers { get; set;}= new Dictionary<int, Transform>();
	}
}