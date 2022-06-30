using UnityEngine;
using System.Collections.Generic;

namespace YooAsset
{
	public sealed class AssetOperationHandle : OperationHandleBase
	{
		private System.Action<AssetOperationHandle> _callback;

		internal AssetOperationHandle(ProviderBase provider) : base(provider)
		{
		}
		internal override void InvokeCallback()
		{
			_callback?.Invoke(this);
		}

		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<AssetOperationHandle> Completed
		{
			add
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(AssetOperationHandle)} is invalid");
				if (Provider.IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(AssetOperationHandle)} is invalid");
				_callback -= value;
			}
		}

		/// <summary>
		/// 资源对象
		/// </summary>
		public UnityEngine.Object AssetObject
		{
			get
			{
				if (IsValid == false)
					return null;
				return Provider.AssetObject;
			}
		}

		/// <summary>
		/// 等待异步执行完毕
		/// </summary>
		public void WaitForAsyncComplete()
		{
			if (IsValid == false)
				return;
			Provider.WaitForAsyncComplete();
		}

		/// <summary>
		/// 释放资源句柄
		/// </summary>
		public void Release()
		{
			this.ReleaseInternal();
		}


		/// <summary>
		/// 同步初始化游戏对象
		/// </summary>
		/// <param name="parent">父类对象</param>
		/// <returns></returns>
		public GameObject InstantiateSync(Transform parent = null)
		{
			return InstantiateSyncInternal(Vector3.zero, Quaternion.identity, parent, false);
		}

		/// <summary>
		/// 同步初始化游戏对象
		/// </summary>
		/// <param name="position">坐标</param>
		/// <param name="rotation">角度</param>
		/// <param name="parent">父类对象</param>
		public GameObject InstantiateSync(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			return InstantiateSyncInternal(position, rotation, parent, true);
		}

		/// <summary>
		/// 异步初始化游戏对象
		/// </summary>
		/// <param name="parent">父类对象</param>
		public InstantiateOperation InstantiateAsync(Transform parent = null)
		{
			return InstantiateAsyncInternal(Vector3.zero, Quaternion.identity, parent, false);
		}

		/// <summary>
		/// 异步初始化游戏对象
		/// </summary>
		/// <param name="position">坐标</param>
		/// <param name="rotation">角度</param>
		/// <param name="parent">父类对象</param>
		public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			return InstantiateAsyncInternal(position, rotation, parent, true);
		}


		private GameObject InstantiateSyncInternal(Vector3 position, Quaternion rotation, Transform parent, bool setPositionRotation)
		{
			if (IsValid == false)
				return null;
			if (Provider.AssetObject == null)
				return null;

			GameObject result;
			if (setPositionRotation)
			{
				if (parent == null)
					result = UnityEngine.Object.Instantiate(Provider.AssetObject as GameObject, position, rotation);
				else
					result = UnityEngine.Object.Instantiate(Provider.AssetObject as GameObject, position, rotation, parent);
			}
			else
			{
				if (parent == null)
					result = UnityEngine.Object.Instantiate(Provider.AssetObject as GameObject);
				else
					result = UnityEngine.Object.Instantiate(Provider.AssetObject as GameObject, parent);
			}
			return result;
		}
		private InstantiateOperation InstantiateAsyncInternal(Vector3 position, Quaternion rotation, Transform parent, bool setPositionRotation)
		{
			InstantiateOperation operation = new InstantiateOperation(this, position, rotation, parent, setPositionRotation);
			OperationSystem.StartOperaiton(operation);
			return operation;
		}
	}
}