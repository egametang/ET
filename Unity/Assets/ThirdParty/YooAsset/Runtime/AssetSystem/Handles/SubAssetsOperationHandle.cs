using System.Collections.Generic;

namespace YooAsset
{
	public sealed class SubAssetsOperationHandle : OperationHandleBase
	{
		private System.Action<SubAssetsOperationHandle> _callback;

		internal SubAssetsOperationHandle(ProviderBase provider) : base(provider)
		{
		}
		internal override void InvokeCallback()
		{
			_callback?.Invoke(this);
		}

		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<SubAssetsOperationHandle> Completed
		{
			add
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(SubAssetsOperationHandle)} is invalid");
				if (Provider.IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(SubAssetsOperationHandle)} is invalid");
				_callback -= value;
			}
		}

		/// <summary>
		/// 子资源对象集合
		/// </summary>
		public UnityEngine.Object[] AllAssetObjects
		{
			get
			{
				if (IsValid == false)
					return null;
				return Provider.AllAssetObjects;
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
		/// 获取子资源对象
		/// </summary>
		/// <typeparam name="TObject">子资源对象类型</typeparam>
		/// <param name="assetName">子资源对象名称</param>
		public TObject GetSubAssetObject<TObject>(string assetName) where TObject : UnityEngine.Object
		{
			if (IsValid == false)
				return null;

			foreach (var assetObject in Provider.AllAssetObjects)
			{
				if (assetObject.name == assetName)
					return assetObject as TObject;
			}

			YooLogger.Warning($"Not found sub asset object : {assetName}");
			return null;
		}

		/// <summary>
		/// 获取所有的子资源对象集合
		/// </summary>
		/// <typeparam name="TObject">子资源对象类型</typeparam>
		public TObject[] GetSubAssetObjects<TObject>() where TObject : UnityEngine.Object
		{
			if (IsValid == false)
				return null;

			List<TObject> ret = new List<TObject>(Provider.AllAssetObjects.Length);
			foreach (var assetObject in Provider.AllAssetObjects)
			{
				var retObject = assetObject as TObject;
				if (retObject != null)
					ret.Add(retObject);
				else
					YooLogger.Warning($"The type conversion failed : {assetObject.name}");
			}
			return ret.ToArray();
		}
	}
}