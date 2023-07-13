using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace UniFramework.Pooling
{
	internal class GameObjectPool
	{
		private readonly GameObject _root;
		private readonly Queue<InstantiateOperation> _cacheOperations;
		private readonly bool _dontDestroy;
		private readonly int _initCapacity;
		private readonly int _maxCapacity;
		private readonly float _destroyTime;
		private float _lastRestoreRealTime = -1f;

		/// <summary>
		/// 资源句柄
		/// </summary>
		public AssetOperationHandle AssetHandle { private set; get; }

		/// <summary>
		/// 资源定位地址
		/// </summary>
		public string Location { private set; get; }

		/// <summary>
		/// 内部缓存总数
		/// </summary>
		public int CacheCount
		{
			get { return _cacheOperations.Count; }
		}

		/// <summary>
		/// 外部使用总数
		/// </summary>
		public int SpawnCount { private set; get; } = 0;

		/// <summary>
		/// 是否常驻不销毁
		/// </summary>
		public bool DontDestroy
		{
			get { return _dontDestroy; }
		}


		public GameObjectPool(GameObject poolingRoot, string location, bool dontDestroy, int initCapacity, int maxCapacity, float destroyTime)
		{
			_root = new GameObject(location);
			_root.transform.parent = poolingRoot.transform;
			Location = location;

			_dontDestroy = dontDestroy;
			_initCapacity = initCapacity;
			_maxCapacity = maxCapacity;
			_destroyTime = destroyTime;

			// 创建缓存池
			_cacheOperations = new Queue<InstantiateOperation>(initCapacity);
		}

		/// <summary>
		/// 创建对象池
		/// </summary>
		public void CreatePool(ResourcePackage package)
		{
			// 加载游戏对象
			AssetHandle = package.LoadAssetAsync<GameObject>(Location);

			// 创建初始对象
			for (int i = 0; i < _initCapacity; i++)
			{
				var operation = AssetHandle.InstantiateAsync(_root.transform);
				operation.Completed += Operation_Completed;
				_cacheOperations.Enqueue(operation);
			}
		}
		private void Operation_Completed(AsyncOperationBase obj)
		{
			if (obj.Status == EOperationStatus.Succeed)
			{
				var op = obj as InstantiateOperation;
				if (op.Result != null)
					op.Result.SetActive(false);
			}
		}

		/// <summary>
		/// 销毁游戏对象池
		/// </summary>
		public void DestroyPool()
		{
			// 卸载资源对象
			AssetHandle.Release();
			AssetHandle = null;

			// 销毁游戏对象
			GameObject.Destroy(_root);
			_cacheOperations.Clear();

			SpawnCount = 0;
		}

		/// <summary>
		/// 查询静默时间内是否可以销毁
		/// </summary>
		public bool CanAutoDestroy()
		{
			if (_dontDestroy)
				return false;
			if (_destroyTime < 0)
				return false;

			if (_lastRestoreRealTime > 0 && SpawnCount <= 0)
				return (Time.realtimeSinceStartup - _lastRestoreRealTime) > _destroyTime;
			else
				return false;
		}

		/// <summary>
		/// 游戏对象池是否已经销毁
		/// </summary>
		public bool IsDestroyed()
		{
			return AssetHandle == null;
		}

		/// <summary>
		/// 回收
		/// </summary>
		public void Restore(InstantiateOperation operation)
		{
			if (IsDestroyed())
			{
				DestroyInstantiateOperation(operation);
				return;
			}

			SpawnCount--;
			if (SpawnCount <= 0)
				_lastRestoreRealTime = Time.realtimeSinceStartup;

			// 如果外部逻辑销毁了游戏对象
			if (operation.Status == EOperationStatus.Succeed)
			{
				if (operation.Result == null)
					return;
			}

			// 如果缓存池还未满员
			if (_cacheOperations.Count < _maxCapacity)
			{
				SetRestoreGameObject(operation.Result);
				_cacheOperations.Enqueue(operation);
			}
			else
			{
				DestroyInstantiateOperation(operation);
			}
		}

		/// <summary>
		/// 丢弃
		/// </summary>
		public void Discard(InstantiateOperation operation)
		{
			if (IsDestroyed())
			{
				DestroyInstantiateOperation(operation);
				return;
			}

			SpawnCount--;
			if (SpawnCount <= 0)
				_lastRestoreRealTime = Time.realtimeSinceStartup;

			DestroyInstantiateOperation(operation);
		}

		/// <summary>
		/// 获取一个游戏对象
		/// </summary>
		public SpawnHandle Spawn(Transform parent, Vector3 position, Quaternion rotation, bool forceClone, params System.Object[] userDatas)
		{
			InstantiateOperation operation;
			if (forceClone == false && _cacheOperations.Count > 0)
				operation = _cacheOperations.Dequeue();
			else
				operation = AssetHandle.InstantiateAsync();

			SpawnCount++;
			SpawnHandle handle = new SpawnHandle(this, operation, parent, position, rotation, userDatas);
			YooAssets.StartOperation(handle);
			return handle;
		}

		private void DestroyInstantiateOperation(InstantiateOperation operation)
		{
			// 取消异步操作
			operation.Cancel();

			// 销毁游戏对象
			if (operation.Result != null)
			{
				GameObject.Destroy(operation.Result);
			}
		}
		private void SetRestoreGameObject(GameObject gameObj)
		{
			if (gameObj != null)
			{
				gameObj.SetActive(false);
				gameObj.transform.SetParent(_root.transform);
				gameObj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			}
		}
	}
}