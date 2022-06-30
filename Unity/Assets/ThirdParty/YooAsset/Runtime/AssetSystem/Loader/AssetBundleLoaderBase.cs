using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal abstract class AssetBundleLoaderBase
	{
		public enum EStatus
		{
			None = 0,
			Succeed,
			Failed
		}

		/// <summary>
		/// 资源包文件信息
		/// </summary>
		public BundleInfo MainBundleInfo { private set; get; }

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount { private set; get; }

		/// <summary>
		/// 加载状态
		/// </summary>
		public EStatus Status { protected set; get; }

		/// <summary>
		/// 最近的错误信息
		/// </summary>
		public string LastError { protected set; get; }

		/// <summary>
		/// 是否已经销毁
		/// </summary>
		public bool IsDestroyed { private set; get; } = false;

		private readonly List<ProviderBase> _providers = new List<ProviderBase>(100);
		internal AssetBundle CacheBundle { set; get; }


		public AssetBundleLoaderBase(BundleInfo bundleInfo)
		{
			MainBundleInfo = bundleInfo;
			RefCount = 0;
			Status = EStatus.None;
		}

		/// <summary>
		/// 添加附属的资源提供者
		/// </summary>
		public void AddProvider(ProviderBase provider)
		{
			if (_providers.Contains(provider) == false)
				_providers.Add(provider);
		}

		/// <summary>
		/// 引用（引用计数递加）
		/// </summary>
		public void Reference()
		{
			RefCount++;
		}

		/// <summary>
		/// 释放（引用计数递减）
		/// </summary>
		public void Release()
		{
			RefCount--;
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public abstract void Update();
	
		/// <summary>
		/// 销毁
		/// </summary>
		public void Destroy(bool forceDestroy)
		{
			IsDestroyed = true;

			// Check fatal
			if (forceDestroy == false)
			{
				if (RefCount > 0)
					throw new Exception($"Bundle file loader ref is not zero : {MainBundleInfo.BundleName}");
				if (IsDone() == false)
					throw new Exception($"Bundle file loader is not done : {MainBundleInfo.BundleName}");
			}

			if (CacheBundle != null)
			{
				CacheBundle.Unload(true);
				CacheBundle = null;
			}
		}

		/// <summary>
		/// 是否完毕（无论成功或失败）
		/// </summary>
		public bool IsDone()
		{
			return Status == EStatus.Succeed || Status == EStatus.Failed;
		}

		/// <summary>
		/// 是否可以销毁
		/// </summary>
		public bool CanDestroy()
		{
			if (IsDone() == false)
				return false;

			return RefCount <= 0;
		}

		/// <summary>
		/// 在满足条件的前提下，销毁所有资源提供者
		/// </summary>
		public void TryDestroyAllProviders()
		{
			if (IsDone() == false)
				return;

			// 注意：必须等待所有Provider可以销毁的时候，才可以释放Bundle文件。
			foreach (var provider in _providers)
			{
				if (provider.CanDestroy() == false)
					return;
			}

			// 除了自己没有其它引用
			if (RefCount > _providers.Count)
				return;

			// 销毁所有Providers
			foreach (var provider in _providers)
			{
				provider.Destroy();
			}

			// 从列表里移除Providers
			AssetSystem.RemoveBundleProviders(_providers);
			_providers.Clear();
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public abstract void WaitForAsyncComplete();
	}
}