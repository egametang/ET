using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace UniFramework.Pooling
{
	/// <summary>
	/// 游戏对象池系统
	/// </summary>
	public static class UniPooling
	{
		private static bool _isInitialize = false;
		private static GameObject _driver = null;
		private static readonly List<Spawner> _spawners = new List<Spawner>();


		/// <summary>
		/// 初始化游戏对象池系统
		/// </summary>
		public static void Initalize()
		{
			if (_isInitialize)
				throw new Exception($"{nameof(UniPooling)} is initialized !");

			if (_isInitialize == false)
			{
				// 创建驱动器
				_isInitialize = true;
				_driver = new UnityEngine.GameObject($"[{nameof(UniPooling)}]");
				_driver.AddComponent<UniPoolingDriver>();
				UnityEngine.Object.DontDestroyOnLoad(_driver);
				UniLogger.Log($"{nameof(UniPooling)} initalize !");
			}
		}

		/// <summary>
		/// 销毁游戏对象池系统
		/// </summary>
		public static void Destroy()
		{
			if (_isInitialize)
			{
				foreach (var spawner in _spawners)
				{
					spawner.Destroy();
				}
				_spawners.Clear();

				_isInitialize = false;
				if (_driver != null)
					GameObject.Destroy(_driver);
				UniLogger.Log($"{nameof(UniPooling)} destroy all !");
			}
		}

		/// <summary>
		/// 更新游戏对象池系统
		/// </summary>
		internal static void Update()
		{
			if (_isInitialize)
			{
				foreach (var spawner in _spawners)
				{
					spawner.Update();
				}
			}
		}

		/// <summary>
		/// 创建游戏对象生成器
		/// </summary>
		/// <param name="packageName">资源包名称</param>
		public static Spawner CreateSpawner(string packageName)
		{
			// 获取资源包
			var assetPackage = YooAssets.GetPackage(packageName);
			if (assetPackage == null)
				throw new Exception($"Not found asset package : {packageName}");

			// 检测资源包初始化状态
			if (assetPackage.InitializeStatus == EOperationStatus.None)
				throw new Exception($"Asset package {packageName} not initialize !");
			if (assetPackage.InitializeStatus == EOperationStatus.Failed)
				throw new Exception($"Asset package {packageName} initialize failed !");

			if (HasSpawner(packageName))
				return GetSpawner(packageName);

			Spawner spawner = new Spawner(_driver, assetPackage);
			_spawners.Add(spawner);
			return spawner;
		}

		/// <summary>
		/// 获取游戏对象生成器
		/// </summary>
		/// <param name="packageName">资源包名称</param>
		public static Spawner GetSpawner(string packageName)
		{
			foreach (var spawner in _spawners)
			{
				if (spawner.PackageName == packageName)
					return spawner;
			}

			UniLogger.Warning($"Not found spawner : {packageName}");
			return null;
		}

		/// <summary>
		/// 检测游戏对象生成器是否存在
		/// </summary>
		/// <param name="packageName">资源包名称</param>
		public static bool HasSpawner(string packageName)
		{
			foreach (var spawner in _spawners)
			{
				if (spawner.PackageName == packageName)
					return true;
			}
			return false;
		}
	}
}