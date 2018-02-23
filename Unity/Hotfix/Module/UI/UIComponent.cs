using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace Hotfix
{
	[ObjectSystem]
	public class UiComponentAwakeSystem : AwakeSystem<UIComponent>
	{
		public override void Awake(UIComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class UiComponentLoadSystem : LoadSystem<UIComponent>
	{
		public override void Load(UIComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// 管理所有UI
	/// </summary>
	public class UIComponent: Component
	{
		private GameObject Root;
		private readonly Dictionary<UIType, IUIFactory> UiTypes = new Dictionary<UIType, IUIFactory>();
		private readonly Dictionary<UIType, UI> uis = new Dictionary<UIType, UI>();

		public override void Dispose()
		{
			if (Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (UIType type in uis.Keys.ToArray())
			{
				UI ui;
				if (!uis.TryGetValue(type, out ui))
				{
					continue;
				}
				uis.Remove(type);
				ui.Dispose();
			}

			this.UiTypes.Clear();
			this.uis.Clear();
		}

		public void Awake()
		{
			this.Root = GameObject.Find("Global/UI/");
			this.Load();
		}

		public void Load()
		{
			UiTypes.Clear();
            
            Type[] types = Game.Hotfix.GetHotfixTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof (UIFactoryAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				UIFactoryAttribute attribute = attrs[0] as UIFactoryAttribute;
				if (UiTypes.ContainsKey((UIType)attribute.Type))
				{
                    Log.Debug($"已经存在同类UI Factory: {attribute.Type}");
					throw new Exception($"已经存在同类UI Factory: {attribute.Type}");
				}
				object o = Activator.CreateInstance(type);
				IUIFactory factory = o as IUIFactory;
				if (factory == null)
				{
					Log.Error($"{o.GetType().FullName} 没有继承 IUIFactory");
					continue;
				}
				this.UiTypes.Add((UIType)attribute.Type, factory);
			}
		}

		public UI Create(UIType type)
		{
			try
			{
				UI ui = UiTypes[type].Create(this.GetParent<Scene>(), type, Root);
                uis.Add(type, ui);

				// 设置canvas
				string cavasName = ui.GameObject.GetComponent<CanvasConfig>().CanvasName;
				ui.GameObject.transform.SetParent(this.Root.Get<GameObject>(cavasName).transform, false);
				return ui;
			}
			catch (Exception e)
			{
				throw new Exception($"{type} UI 错误: {e.ToStr()}");
			}
		}

		public void Add(UIType type, UI ui)
		{
			this.uis.Add(type, ui);
		}

		public void Remove(UIType type)
		{
			UI ui;
			if (!uis.TryGetValue(type, out ui))
			{
				return;
			}
            UiTypes[type].Remove(type);
            uis.Remove(type);
			ui.Dispose();
		}

		public void RemoveAll()
		{
			foreach (UIType type in this.uis.Keys.ToArray())
			{
				UI ui;
				if (!this.uis.TryGetValue(type, out ui))
				{
					continue;
                }
                this.uis.Remove(type);
				ui.Dispose();
			}
		}

		public UI Get(UIType type)
		{
			UI ui;
			this.uis.TryGetValue(type, out ui);
			return ui;
		}

		public List<UIType> GetUITypeList()
		{
			return new List<UIType>(this.uis.Keys);
		}
	}
}