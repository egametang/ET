using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace UniFramework.Window
{
	public static class UniWindow
	{
		public struct WindowInfo
		{
			public string WindowName;
			public int WindowLayer;
			public bool IsLoadDone;
		}

		private static bool _isInitialize = false;
		private static GameObject _driver = null;
		private static readonly List<UIWindow> _stack = new List<UIWindow>(100);
		internal static GameObject Desktop { private set; get; }


		/// <summary>
		/// 初始化界面系统
		/// </summary>
		public static void Initalize(GameObject desktop)
		{
			if (_isInitialize)
				throw new Exception($"{nameof(UniWindow)} is initialized !");
			if (desktop == null)
				throw new ArgumentNullException();

			if (_isInitialize == false)
			{
				// 创建驱动器
				_isInitialize = true;
				_driver = new UnityEngine.GameObject($"[{nameof(UniWindow)}]");
				_driver.AddComponent<UniWindowDriver>();
				UnityEngine.Object.DontDestroyOnLoad(_driver);
				UniLogger.Log($"{nameof(UniWindow)} initalize !");

				Desktop = desktop;
			}
		}

		/// <summary>
		/// 销毁界面系统
		/// </summary>
		public static void Destroy()
		{
			if (_isInitialize)
			{
				CloseAll();

				_isInitialize = false;
				if (_driver != null)
					GameObject.Destroy(_driver);
				UniLogger.Log($"{nameof(UniWindow)} destroy all !");
			}
		}

		/// <summary>
		/// 更新界面系统
		/// </summary>
		internal static void Update()
		{
			if (_isInitialize)
			{
				int count = _stack.Count;
				for (int i = 0; i < _stack.Count; i++)
				{
					if (_stack.Count != count)
						break;
					var window = _stack[i];
					window.InternalUpdate();
				}
			}
		}

		/// <summary>
		/// 设置屏幕安全区域（异形屏支持）
		/// </summary>
		/// <param name="safeRect">安全区域</param>
		public static void ApplyScreenSafeRect(Rect safeRect)
		{
			CanvasScaler scaler = Desktop.GetComponentInParent<CanvasScaler>();
			if (scaler == null)
			{
				UniLogger.Error($"Not found {nameof(CanvasScaler)} !");
				return;
			}

			// Convert safe area rectangle from absolute pixels to UGUI coordinates
			float rateX = scaler.referenceResolution.x / Screen.width;
			float rateY = scaler.referenceResolution.y / Screen.height;
			float posX = (int)(safeRect.position.x * rateX);
			float posY = (int)(safeRect.position.y * rateY);
			float width = (int)(safeRect.size.x * rateX);
			float height = (int)(safeRect.size.y * rateY);

			float offsetMaxX = scaler.referenceResolution.x - width - posX;
			float offsetMaxY = scaler.referenceResolution.y - height - posY;

			// 注意：安全区坐标系的原点为左下角	
			var rectTrans = Desktop.transform as RectTransform;
			rectTrans.offsetMin = new Vector2(posX, posY); //锚框状态下的屏幕左下角偏移向量
			rectTrans.offsetMax = new Vector2(-offsetMaxX, -offsetMaxY); //锚框状态下的屏幕右上角偏移向量
		}

		/// <summary>
		/// 模拟IPhoneX异形屏
		/// </summary>
		public static void SimulateIPhoneXNotchScreen()
		{
			Rect rect;
			if (Screen.height > Screen.width)
			{
				// 竖屏Portrait
				float deviceWidth = 1125;
				float deviceHeight = 2436;
				rect = new Rect(0f / deviceWidth, 102f / deviceHeight, 1125f / deviceWidth, 2202f / deviceHeight);
			}
			else
			{
				// 横屏Landscape
				float deviceWidth = 2436;
				float deviceHeight = 1125;
				rect = new Rect(132f / deviceWidth, 63f / deviceHeight, 2172f / deviceWidth, 1062f / deviceHeight);
			}

			Rect safeArea = new Rect(Screen.width * rect.x, Screen.height * rect.y, Screen.width * rect.width, Screen.height * rect.height);
			ApplyScreenSafeRect(safeArea);
		}


		/// <summary>
		/// 获取窗口堆栈信息
		/// </summary>
		public static void GetWindowInfos(List<WindowInfo> output)
		{
			if (output == null)
				output = new List<WindowInfo>();
			else
				output.Clear();

			for (int i = 0; i < _stack.Count; i++)
			{
				var window = _stack[i];
				WindowInfo info = new WindowInfo();
				info.WindowName = window.WindowName;
				info.WindowLayer = window.WindowLayer;
				info.IsLoadDone = window.IsLoadDone;
				output.Add(info);
			}
		}

		/// <summary>
		/// 获取所有层级下顶部的窗口名称
		/// </summary>
		public static string GetTopWindow()
		{
			if (_stack.Count == 0)
				return string.Empty;

			UIWindow topWindow = _stack[_stack.Count - 1];
			return topWindow.WindowName;
		}

		/// <summary>
		/// 获取指定层级下顶部的窗口名称
		/// </summary>
		public static string GetTopWindow(int layer)
		{
			UIWindow lastOne = null;
			for (int i = 0; i < _stack.Count; i++)
			{
				if (_stack[i].WindowLayer == layer)
					lastOne = _stack[i];
			}

			if (lastOne == null)
				return string.Empty;

			return lastOne.WindowName;
		}

		/// <summary>
		/// 是否有任意窗口正在加载
		/// </summary>
		public static bool IsAnyLoading()
		{
			for (int i = 0; i < _stack.Count; i++)
			{
				var window = _stack[i];
				if (window.IsLoadDone == false)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 查询窗口是否存在
		/// </summary>
		public static bool HasWindow<T>()
		{
			return HasWindow(typeof(T));
		}
		public static bool HasWindow(Type type)
		{
			return IsContains(type.FullName);
		}


		/// <summary>
		/// 异步打开窗口
		/// </summary>
		/// <param name="location">资源定位地址</param>
		/// <param name="userDatas">用户自定义数据</param>
		public static OpenWindowOperation OpenWindowAsync<T>(string location, params System.Object[] userDatas) where T : UIWindow
		{
			return OpenWindowAsync(typeof(T), location, userDatas);
		}
		public static OpenWindowOperation OpenWindowAsync(Type type, string location, params System.Object[] userDatas)
		{
			string windowName = type.FullName;

			// 如果窗口已经存在
			if (IsContains(windowName))
			{
				UIWindow window = GetWindow(windowName);
				Pop(window); //弹出窗口
				Push(window); //重新压入
				window.TryInvoke(OnWindowPrepare, userDatas);
				var operation = new OpenWindowOperation(window.Handle);
				YooAssets.StartOperation(operation);
				return operation;
			}
			else
			{
				UIWindow window = CreateInstance(type);
				Push(window); //首次压入
				window.InternalLoad(location, OnWindowPrepare, userDatas);
				var operation = new OpenWindowOperation(window.Handle);
				YooAssets.StartOperation(operation);
				return operation;
			}
		}

		/// <summary>
		/// 同步打开窗口
		/// </summary>
		/// <typeparam name="T">窗口类</typeparam>
		/// <param name="location">资源定位地址</param>
		/// <param name="userDatas">用户自定义数据</param>
		public static OpenWindowOperation OpenWindowSync<T>(string location, params System.Object[] userDatas) where T : UIWindow
		{
			var operation = OpenWindowAsync(typeof(T), location, userDatas);
			operation.WaitForAsyncComplete();
			return operation;
		}
		public static OpenWindowOperation OpenWindowSync(Type type, string location, params System.Object[] userDatas)
		{
			var operation = OpenWindowAsync(type, location, userDatas);
			operation.WaitForAsyncComplete();
			return operation;
		}

		/// <summary>
		/// 关闭窗口
		/// </summary>
		public static void CloseWindow<T>() where T : UIWindow
		{
			CloseWindow(typeof(T));
		}
		public static void CloseWindow(Type type)
		{
			string windowName = type.FullName;
			UIWindow window = GetWindow(windowName);
			if (window == null)
				return;

			window.InternalDestroy();
			Pop(window);
			OnSortWindowDepth(window.WindowLayer);
			OnSetWindowVisible();
		}

		/// <summary>
		/// 关闭所有窗口
		/// </summary>
		public static void CloseAll()
		{
			for (int i = 0; i < _stack.Count; i++)
			{
				UIWindow window = _stack[i];
				window.InternalDestroy();
			}
			_stack.Clear();
		}


		private static void OnWindowPrepare(UIWindow window)
		{
			OnSortWindowDepth(window.WindowLayer);
			window.InternalCreate();
			window.InternalRefresh();
			OnSetWindowVisible();
		}
		private static void OnSortWindowDepth(int layer)
		{
			int depth = layer;
			for (int i = 0; i < _stack.Count; i++)
			{
				if (_stack[i].WindowLayer == layer)
				{
					_stack[i].Depth = depth;
					depth += 100; //注意：每次递增100深度
				}
			}
		}
		private static void OnSetWindowVisible()
		{
			bool isHideNext = false;
			for (int i = _stack.Count - 1; i >= 0; i--)
			{
				UIWindow window = _stack[i];
				if (isHideNext == false)
				{
					window.Visible = true;
					if (window.IsPrepare && window.FullScreen)
						isHideNext = true;
				}
				else
				{
					window.Visible = false;
				}
			}
		}

		private static UIWindow CreateInstance(Type type)
		{
			UIWindow window = Activator.CreateInstance(type) as UIWindow;
			WindowAttribute attribute = Attribute.GetCustomAttribute(type, typeof(WindowAttribute)) as WindowAttribute;

			if (window == null)
				throw new Exception($"Window {type.FullName} create instance failed.");
			if (attribute == null)
				throw new Exception($"Window {type.FullName} not found {nameof(WindowAttribute)} attribute.");

			window.Init(type.FullName, attribute.WindowLayer, attribute.FullScreen);
			return window;
		}
		private static UIWindow GetWindow(string name)
		{
			for (int i = 0; i < _stack.Count; i++)
			{
				UIWindow window = _stack[i];
				if (window.WindowName == name)
					return window;
			}
			return null;
		}
		private static bool IsContains(string name)
		{
			for (int i = 0; i < _stack.Count; i++)
			{
				UIWindow window = _stack[i];
				if (window.WindowName == name)
					return true;
			}
			return false;
		}
		private static void Push(UIWindow window)
		{
			// 如果已经存在
			if (IsContains(window.WindowName))
				throw new System.Exception($"Window {window.WindowName} is exist.");

			// 获取插入到所属层级的位置
			int insertIndex = -1;
			for (int i = 0; i < _stack.Count; i++)
			{
				if (window.WindowLayer == _stack[i].WindowLayer)
					insertIndex = i + 1;
			}

			// 如果没有所属层级，找到相邻层级
			if (insertIndex == -1)
			{
				for (int i = 0; i < _stack.Count; i++)
				{
					if (window.WindowLayer > _stack[i].WindowLayer)
						insertIndex = i + 1;
				}
			}

			// 如果是空栈或没有找到插入位置
			if (insertIndex == -1)
			{
				insertIndex = 0;
			}

			// 最后插入到堆栈
			_stack.Insert(insertIndex, window);
		}
		private static void Pop(UIWindow window)
		{
			// 从堆栈里移除
			_stack.Remove(window);
		}
	}
}