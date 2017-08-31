using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILRuntime.CLR.Method;
using Model;
using UnityEngine;

namespace Hotfix
{
#if ILRuntime
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

		public UI Create(Scene scene, UIType type, GameObject parent)
		{
			this.params3[0] = scene;
			this.params3[1] = type;
			this.params3[2] = parent;
			return (UI)Model.Init.Instance.AppDomain.Invoke(methodInfo, instance, params3);
		}
	}
#else
	public class IMonoUIFactoryMethod : IUIFactory
	{
		private readonly object instance;
		private readonly MethodInfo methodInfo;
		private readonly object[] params3 = new object[3];
		public IMonoUIFactoryMethod(Type type)
		{
			this.instance = Activator.CreateInstance(type);
			this.methodInfo = type.GetMethod("Create");
		}

		public UI Create(Scene scene, UIType type, GameObject parent)
		{
			this.params3[0] = scene;
			this.params3[1] = type;
			this.params3[2] = parent;
			return (UI)this.methodInfo.Invoke(this.instance, this.params3);
		}
	}
#endif

	/// <summary>
	/// 管理所有UI
	/// </summary>
	[ObjectEvent((int)EntityEventId.UIComponent)]
	public class UIComponent: Component, IAwake, ILoad
	{
		private GameObject Root;
		private Dictionary<UIType, IUIFactory> UiTypes;
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
		}

		public void Awake()
		{
			this.Root = GameObject.Find("Global/UI/");
			this.Load();
		}

		public void Load()
		{
            UiTypes = new Dictionary<UIType, IUIFactory>();
            
            Type[] types = DllHelper.GetHotfixTypes();

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
#if ILRuntime
				IUIFactory iuiFactory = new IILUIFactoryMethod(type);
#else
				IUIFactory iuiFactory = new IMonoUIFactoryMethod(type);
#endif

				this.UiTypes.Add((UIType)attribute.Type, iuiFactory);
			}
		}

		public UI Create(UIType type)
		{
			try
			{
				UI ui = UiTypes[type].Create(GetOwner<Scene>(), type, Root);
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