using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;

namespace Model
{
	public class IILUIFactoryMethod: IUIFactory
	{
		private readonly ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private readonly ILTypeInstance instance;
		private readonly IMethod method;
		private readonly object[] params3 = new object[3];

		public IILUIFactoryMethod(Type type)
		{
			appDomain = ObjectEvents.Instance.AppDomain;
			this.instance = this.appDomain.Instantiate(type.FullName);
			this.method = this.instance.Type.GetMethod("Create", 3);
		}

		public UI Create(Scene scene, int type, UI parent)
		{
			this.params3[0] = scene;
			this.params3[1] = type;
			this.params3[2] = parent;
			object obj = this.appDomain.Invoke(this.method, this.instance, this.params3);
			return (UI) obj;
		}
	}

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

		public UI Create(Scene scene, int type, UI parent)
		{
			this.params3[0] = scene;
			this.params3[1] = type;
			this.params3[2] = parent;
			return (UI)this.methodInfo.Invoke(this.instance, params3);
		}
	}

	/// <summary>
	/// 管理所有UI
	/// </summary>
	[EntityEvent(EntityEventId.UIComponent)]
	public class UIComponent: Component
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

		private void Awake()
		{
			GameObject uiCanvas = GameObject.Find("Global/UI/UICanvas");
			this.Root = new UI(this.GetOwner<Scene>(), UIType.Root, null, uiCanvas);
			this.Load();
		}

		private void Load()
		{
			this.UiTypes = new Dictionary<int, IUIFactory>();

#if ILRuntime
			Type[] types = DllHelper.GetHotfixTypes();
#else
			Type[] types = DllHelper.GetMonoTypes();
#endif
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
					throw new GameException($"已经存在同类UI Factory: {attribute.Type}");
				}

#if ILRuntime
				IUIFactory iuiFactory = new IILUIFactoryMethod(type);
#else
				IUIFactory iuiFactory = new IMonoUIFactoryMethod(type);
#endif

				this.UiTypes.Add(attribute.Type, iuiFactory);
			}
		}

		public UI Create(int type)
		{
			try
			{
				UI ui = this.UiTypes[type].Create(this.GetOwner<Scene>(), type, this.Root);
				this.uis.Add(type, ui);

				// 假如UI有配置行为树,执行之
				UIConfig uiConfig = ui.GameObject.GetComponent<UIConfig>();
				if (uiConfig != null)
				{
					BehaviorTree behaviorTree = Game.Scene.GetComponent<BehaviorTreeComponent>().CreateTree(this.GetOwner<Scene>(), uiConfig.CreateBT);
					BTEnv env = new BTEnv
					{
						{"UI", ui}
					};
					behaviorTree.Run(env);
				}

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