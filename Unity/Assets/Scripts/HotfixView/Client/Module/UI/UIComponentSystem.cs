﻿using System.Collections.Generic;

namespace ET.Client
{
	/// <summary>
	/// 管理Scene上的UI
	/// </summary>
	[EntitySystemOf(typeof(UIComponent))]
	[FriendOf(typeof(UIComponent))]
	public static partial class UIComponentSystem
	{
		[EntitySystem]
		private static void Awake(this UIComponent self)
		{
			self.UIGlobalComponent = self.Root().GetComponent<UIGlobalComponent>();
		}
		
		public static async ETTask<UI> Create(this UIComponent self, string uiType, UILayer uiLayer)
		{
			UI ui = await self.UIGlobalComponent.OnCreate(self, uiType, uiLayer);
			self.UIs.Add(uiType, ui);
			return ui;
		}

		public static void Remove(this UIComponent self, string uiType)
		{
			if (!self.UIs.TryGetValue(uiType, out UI ui))
			{
				return;
			}
			
			self.UIGlobalComponent.OnRemove(self, uiType);
			
			self.UIs.Remove(uiType);
			ui.Dispose();
		}

		public static UI Get(this UIComponent self, string name)
		{
			UI ui = null;
			self.UIs.TryGetValue(name, out ui);
			return ui;
		}
	}
}