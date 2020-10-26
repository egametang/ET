using System.Collections.Generic;

namespace ET
{
	public class UIComponentAwakeSystem : AwakeSystem<UIComponent>
	{
		public override void Awake(UIComponent self)
		{
		}
	}
	
	/// <summary>
	/// 管理Scene上的UI
	/// </summary>
	public static class UIComponentSystem
	{
		public static async ETTask<UI> Create(this UIComponent self, string uiType)
		{
			UI ui = await UIEventComponent.Instance.OnCreate(self, uiType);
			self.UIs.Add(uiType, ui);
			return ui;
		}

		public static void Remove(this UIComponent self, string uiType)
		{
			if (!self.UIs.TryGetValue(uiType, out UI ui))
			{
				return;
			}
			
			UIEventComponent.Instance.OnRemove(self, uiType);
			
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