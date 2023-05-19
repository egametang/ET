using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 初始化操作
	/// </summary>
	public abstract class InitializationOperation : AsyncOperationBase
	{
		public string PackageVersion { protected set; get; }
	}

	/// <summary>
	/// 编辑器下模拟模式的初始化操作
	/// </summary>
	internal sealed class EditorSimulateModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			LoadEditorManifest,
			Done,
		}

		private readonly EditorSimulateModeImpl _impl;
		private readonly string _simulateManifestFilePath;
		private LoadEditorManifestOperation _loadEditorManifestOp;
		private ESteps _steps = ESteps.None;

		internal EditorSimulateModeInitializationOperation(EditorSimulateModeImpl impl, string simulateManifestFilePath)
		{
			_impl = impl;
			_simulateManifestFilePath = simulateManifestFilePath;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadEditorManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.LoadEditorManifest)
			{
				if (_loadEditorManifestOp == null)
				{
					_loadEditorManifestOp = new LoadEditorManifestOperation(_simulateManifestFilePath);
					OperationSystem.StartOperation(_loadEditorManifestOp);
				}

				if (_loadEditorManifestOp.IsDone == false)
					return;

				if (_loadEditorManifestOp.Status == EOperationStatus.Succeed)
				{
					PackageVersion = _loadEditorManifestOp.Manifest.PackageVersion;
					_impl.ActiveManifest = _loadEditorManifestOp.Manifest;
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _loadEditorManifestOp.Error;
				}
			}
		}
	}

	/// <summary>
	/// 离线运行模式的初始化操作
	/// </summary>
	internal sealed class OfflinePlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			QueryBuildinPackageVersion,
			LoadBuildinManifest,
			PackageCaching,
			Done,
		}

		private readonly OfflinePlayModeImpl _impl;
		private readonly string _packageName;
		private QueryBuildinPackageVersionOperation _queryBuildinPackageVersionOp;
		private LoadBuildinManifestOperation _loadBuildinManifestOp;
		private PackageCachingOperation _cachingOperation;
		private ESteps _steps = ESteps.None;

		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.QueryBuildinPackageVersion;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.QueryBuildinPackageVersion)
			{
				if (_queryBuildinPackageVersionOp == null)
				{
					_queryBuildinPackageVersionOp = new QueryBuildinPackageVersionOperation(_packageName);
					OperationSystem.StartOperation(_queryBuildinPackageVersionOp);
				}

				if (_queryBuildinPackageVersionOp.IsDone == false)
					return;

				if (_queryBuildinPackageVersionOp.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.LoadBuildinManifest;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _queryBuildinPackageVersionOp.Error;
				}
			}

			if (_steps == ESteps.LoadBuildinManifest)
			{
				if (_loadBuildinManifestOp == null)
				{
					_loadBuildinManifestOp = new LoadBuildinManifestOperation(_packageName, _queryBuildinPackageVersionOp.PackageVersion);
					OperationSystem.StartOperation(_loadBuildinManifestOp);
				}

				Progress = _loadBuildinManifestOp.Progress;
				if (_loadBuildinManifestOp.IsDone == false)
					return;

				if (_loadBuildinManifestOp.Status == EOperationStatus.Succeed)
				{
					PackageVersion = _loadBuildinManifestOp.Manifest.PackageVersion;
					_impl.ActiveManifest = _loadBuildinManifestOp.Manifest;
					_steps = ESteps.PackageCaching;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _loadBuildinManifestOp.Error;
				}
			}

			if (_steps == ESteps.PackageCaching)
			{
				if (_cachingOperation == null)
				{
					_cachingOperation = new PackageCachingOperation(_packageName);
					OperationSystem.StartOperation(_cachingOperation);
				}

				Progress = _cachingOperation.Progress;
				if (_cachingOperation.IsDone)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
			}
		}
	}

	/// <summary>
	/// 联机运行模式的初始化操作
	/// 注意：优先从沙盒里加载清单，如果沙盒里不存在就尝试把内置清单拷贝到沙盒并加载该清单。
	/// </summary>
	internal sealed class HostPlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			CheckAppFootPrint,
			QueryCachePackageVersion,
			TryLoadCacheManifest,
			QueryBuildinPackageVersion,
			UnpackBuildinManifest,
			LoadBuildinManifest,
			PackageCaching,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private QueryBuildinPackageVersionOperation _queryBuildinPackageVersionOp;
		private QueryCachePackageVersionOperation _queryCachePackageVersionOp;
		private UnpackBuildinManifestOperation _unpackBuildinManifestOp;
		private LoadBuildinManifestOperation _loadBuildinManifestOp;
		private LoadCacheManifestOperation _loadCacheManifestOp;
		private PackageCachingOperation _cachingOperation;
		private ESteps _steps = ESteps.None;

		internal HostPlayModeInitializationOperation(HostPlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.CheckAppFootPrint;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.CheckAppFootPrint)
			{
				var appFootPrint = new AppFootPrint();
				appFootPrint.Load();

				// 如果水印发生变化，则说明覆盖安装后首次打开游戏
				if (appFootPrint.IsDirty())
				{
					PersistentTools.DeleteManifestFolder();
					appFootPrint.Coverage();
					YooLogger.Log("Delete manifest files when application foot print dirty !");
				}
				_steps = ESteps.QueryCachePackageVersion;
			}

			if (_steps == ESteps.QueryCachePackageVersion)
			{
				if (_queryCachePackageVersionOp == null)
				{
					_queryCachePackageVersionOp = new QueryCachePackageVersionOperation(_packageName);
					OperationSystem.StartOperation(_queryCachePackageVersionOp);
				}

				if (_queryCachePackageVersionOp.IsDone == false)
					return;

				if (_queryCachePackageVersionOp.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.TryLoadCacheManifest;
				}
				else
				{
					_steps = ESteps.QueryBuildinPackageVersion;
				}
			}

			if (_steps == ESteps.TryLoadCacheManifest)
			{
				if (_loadCacheManifestOp == null)
				{
					_loadCacheManifestOp = new LoadCacheManifestOperation(_packageName, _queryCachePackageVersionOp.PackageVersion);
					OperationSystem.StartOperation(_loadCacheManifestOp);
				}

				if (_loadCacheManifestOp.IsDone == false)
					return;

				if (_loadCacheManifestOp.Status == EOperationStatus.Succeed)
				{
					PackageVersion = _loadCacheManifestOp.Manifest.PackageVersion;
					_impl.ActiveManifest = _loadCacheManifestOp.Manifest;
					_steps = ESteps.PackageCaching;
				}
				else
				{
					_steps = ESteps.QueryBuildinPackageVersion;
				}
			}

			if (_steps == ESteps.QueryBuildinPackageVersion)
			{
				if (_queryBuildinPackageVersionOp == null)
				{
					_queryBuildinPackageVersionOp = new QueryBuildinPackageVersionOperation(_packageName);
					OperationSystem.StartOperation(_queryBuildinPackageVersionOp);
				}

				if (_queryBuildinPackageVersionOp.IsDone == false)
					return;

				if (_queryBuildinPackageVersionOp.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.UnpackBuildinManifest;
				}
				else
				{
					// 注意：为了兼容MOD模式，初始化动态新增的包裹的时候，如果内置清单不存在也不需要报错！
					_steps = ESteps.PackageCaching;
					string error = _queryBuildinPackageVersionOp.Error;
					YooLogger.Log($"Failed to load buildin package version file : {error}");
				}
			}

			if (_steps == ESteps.UnpackBuildinManifest)
			{
				if (_unpackBuildinManifestOp == null)
				{
					_unpackBuildinManifestOp = new UnpackBuildinManifestOperation(_packageName, _queryBuildinPackageVersionOp.PackageVersion);
					OperationSystem.StartOperation(_unpackBuildinManifestOp);
				}

				if (_unpackBuildinManifestOp.IsDone == false)
					return;

				if (_unpackBuildinManifestOp.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.LoadBuildinManifest;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _unpackBuildinManifestOp.Error;
				}
			}

			if (_steps == ESteps.LoadBuildinManifest)
			{
				if (_loadBuildinManifestOp == null)
				{
					_loadBuildinManifestOp = new LoadBuildinManifestOperation(_packageName, _queryBuildinPackageVersionOp.PackageVersion);
					OperationSystem.StartOperation(_loadBuildinManifestOp);
				}

				Progress = _loadBuildinManifestOp.Progress;
				if (_loadBuildinManifestOp.IsDone == false)
					return;

				if (_loadBuildinManifestOp.Status == EOperationStatus.Succeed)
				{
					PackageVersion = _loadBuildinManifestOp.Manifest.PackageVersion;
					_impl.ActiveManifest = _loadBuildinManifestOp.Manifest;
					_steps = ESteps.PackageCaching;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _loadBuildinManifestOp.Error;
				}
			}

			if (_steps == ESteps.PackageCaching)
			{
				if (_cachingOperation == null)
				{
					_cachingOperation = new PackageCachingOperation(_packageName);
					OperationSystem.StartOperation(_cachingOperation);
				}

				Progress = _cachingOperation.Progress;
				if (_cachingOperation.IsDone)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
			}
		}
	}

	/// <summary>
	/// 应用程序水印
	/// </summary>
	internal class AppFootPrint
	{
		private string _footPrint;

		/// <summary>
		/// 读取应用程序水印
		/// </summary>
		public void Load()
		{
			string footPrintFilePath = PersistentTools.GetAppFootPrintFilePath();
			if (File.Exists(footPrintFilePath))
			{
				_footPrint = FileUtility.ReadAllText(footPrintFilePath);
			}
			else
			{
				Coverage();
			}
		}

		/// <summary>
		/// 检测水印是否发生变化
		/// </summary>
		public bool IsDirty()
		{
#if UNITY_EDITOR
			return _footPrint != Application.version;
#else
			return _footPrint != Application.buildGUID;
#endif
		}

		/// <summary>
		/// 覆盖掉水印
		/// </summary>
		public void Coverage()
		{
#if UNITY_EDITOR
			_footPrint = Application.version;
#else
			_footPrint = Application.buildGUID;
#endif
			string footPrintFilePath = PersistentTools.GetAppFootPrintFilePath();
			FileUtility.CreateFile(footPrintFilePath, _footPrint);
			YooLogger.Log($"Save application foot print : {_footPrint}");
		}
	}
}