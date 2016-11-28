using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Base;
using UnityEngine;

namespace Model
{
	/// <summary>
	/// 管理所有UI
	/// </summary>
	[ComponentEvent(typeof(UIComponent))]
	public class UIComponent: Component
	{
        private UI Root;
		private Dictionary<UIType, IUIFactory> UiTypes;
		private readonly Dictionary<UIType, UI> uis = new Dictionary<UIType, UI>();

		public override void Dispose()
		{
			if (this.Id == 0)
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
		}

		private void Awake()
		{
			GameObject uiCanvas = GameObject.Find("/UICanvas");
			uiCanvas.GetComponent<Canvas>().worldCamera = GameObject.Find("/Camera").GetComponent<Camera>();
			this.Root = new UI(this.GetOwner<Scene>(), UIType.Root, null, uiCanvas);
			this.Load();
		}

		private void Load()
		{
			this.UiTypes = new Dictionary<UIType, IUIFactory>();

			Assembly[] assemblies = Game.ComponentEventManager.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					object[] attrs = type.GetCustomAttributes(typeof(UIFactoryAttribute), false);
					if (attrs.Length == 0)
					{
						continue;
					}

					UIFactoryAttribute attribute = attrs[0] as UIFactoryAttribute;
					if (this.UiTypes.ContainsKey(attribute.Type))
					{
						throw new GameException($"已经存在同类UI Factory: {attribute.Type}");
					}
					IUIFactory iIuiFactory = Activator.CreateInstance(type) as IUIFactory;
					if (iIuiFactory == null)
					{
						throw new GameException("UI Factory没有继承IUIFactory");
					}
					this.UiTypes.Add(attribute.Type, iIuiFactory);
				}
			}
		}

		public UI Create(UIType type)
		{
			try
			{
				UI ui = this.UiTypes[type].Create(this.GetOwner<Scene>(), type, this.Root);
				this.uis.Add(type, ui);
				return ui;
			}
			catch (Exception e)
			{
				throw new Exception($"{type} UI 错误: {e}");
			}
		}

		public void Add(UIType type, UI ui)
		{
			this.uis.Add(type, ui);
		}

		public void Remove(UIType type)
		{
			UI ui;
			if (!this.uis.TryGetValue(type, out ui))
			{
				return;
			}
			this.uis.Remove(type);
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