using System;
using System.Collections.Generic;
using System.Linq;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

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
	/// </summary>
	public class UIComponent: Component
	{
		private GameObject Root;
		private readonly Dictionary<string, IUIFactory> UiTypes = new Dictionary<string, IUIFactory>();
		private readonly Dictionary<string, UI> uis = new Dictionary<string, UI>();

	    //自定义扩展
	    private Dictionary<WindowLayer, GameObject> m_allLayers = new Dictionary<WindowLayer, GameObject>();

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
			this.uis.Clear();
		}

		public void Awake()
		{
			this.Root = GameObject.Find("Global/UI/");
		    this.InstantiateUi(Root.transform);
            this.Load();
		}

		public void Load()
		{
			UiTypes.Clear();
            
			List<Type> types = ETModel.Game.Hotfix.GetHotfixTypes();

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

		public UI Create(string type)
		{
			try
			{
			    UI ui;

			    if (uis.ContainsKey(type))
			    {
			        ui = uis[type];
			    }
			    else
			    {
			        ui = UiTypes[type].Create(this.GetParent<Scene>(), type, Root);
			        uis.Add(type, ui);
			    }

			    // 设置Ui层级
			    SetViewParent(ui, ui.GameObject.GetComponent<CanvasConfig>().UiWindowLayer);

			    //调用Show方法
			    ui.UiComponent.Show();

			    return ui;

    //            UI ui = UiTypes[type].Create(this.GetParent<Scene>(), type, Root);
    //            uis.Add(type, ui);

				//// 设置canvas
				//string cavasName = ui.GameObject.GetComponent<CanvasConfig>().CanvasName;
				//ui.GameObject.transform.SetParent(this.Root.Get<GameObject>(cavasName).transform, false);
				//return ui;
			}
			catch (Exception e)
			{
				throw new Exception($"{type} UI 错误: {e}");
			}
		}

	    public void Close(string type)
	    {
	        UI ui;
	        if (!uis.TryGetValue(type, out ui))
	        {
	            return;
	        }
	        uis[type].UiComponent.Close();
	        SetViewParent(uis[type], WindowLayer.UIHiden);
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
            UiTypes[type].Remove(type);
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

        #region 自定义扩展

        private void InstantiateUi(Transform parent)
        {
            WindowLayer[] _names = new WindowLayer[]
            {
                WindowLayer.UIHiden,
                WindowLayer.Bottom,
                WindowLayer.Medium,
                WindowLayer.Top,
                WindowLayer.TopMost
            };
            Camera _cam = new GameObject().AddComponent<Camera>();
            _cam.clearFlags = CameraClearFlags.Depth;
            _cam.cullingMask = 1 << LayerMask.NameToLayer("UI");
            _cam.orthographic = true;
            _cam.depth = 10;
            _cam.name = "UiCamera";
            _cam.transform.SetParent(parent);
            _cam.transform.localPosition = Vector3.zero;

            foreach (var layer in _names)
            {
                var it = layer.ToString();
                GameObject _go = new GameObject();
                this.m_allLayers.Add(layer, _go);
                Canvas _canvas = _go.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = _cam;
                _canvas.sortingOrder = (int)layer;
                CanvasScaler _scale = _go.AddComponent<CanvasScaler>();
                _scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _scale.referenceResolution = new Vector2(1920, 1080);
                _scale.matchWidthOrHeight = 1;
                GraphicRaycaster _graphic = _go.AddComponent<GraphicRaycaster>();
                _go.name = it;
                _go.transform.SetParent(parent);
                _go.transform.localPosition = Vector3.zero;
                if (layer == WindowLayer.UIHiden)
                {
                    _go.layer = LayerMask.NameToLayer("UIHiden");
                    _graphic.enabled = false;
                }
                else
                {
                    _go.layer = LayerMask.NameToLayer("UI");
                }
            }
        }

        //修改UI层级
        void SetViewParent(UI ui, WindowLayer layer)
        {
            RectTransform _rt = ui.GameObject.GetComponent<RectTransform>();
            _rt.SetParent(m_allLayers[layer].transform);
            _rt.anchorMin = Vector2.zero;
            _rt.anchorMax = Vector2.one;
            _rt.offsetMax = Vector2.zero;
            _rt.offsetMin = Vector2.zero;
            _rt.pivot = new Vector2(0.5f, 0.5f);
            //
            _rt.localScale = Vector3.one;
            _rt.localPosition = Vector3.zero;
            _rt.localRotation = Quaternion.identity;
        }

        #endregion
    }
}