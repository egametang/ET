using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class AssetBundleFileLoader : BundleLoaderBase
	{
		private enum ESteps
		{
			None = 0,
			Download,
			CheckDownload,
			Unpack,
			CheckUnpack,
			LoadFile,
			CheckLoadFile,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private bool _isWaitForAsyncComplete = false;
		private bool _isShowWaitForAsyncError = false;
		private DownloaderBase _unpacker;
		private DownloaderBase _downloader;
		private AssetBundleCreateRequest _createRequest;
		private Stream _stream;


		public AssetBundleFileLoader(AssetSystemImpl impl, BundleInfo bundleInfo) : base(impl, bundleInfo)
		{
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public override void Update()
		{
			if (_steps == ESteps.Done)
				return;

			if (_steps == ESteps.None)
			{
				if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				{
					_steps = ESteps.Download;
					FileLoadPath = MainBundleInfo.Bundle.CachedDataFilePath;
				}
				else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
				{
#if UNITY_ANDROID
					EBundleLoadMethod loadMethod = (EBundleLoadMethod)MainBundleInfo.Bundle.LoadMethod;
					if (loadMethod == EBundleLoadMethod.LoadFromMemory || loadMethod == EBundleLoadMethod.LoadFromStream)
					{
						_steps = ESteps.Unpack;
						FileLoadPath = MainBundleInfo.Bundle.CachedDataFilePath;
					}
					else
					{
						_steps = ESteps.LoadFile;
						FileLoadPath = MainBundleInfo.Bundle.StreamingFilePath;
					}
#else
					_steps = ESteps.LoadFile;
					FileLoadPath = MainBundleInfo.Bundle.StreamingFilePath;
#endif
				}
				else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
				{
					_steps = ESteps.LoadFile;
					FileLoadPath = MainBundleInfo.Bundle.CachedDataFilePath;
				}
				else
				{
					throw new System.NotImplementedException(MainBundleInfo.LoadMode.ToString());
				}
			}

			// 1. 从服务器下载
			if (_steps == ESteps.Download)
			{
				int failedTryAgain = Impl.DownloadFailedTryAgain;
				_downloader = DownloadSystem.BeginDownload(MainBundleInfo, failedTryAgain);
				_steps = ESteps.CheckDownload;
			}

			// 2. 检测服务器下载结果
			if (_steps == ESteps.CheckDownload)
			{
				DownloadProgress = _downloader.DownloadProgress;
				DownloadedBytes = _downloader.DownloadedBytes;
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = _downloader.GetLastError();
				}
				else
				{
					_steps = ESteps.LoadFile;
					return; //下载完毕等待一帧再去加载！
				}
			}

			// 3. 内置文件解压
			if (_steps == ESteps.Unpack)
			{
				int failedTryAgain = Impl.DownloadFailedTryAgain;
				var bundleInfo = ManifestTools.ConvertToUnpackInfo(MainBundleInfo.Bundle);
				_unpacker = DownloadSystem.BeginDownload(bundleInfo, failedTryAgain);
				_steps = ESteps.CheckUnpack;
			}

			// 4.检测内置文件解压结果
			if (_steps == ESteps.CheckUnpack)
			{
				DownloadProgress = _unpacker.DownloadProgress;
				DownloadedBytes = _unpacker.DownloadedBytes;
				if (_unpacker.IsDone() == false)
					return;

				if (_unpacker.HasError())
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = _unpacker.GetLastError();
				}
				else
				{
					_steps = ESteps.LoadFile;
				}
			}

			// 5. 加载AssetBundle
			if (_steps == ESteps.LoadFile)
			{
#if UNITY_EDITOR
				// 注意：Unity2017.4编辑器模式下，如果AssetBundle文件不存在会导致编辑器崩溃，这里做了预判。
				if (System.IO.File.Exists(FileLoadPath) == false)
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = $"Not found assetBundle file : {FileLoadPath}";
					YooLogger.Error(LastError);
					return;
				}
#endif

				// 设置下载进度
				DownloadProgress = 1f;
				DownloadedBytes = (ulong)MainBundleInfo.Bundle.FileSize;

				// Load assetBundle file
				var loadMethod = (EBundleLoadMethod)MainBundleInfo.Bundle.LoadMethod;
				if (loadMethod == EBundleLoadMethod.Normal)
				{
					if (_isWaitForAsyncComplete)
						CacheBundle = AssetBundle.LoadFromFile(FileLoadPath);
					else
						_createRequest = AssetBundle.LoadFromFileAsync(FileLoadPath);
				}
				else
				{
					if (Impl.DecryptionServices == null)
					{
						_steps = ESteps.Done;
						Status = EStatus.Failed;
						LastError = $"{nameof(IDecryptionServices)} is null : {MainBundleInfo.Bundle.BundleName}";
						YooLogger.Error(LastError);
						return;
					}

					DecryptFileInfo fileInfo = new DecryptFileInfo();
					fileInfo.BundleName = MainBundleInfo.Bundle.BundleName;
					fileInfo.FilePath = FileLoadPath;

					if (loadMethod == EBundleLoadMethod.LoadFromFileOffset)
					{
						ulong offset = Impl.DecryptionServices.LoadFromFileOffset(fileInfo);
						if (_isWaitForAsyncComplete)
							CacheBundle = AssetBundle.LoadFromFile(FileLoadPath, 0, offset);
						else
							_createRequest = AssetBundle.LoadFromFileAsync(FileLoadPath, 0, offset);
					}
					else if (loadMethod == EBundleLoadMethod.LoadFromMemory)
					{
						byte[] fileData = Impl.DecryptionServices.LoadFromMemory(fileInfo);
						if (_isWaitForAsyncComplete)
							CacheBundle = AssetBundle.LoadFromMemory(fileData);
						else
							_createRequest = AssetBundle.LoadFromMemoryAsync(fileData);
					}
					else if (loadMethod == EBundleLoadMethod.LoadFromStream)
					{
						_stream = Impl.DecryptionServices.LoadFromStream(fileInfo);
						uint managedReadBufferSize = Impl.DecryptionServices.GetManagedReadBufferSize();
						if (_isWaitForAsyncComplete)
							CacheBundle = AssetBundle.LoadFromStream(_stream, 0, managedReadBufferSize);
						else
							_createRequest = AssetBundle.LoadFromStreamAsync(_stream, 0, managedReadBufferSize);
					}
					else
					{
						throw new System.NotImplementedException();
					}
				}
				_steps = ESteps.CheckLoadFile;
			}

			// 6. 检测AssetBundle加载结果
			if (_steps == ESteps.CheckLoadFile)
			{
				if (_createRequest != null)
				{
					if (_isWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						YooLogger.Warning("Suspend the main thread to load unity bundle.");
						CacheBundle = _createRequest.assetBundle;
					}
					else
					{
						if (_createRequest.isDone == false)
							return;
						CacheBundle = _createRequest.assetBundle;
					}
				}

				// Check error			
				if (CacheBundle == null)
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = $"Failed to load assetBundle : {MainBundleInfo.Bundle.BundleName}";
					YooLogger.Error(LastError);

					// 注意：当缓存文件的校验等级为Low的时候，并不能保证缓存文件的完整性。
					// 在AssetBundle文件加载失败的情况下，我们需要重新验证文件的完整性！
					if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
					{
						var result = CacheSystem.VerifyingRecordFile(MainBundleInfo.Bundle.PackageName, MainBundleInfo.Bundle.CacheGUID);
						if (result != EVerifyResult.Succeed)
						{
							YooLogger.Error($"Found possibly corrupt file ! {MainBundleInfo.Bundle.CacheGUID} Verify result : {result}");
							CacheSystem.DiscardFile(MainBundleInfo.Bundle.PackageName, MainBundleInfo.Bundle.CacheGUID);
						}
					}
				}
				else
				{
					_steps = ESteps.Done;
					Status = EStatus.Succeed;
				}
			}
		}

		/// <summary>
		/// 销毁
		/// </summary>
		public override void Destroy(bool forceDestroy)
		{
			base.Destroy(forceDestroy);

			if (_stream != null)
			{
				_stream.Close();
				_stream.Dispose();
				_stream = null;
			}
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public override void WaitForAsyncComplete()
		{
			_isWaitForAsyncComplete = true;

			int frame = 1000;
			while (true)
			{
				// 文件解压
				if (_unpacker != null)
				{
					if (_unpacker.IsDone() == false)
					{
						_unpacker.WaitForAsyncComplete = true;
						_unpacker.Update();
						continue;
					}
				}

				// 保险机制
				// 注意：如果需要从WEB端下载资源，可能会触发保险机制！
				frame--;
				if (frame == 0)
				{
					if (_isShowWaitForAsyncError == false)
					{
						_isShowWaitForAsyncError = true;
						YooLogger.Error($"{nameof(WaitForAsyncComplete)} failed ! Try load bundle : {MainBundleInfo.Bundle.BundleName} from remote with sync load method !");
					}
					break;
				}

				// 驱动流程
				Update();

				// 完成后退出
				if (IsDone())
					break;
			}
		}
	}
}