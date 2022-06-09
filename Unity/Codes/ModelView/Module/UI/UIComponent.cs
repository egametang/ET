﻿using System.Collections.Generic;

namespace ET
{
	/// <summary>
	/// 管理Scene上的UI
	/// </summary>
	[ComponentOf(typeof(Scene))]
	[ChildType(typeof(UI))]
	public class UIComponent: Entity, IAwake
	{
		public Dictionary<string, UI> UIs = new Dictionary<string, UI>();
	}
}