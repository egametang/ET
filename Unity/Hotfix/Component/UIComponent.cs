using System;
using System.Collections.Generic;
using System.Linq;
using ILRuntime.CLR.Method;
using Model;
using UnityEngine;

namespace Hotfix
{
	public class IILUIFactoryMethod : IUIFactory
	{
		private readonly object instance;
		private readonly IMethod methodInfo;
		private readonly object[] params3 = new object[3];
		public IILUIFactoryMethod(Type type)
		{
			this.instance = Activator.CreateInstance(type);
			this.methodInfo = DllHelper.GetType(type.FullName).GetMethod("Create", 3);
		}

		public UI Create(Scene scene, int type, UI parent)
		{
			this.params3[0] = scene;
			this.params3[1] = type;
			this.params3[2] = parent;
			return (UI)Init.Instance.AppDomain.Invoke(this.methodInfo, this.instance, this.params3);
		}
	}

	/// <summary>
	/// 管理所有UI
	/// </summary>
	[ObjectEvent(EntityEventId.UIComponent)]
	public class UIComponent: Component, IAwake, ILoad
	{
		private UI Root;
		private Dictionary<int, IUIFactory> UiTypes;
		private readonly Dictionary<int, UI> uis = new Dictionary<int, UI>();

		public override void Dispose()
		{
			if (this.Id == 0)
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
		}

		public void Awake()
		{
			GameObject uiCanvas = GameObject.Find("Global/UI/UICanvas");
			this.Root = new UI(this.GetOwner<Scene>(), UIType.Root, null, uiCanvas);
			this.Load();
		}

		public void Load()
		{
			this.UiTypes = new Dictionary<int, IUIFactory>();
			
			Type[] types = DllHelper.GetHotfixTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof (UIFactoryAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				UIFactoryAttribute attribute = attrs[0] as UIFactoryAttribute;
				if (this.UiTypes.ContainsKey(attribute.Type))
				{
					throw new Exception($"已经存在同类UI Factory: {attribute.Type}");
				}
				
				IUIFactory iuiFactory = new IILUIFactoryMethod(type);

				this.UiTypes.Add(attribute.Type, iuiFactory);
			}
		}

		public UI Create(int type)
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

		public void Add(int type, UI ui)
		{
			this.uis.Add(type, ui);
		}

		public void Remove(int type)
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