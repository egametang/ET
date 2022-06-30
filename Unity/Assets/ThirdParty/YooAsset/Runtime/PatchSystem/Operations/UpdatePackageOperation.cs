using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
	public abstract class UpdatePackageOperation : AsyncOperationBase
	{
		/// <summary>
		/// 创建包裹下载器
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public abstract PackageDownloaderOperation CreatePackageDownloader(int downloadingMaxNumber, int failedTryAgain);
	}

	/// <summary>
	/// 编辑器下模拟运行的更新资源包裹操作
	/// </summary>
	internal sealed class EditorPlayModeUpdatePackageOperation : UpdatePackageOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}

		/// <summary>
		/// 创建包裹下载器
		/// </summary>
		public override PackageDownloaderOperation CreatePackageDownloader(int downloadingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> downloadList = new List<BundleInfo>();
			var operation = new PackageDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
			return operation;
		}
	}

	/// <summary>
	/// 离线模式的更新资源包裹操作
	/// </summary>
	internal sealed class OfflinePlayModeUpdatePackageOperation : UpdatePackageOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}

		/// <summary>
		/// 创建包裹下载器
		/// </summary>
		public override PackageDownloaderOperation CreatePackageDownloader(int downloadingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> downloadList = new List<BundleInfo>();
			var operation = new PackageDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
			return operation;
		}
	}

	/// <summary>
	/// 网络模式的更新资源包裹操作
	/// </summary>
	internal sealed class HostPlayModeUpdatePackageOperation : UpdatePackageOperation
	{
		private enum ESteps
		{
			None,
			LoadWebManifest,
			CheckWebManifest,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly int _resourceVersion;
		private readonly int _timeout;
		private ESteps _steps = ESteps.None;
		private UnityWebDataRequester _downloader;
		private PatchManifest _remotePatchManifest;

		internal HostPlayModeUpdatePackageOperation(HostPlayModeImpl impl, int resourceVersion, int timeout)
		{
			_impl = impl;
			_resourceVersion = resourceVersion;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.LoadWebManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadWebManifest)
			{
				string webURL = GetPatchManifestRequestURL(YooAssetSettingsData.GetPatchManifestFileName(_resourceVersion));
				YooLogger.Log($"Beginning to request patch manifest : {webURL}");
				_downloader = new UnityWebDataRequester();
				_downloader.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckWebManifest;
			}

			if (_steps == ESteps.CheckWebManifest)
			{
				Progress = _downloader.Progress();
				if (_downloader.IsDone() == false)
					return;

				// Check error
				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader.GetError();
				}
				else
				{
					// 解析补丁清单			
					if (ParseRemotePatchManifest(_downloader.GetText()))
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"URL : {_downloader.URL} Error : remote patch manifest content is invalid";
					}
				}
				_downloader.Dispose();
			}
		}

		/// <summary>
		/// 创建包裹下载器
		/// </summary>
		public override PackageDownloaderOperation CreatePackageDownloader(int downloadingMaxNumber, int failedTryAgain)
		{
			if (Status == EOperationStatus.Succeed)
			{
				List<BundleInfo> downloadList = GetDownloadList();
				var operation = new PackageDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else
			{
				YooLogger.Error($"{nameof(UpdatePackageOperation)} status is failed !");
				var operation = new PackageDownloaderOperation(null, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
		}

		/// <summary>
		/// 获取补丁清单请求地址
		/// </summary>
		private string GetPatchManifestRequestURL(string fileName)
		{
			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				return _impl.GetPatchDownloadFallbackURL(fileName);
			else
				return _impl.GetPatchDownloadMainURL(fileName);
		}

		/// <summary>
		/// 解析远端请求的补丁清单
		/// </summary>
		private bool ParseRemotePatchManifest(string content)
		{
			try
			{
				_remotePatchManifest = PatchManifest.Deserialize(content);
				return true;
			}
			catch (Exception e)
			{
				YooLogger.Warning(e.ToString());
				return false;
			}
		}

		/// <summary>
		/// 获取下载列表
		/// </summary>
		private List<BundleInfo> GetDownloadList()
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in _remotePatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (_impl.AppPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
						continue;
				}

				// 注意：通过比对文件大小做快速的文件校验！
				// 注意：在初始化的时候会去做最终校验！
				string filePath = SandboxHelper.MakeCacheFilePath(patchBundle.Hash);
				if (File.Exists(filePath))
				{
					long fileSize = FileUtility.GetFileSize(filePath);
					if (fileSize == patchBundle.SizeBytes)
						continue;
				}

				downloadList.Add(patchBundle);
			}

			return _impl.ConvertToDownloadList(downloadList);
		}
	}
}