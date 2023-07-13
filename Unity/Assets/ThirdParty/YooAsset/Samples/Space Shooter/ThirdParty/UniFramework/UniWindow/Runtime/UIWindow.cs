using System;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace UniFramework.Window
{
	public abstract class UIWindow
	{
		public const int WINDOW_HIDE_LAYER = 2; // Ignore Raycast
		public const int WINDOW_SHOW_LAYER = 5; // UI

		internal AssetOperationHandle Handle { private set; get;}
		private System.Action<UIWindow> _prepareCallback;
		private System.Object[] _userDatas;

		private bool _isCreate = false;
		private GameObject _panel;
		private Canvas _canvas;
		private Canvas[] _childCanvas;
		private GraphicRaycaster _raycaster;
		private GraphicRaycaster[] _childRaycaster;

		/// <summary>
		/// 面板的Transfrom组件
		/// </summary>
		public Transform transform
		{
			get
			{
				return _panel.transform;
			}
		}

		/// <summary>
		/// 面板的游戏对象
		/// </summary>
		public GameObject gameObject
		{
			get
			{
				return _panel;
			}
		}
		
		/// <summary>
		/// 窗口名称
		/// </summary>
		public string WindowName { private set; get; }

		/// <summary>
		/// 窗口层级
		/// </summary>
		public int WindowLayer { private set; get; }

		/// <summary>
		/// 是否为全屏窗口
		/// </summary>
		public bool FullScreen { private set; get; }

		/// <summary>
		/// 自定义数据
		/// </summary>
		public System.Object UserData
		{
			get
			{
				if (_userDatas != null && _userDatas.Length >= 1)
					return _userDatas[0];
				else
					return null;
			}
		}

		/// <summary>
		/// 自定义数据集
		/// </summary>
		public System.Object[] UserDatas
		{
			get { return _userDatas; }
		}

		/// <summary>
		/// 窗口深度值
		/// </summary>
		public int Depth
		{
			get
			{
				if (_canvas != null)
					return _canvas.sortingOrder;
				else
					return 0;
			}

			set
			{
				if (_canvas != null)
				{
					if (_canvas.sortingOrder == value)
						return;

					// 设置父类
					_canvas.sortingOrder = value;

					// 设置子类
					int depth = value;
					for (int i = 0; i < _childCanvas.Length; i++)
					{
						var canvas = _childCanvas[i];
						if (canvas != _canvas)
						{
							depth += 5; //注意递增值
							canvas.sortingOrder = depth;
						}
					}

					// 虚函数
					if (_isCreate)
						OnSortDepth(value);
				}
			}
		}

		/// <summary>
		/// <summary>
		/// 窗口可见性
		/// </summary>
		public bool Visible
		{
			get
			{
				if (_canvas != null)
					return _canvas.gameObject.layer == WINDOW_SHOW_LAYER;
				else
					return false;
			}

			set
			{
				if (_canvas != null)
				{
					int setLayer = value ? WINDOW_SHOW_LAYER : WINDOW_HIDE_LAYER;
					if (_canvas.gameObject.layer == setLayer)
						return;

					// 显示设置
					_canvas.gameObject.layer = setLayer;
					for (int i = 0; i < _childCanvas.Length; i++)
					{
						_childCanvas[i].gameObject.layer = setLayer;
					}

					// 交互设置
					Interactable = value;

					// 虚函数
					if (_isCreate)
						OnSetVisible(value);
				}
			}
		}

		/// <summary>
		/// 窗口交互性
		/// </summary>
		private bool Interactable
		{
			get
			{
				if (_raycaster != null)
					return _raycaster.enabled;
				else
					return false;
			}

			set
			{
				if (_raycaster != null)
				{
					_raycaster.enabled = value;
					for (int i = 0; i < _childRaycaster.Length; i++)
					{
						_childRaycaster[i].enabled = value;
					}
				}
			}
		}

		/// <summary>
		/// 是否加载完毕
		/// </summary>
		internal bool IsLoadDone { get { return Handle.IsDone; } }

		/// <summary>
		/// 是否准备完毕
		/// </summary>
		internal bool IsPrepare { private set; get; }


		public void Init(string name, int layer, bool fullScreen)
		{
			WindowName = name;
			WindowLayer = layer;
			FullScreen = fullScreen;
		}

		/// <summary>
		/// 窗口创建
		/// </summary>
		public abstract void OnCreate();

		/// <summary>
		/// 窗口刷新
		/// </summary>
		public abstract void OnRefresh();

		/// <summary>
		/// 窗口更新
		/// </summary>
		public abstract void OnUpdate();

		/// <summary>
		/// 窗口销毁
		/// </summary>
		public abstract void OnDestroy();

		/// <summary>
		/// 当触发窗口的层级排序
		/// </summary>
		protected virtual void OnSortDepth(int depth) { }

		/// <summary>
		/// 当因为全屏遮挡触发窗口的显隐
		/// </summary>
		protected virtual void OnSetVisible(bool visible) { }

		internal void TryInvoke(System.Action<UIWindow> prepareCallback, System.Object[] userDatas)
		{
			_userDatas = userDatas;
			if (IsPrepare)
				prepareCallback?.Invoke(this);
			else
				_prepareCallback = prepareCallback;
		}
		internal void InternalLoad(string location, System.Action<UIWindow> prepareCallback, System.Object[] userDatas)
		{
			if (Handle != null)
				return;

			_prepareCallback = prepareCallback;
			_userDatas = userDatas;
			Handle = YooAssets.LoadAssetAsync<GameObject>(location);
			Handle.Completed += Handle_Completed;
		}
		internal void InternalCreate()
		{
			if (_isCreate == false)
			{
				_isCreate = true;
				OnCreate();
			}
		}
		internal void InternalRefresh()
		{
			OnRefresh();
		}
		internal void InternalUpdate()
		{
			if(IsPrepare)
			{
				OnUpdate();
			}
		}
		internal void InternalDestroy()
		{
			_isCreate = false;

			// 注销回调函数
			_prepareCallback = null;

			// 卸载面板资源
			if (Handle != null)
			{
				Handle.Release();
				Handle = null;
			}

			// 销毁面板对象
			if (_panel != null)
			{
				OnDestroy();
				GameObject.Destroy(_panel);
				_panel = null;
			}
		}

		private void Handle_Completed(AssetOperationHandle handle)
		{
			if (handle.AssetObject == null)
				return;

			// 实例化对象
			_panel = handle.InstantiateSync(UniWindow.Desktop.transform);
			_panel.transform.localPosition = Vector3.zero;

			// 获取组件
			_canvas = _panel.GetComponent<Canvas>();
			if (_canvas == null)
				throw new Exception($"Not found {nameof(Canvas)} in panel {WindowName}");
			_canvas.overrideSorting = true;
			_canvas.sortingOrder = 0;
			_canvas.sortingLayerName = "Default";

			// 获取组件
			_raycaster = _panel.GetComponent<GraphicRaycaster>();
			_childCanvas = _panel.GetComponentsInChildren<Canvas>(true);
			_childRaycaster = _panel.GetComponentsInChildren<GraphicRaycaster>(true);

			// 通知UI管理器
			IsPrepare = true;
			_prepareCallback?.Invoke(this);
		}
	}
}