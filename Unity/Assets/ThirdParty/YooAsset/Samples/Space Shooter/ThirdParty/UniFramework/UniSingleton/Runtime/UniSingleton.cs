using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Singleton
{
	public static class UniSingleton
	{
		private class Wrapper
		{
			public int Priority { private set; get; }
			public ISingleton Singleton { private set; get; }

			public Wrapper(ISingleton module, int priority)
			{
				Singleton = module;
				Priority = priority;
			}
		}

		private static bool _isInitialize = false;
		private static GameObject _driver = null;
		private static readonly List<Wrapper> _wrappers = new List<Wrapper>(100);
		private static MonoBehaviour _behaviour;
		private static bool _isDirty = false;

		/// <summary>
		/// 初始化单例系统
		/// </summary>
		public static void Initialize()
		{
			if (_isInitialize)
				throw new Exception($"{nameof(UniSingleton)} is initialized !");

			if (_isInitialize == false)
			{
				// 创建驱动器
				_isInitialize = true;
				_driver = new UnityEngine.GameObject($"[{nameof(UniSingleton)}]");
				_behaviour = _driver.AddComponent<UniSingletonDriver>();
				UnityEngine.Object.DontDestroyOnLoad(_driver);
				UniLogger.Log($"{nameof(UniSingleton)} initalize !");
			}
		}

		/// <summary>
		/// 销毁单例系统
		/// </summary>
		public static void Destroy()
		{
			if (_isInitialize)
			{
				DestroyAll();

				_isInitialize = false;
				if (_driver != null)
					GameObject.Destroy(_driver);
				UniLogger.Log($"{nameof(UniSingleton)} destroy all !");
			}
		}

		/// <summary>
		/// 更新单例系统
		/// </summary>
		internal static void Update()
		{
			// 如果需要重新排序
			if (_isDirty)
			{
				_isDirty = false;
				_wrappers.Sort((left, right) =>
				{
					if (left.Priority > right.Priority)
						return -1;
					else if (left.Priority == right.Priority)
						return 0;
					else
						return 1;
				});
			}

			// 轮询所有模块
			for (int i = 0; i < _wrappers.Count; i++)
			{
				_wrappers[i].Singleton.OnUpdate();
			}
		}

		/// <summary>
		/// 获取单例
		/// </summary>
		public static T GetSingleton<T>() where T : class, ISingleton
		{
			System.Type type = typeof(T);
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Singleton.GetType() == type)
					return _wrappers[i].Singleton as T;
			}

			UniLogger.Error($"Not found manager : {type}");
			return null;
		}

		/// <summary>
		/// 查询单例是否存在
		/// </summary>
		public static bool Contains<T>() where T : class, ISingleton
		{
			System.Type type = typeof(T);
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Singleton.GetType() == type)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 创建单例
		/// </summary>
		/// <param name="priority">运行时的优先级，优先级越大越早执行。如果没有设置优先级，那么会按照添加顺序执行</param>
		public static T CreateSingleton<T>(int priority = 0) where T : class, ISingleton
		{
			return CreateSingleton<T>(null, priority);
		}

		/// <summary>
		/// 创建单例
		/// </summary>
		/// <param name="createParam">附加参数</param>
		/// <param name="priority">运行时的优先级，优先级越大越早执行。如果没有设置优先级，那么会按照添加顺序执行</param>
		public static T CreateSingleton<T>(System.Object createParam, int priority = 0) where T : class, ISingleton
		{
			if (priority < 0)
				throw new Exception("The priority can not be negative");

			if (Contains<T>())
				throw new Exception($"Module is already existed : {typeof(T)}");

			// 如果没有设置优先级
			if (priority == 0)
			{
				int minPriority = GetMinPriority();
				priority = --minPriority;
			}

			T module = Activator.CreateInstance<T>();
			Wrapper wrapper = new Wrapper(module, priority);
			wrapper.Singleton.OnCreate(createParam);
			_wrappers.Add(wrapper);
			_isDirty = true;
			return module;
		}

		/// <summary>
		/// 销毁单例
		/// </summary>
		public static bool DestroySingleton<T>() where T : class, ISingleton
		{
			var type = typeof(T);
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Singleton.GetType() == type)
				{
					_wrappers[i].Singleton.OnDestroy();
					_wrappers.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 开启一个协程
		/// </summary>
		public static Coroutine StartCoroutine(IEnumerator coroutine)
		{
			return _behaviour.StartCoroutine(coroutine);
		}
		public static Coroutine StartCoroutine(string methodName)
		{
			return _behaviour.StartCoroutine(methodName);
		}

		/// <summary>
		/// 停止一个协程
		/// </summary>
		public static void StopCoroutine(Coroutine coroutine)
		{
			_behaviour.StopCoroutine(coroutine);
		}
		public static void StopCoroutine(string methodName)
		{
			_behaviour.StopCoroutine(methodName);
		}

		/// <summary>
		/// 停止所有协程
		/// </summary>
		public static void StopAllCoroutines()
		{
			_behaviour.StopAllCoroutines();
		}

		private static int GetMinPriority()
		{
			int minPriority = 0;
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Priority < minPriority)
					minPriority = _wrappers[i].Priority;
			}
			return minPriority; //小于等于零
		}
		private static void DestroyAll()
		{
			for (int i = 0; i < _wrappers.Count; i++)
			{
				_wrappers[i].Singleton.OnDestroy();
			}
			_wrappers.Clear();
		}
	}
}