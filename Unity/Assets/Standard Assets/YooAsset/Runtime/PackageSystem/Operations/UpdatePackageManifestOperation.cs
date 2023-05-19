﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	/// <summary>
	/// 向远端请求并更新清单
	/// </summary>
	public abstract class UpdatePackageManifestOperation : AsyncOperationBase
	{
		/// <summary>
		/// 保存当前清单的版本，用于下次启动时自动加载的版本。
		/// </summary>
		public virtual void SavePackageVersion() { }
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal sealed class EditorPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		public EditorPlayModeUpdatePackageManifestOperation()
		{
		}
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 离线模式的更新清单操作
	/// </summary>
	internal sealed class OfflinePlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		public OfflinePlayModeUpdatePackageManifestOperation()
		{
		}
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 联机模式的更新清单操作
	/// 注意：优先加载沙盒里缓存的清单文件，如果缓存没找到就下载远端清单文件，并保存到本地。
	/// </summary>
	internal sealed class HostPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		private enum ESteps
		{
			None,
			CheckActiveManifest,
			TryLoadCacheManifest,
			DownloadManifest,
			LoadCacheManifest,
			CheckDeserializeManifest,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private readonly bool _autoSaveVersion;
		private readonly int _timeout;
		private LoadCacheManifestOperation _tryLoadCacheManifestOp;
		private LoadCacheManifestOperation _loadCacheManifestOp;
		private DownloadManifestOperation _downloadManifestOp;
		private ESteps _steps = ESteps.None;


		internal HostPlayModeUpdatePackageManifestOperation(HostPlayModeImpl impl, string packageName, string packageVersion, bool autoSaveVersion, int timeout)
		{
			_impl = impl;
			_packageName = packageName;
			_packageVersion = packageVersion;
			_autoSaveVersion = autoSaveVersion;
			_timeout = timeout;
		}
		internal override void Start()
		{
			_steps = ESteps.CheckActiveManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.CheckActiveManifest)
			{
				// 检测当前激活的清单对象	
				if (_impl.ActiveManifest != null && _impl.ActiveManifest.PackageVersion == _packageVersion)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					_steps = ESteps.TryLoadCacheManifest;
				}
			}

			if (_steps == ESteps.TryLoadCacheManifest)
			{
				if (_tryLoadCacheManifestOp == null)
				{
					_tryLoadCacheManifestOp = new LoadCacheManifestOperation(_packageName, _packageVersion);
					OperationSystem.StartOperation(_tryLoadCacheManifestOp);
				}

				if (_tryLoadCacheManifestOp.IsDone == false)
					return;

				if (_tryLoadCacheManifestOp.Status == EOperationStatus.Succeed)
				{
					_impl.ActiveManifest = _tryLoadCacheManifestOp.Manifest;
					if (_autoSaveVersion)
						SavePackageVersion();
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					_steps = ESteps.DownloadManifest;
				}
			}

			if (_steps == ESteps.DownloadManifest)
			{
				if (_downloadManifestOp == null)
				{
					_downloadManifestOp = new DownloadManifestOperation(_impl, _packageName, _packageVersion, _timeout);
					OperationSystem.StartOperation(_downloadManifestOp);
				}

				if (_downloadManifestOp.IsDone == false)
					return;

				if (_downloadManifestOp.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.LoadCacheManifest;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloadManifestOp.Error;
				}
			}

			if (_steps == ESteps.LoadCacheManifest)
			{
				if (_loadCacheManifestOp == null)
				{
					_loadCacheManifestOp = new LoadCacheManifestOperation(_packageName, _packageVersion);
					OperationSystem.StartOperation(_loadCacheManifestOp);
				}

				if (_loadCacheManifestOp.IsDone == false)
					return;

				if (_loadCacheManifestOp.Status == EOperationStatus.Succeed)
				{
					_impl.ActiveManifest = _loadCacheManifestOp.Manifest;
					if (_autoSaveVersion)
						SavePackageVersion();
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _loadCacheManifestOp.Error;
				}
			}
		}

		public override void SavePackageVersion() 
		{
			_impl.FlushManifestVersionFile();
		}
	}
}