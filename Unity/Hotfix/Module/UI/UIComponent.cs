using System;
using System.Collections.Generic;
using System.Linq;
using ETModel;
using UnityEngine;

namespace ETHotfix
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
    /// 同一种UI 只能实现一个工厂 带参数和不带参数去选
	/// </summary>
	public class UIComponent: Component
	{
		private GameObject Root;
		private readonly Dictionary<string, IUIFactory> UiTypes = new Dictionary<string, IUIFactory>();
        private readonly Dictionary<string, IUIFactoryP1> UiP1Types = new Dictionary<string, IUIFactoryP1>();
        private readonly Dictionary<string, UI> uis = new Dictionary<string, UI>();
        
        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (string type in uis.Keys.ToArray())
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

            this.UiP1Types.Clear(); 

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
            UiP1Types.Clear();
            Type[] types = ETModel.Game.Hotfix.GetHotfixTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof (UIFactoryAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				UIFactoryAttribute attribute = attrs[0] as UIFactoryAttribute;
                object o = Activator.CreateInstance(type);
                if(o is IUIFactory )
                {
                    if (UiTypes.ContainsKey(attribute.Type))
                    {
                        Log.Debug($"已经存在同类UI Factory: {attribute.Type}");
                        throw new Exception($"已经存在同类UI Factory: {attribute.Type}");
                    }
                    IUIFactory factory = o as IUIFactory;
                    if (factory == null)
                    {
                        Log.Error($"{o.GetType().FullName} 没有继承 IUIFactory");
                        continue;
                    }
                    this.UiTypes.Add(attribute.Type, factory);
                }
                else if (o is IUIFactoryP1)
                {
                    if (UiP1Types.ContainsKey(attribute.Type))
                    {
                        Log.Debug($"已经存在同类UI FactoryP1: {attribute.Type}");
                        throw new Exception($"已经存在同类UI FactoryP1: {attribute.Type}");
                    }
                    IUIFactoryP1 factory = o as IUIFactoryP1;
                    if (factory == null)
                    {
                        Log.Error($"{o.GetType().FullName} 没有继承 IUIFactoryP1");
                        continue;
                    }
                    this.UiP1Types.Add(attribute.Type, factory);
                }
			}
		}

		public UI Create(string type)
		{
			try
			{                
				UI ui = UiTypes[type].Create(this.GetParent<Scene>(), type, Root);
                uis.Add(type, ui);

                //Log.Warning($"create ui {type} id is == {ui.InstanceId}");

                // 设置canvas
                string cavasName = ui.GameObject.GetComponent<CanvasConfig>().CanvasName;
				GameObject obj = this.Root.Get<GameObject>(cavasName);
				ui.GameObject.transform.SetParent(obj.transform, false);
				return ui;
			}
			catch (Exception e)
			{
				throw new Exception($"{type} UI 错误: {e}");
			}
		}
        public UI Create<K>(string type,K k) where K :  new()
        {
            try
            {
                UI ui = UiP1Types[type].Create<K>(this.GetParent<Scene>(), type, Root,k);
                uis.Add(type, ui);

                //Log.Warning($"create ui {type} id is == {ui.InstanceId}");

                // 设置canvas
                string cavasName = ui.GameObject.GetComponent<CanvasConfig>().CanvasName;
                GameObject obj = this.Root.Get<GameObject>(cavasName);
                ui.GameObject.transform.SetParent(obj.transform, false);
                return ui;
            }
            catch (Exception e)
            {
                throw new Exception($"{type} UI 错误: {e}");
            }
        }

        public void Add(string type, UI ui)
		{
			this.uis.Add(type, ui);
		}

		public void Remove(string type)
		{
			UI ui;
			if (!uis.TryGetValue(type, out ui))
			{
				return;
			}
            if(UiTypes.ContainsKey(type))
            {
                UiTypes[type].Remove(type);
            }
            if (UiP1Types.ContainsKey(type))
            {
                UiP1Types[type].Remove(type);
            }

            uis.Remove(type);
			ui.Dispose();
		}

		public void RemoveAll()
		{
			foreach (string type in this.uis.Keys.ToArray())
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

		public UI Get(string type)
		{
			UI ui;
			this.uis.TryGetValue(type, out ui);
			return ui;
		}

		public List<string> GetUITypeList()
		{
			return new List<string>(this.uis.Keys);
		}
	}
}