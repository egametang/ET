using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ETModel
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
		private readonly Dictionary<int, IUIFactory> UiTypes = new Dictionary<int, IUIFactory>();
		private readonly Dictionary<int, UI> uis = new Dictionary<int, UI>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (int type in uis.Keys.ToArray())
			{
				UI ui;
				if (!uis.TryGetValue(type, out ui))
				{
					continue;
				}
				uis.Remove(type);
				ui.Dispose();
			}

			this.uis.Clear();
			this.UiTypes.Clear();
		}

		public void Awake()
		{
			this.Root = GameObject.Find("Global/UI/");
			this.Load();
		}

		public void Load()
		{
			this.UiTypes.Clear();
            
            Type[] types = DllHelper.GetMonoTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof (UIFactoryAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				UIFactoryAttribute attribute = attrs[0] as UIFactoryAttribute;
				if (UiTypes.ContainsKey(attribute.Type))
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
				this.UiTypes.Add(attribute.Type, factory);
			}
		}

		public UI Create(int type)
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
				throw new Exception($"{type} UI 错误: {e}");
			}
		}

		public void Add(int type, UI ui)
		{
			this.uis.Add(type, ui);
		}

		public void Remove(int type)
		{
			UI ui;
			if (!uis.TryGetValue(type, out ui))
			{
				return;
			}
            uis.Remove(type);
			ui.Dispose();
		}

		public void RemoveAll()
		{
			foreach (int type in this.uis.Keys.ToArray())
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

		public UI Get(int type)
		{
			UI ui;
			this.uis.TryGetValue(type, out ui);
			return ui;
		}

		public List<int> GetUITypeList()
		{
			return new List<int>(this.uis.Keys);
		}
	}
}