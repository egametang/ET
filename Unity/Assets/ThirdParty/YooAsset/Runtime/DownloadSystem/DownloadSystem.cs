using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	/// <summary>
	/// 1. 保证每一时刻资源文件只存在一个下载器
	/// 2. 保证下载器下载完成后立刻验证并缓存
	/// 3. 保证资源文件不会被重复下载
	/// </summary>
	internal static class DownloadSystem
	{
		private static readonly Dictionary<string, DownloaderBase> _downloaderDic = new Dictionary<string, DownloaderBase>();
		private static readonly List<string> _removeList = new List<string>(100);
		private static readonly Dictionary<string, string> _cachedHashList = new Dictionary<string, string>(1000);
		private static int _breakpointResumeFileSize = int.MaxValue;
		private static EVerifyLevel _verifyLevel = EVerifyLevel.High;


		/// <summary>
		/// 初始化
		/// </summary>
		public static void Initialize(int breakpointResumeFileSize, EVerifyLevel verifyLevel)
		{
			_breakpointResumeFileSize = breakpointResumeFileSize;
			_verifyLevel = verifyLevel;
		}

		/// <summary>
		/// 更新所有下载器
		/// </summary>
		public static void Update()
		{
			// 更新下载器
			_removeList.Clear();
			foreach (var valuePair in _downloaderDic)
			{
				var downloader = valuePair.Value;
				downloader.Update();
				if (downloader.IsDone())
					_removeList.Add(valuePair.Key);
			}

			// 移除下载器
			foreach (var key in _removeList)
			{
				_downloaderDic.Remove(key);
			}
		}

		/// <summary>
		/// 销毁所有下载器
		/// </summary>
		public static void DestroyAll()
		{
			foreach (var valuePair in _downloaderDic)
			{
				var downloader = valuePair.Value;
				downloader.Abort();
			}
			_downloaderDic.Clear();
			_removeList.Clear();
			_cachedHashList.Clear();
			_breakpointResumeFileSize = int.MaxValue;
		}


		/// <summary>
		/// 开始下载资源文件
		/// 注意：只有第一次请求的参数才是有效的
		/// </summary>
		public static DownloaderBase BeginDownload(BundleInfo bundleInfo, int failedTryAgain, int timeout = 60)
		{
			// 查询存在的下载器
			if (_downloaderDic.TryGetValue(bundleInfo.Hash, out var downloader))
			{
				return downloader;
			}

			// 如果资源已经缓存
			if (ContainsVerifyFile(bundleInfo.Hash))
			{
				var tempDownloader = new TempDownloader(bundleInfo);
				return tempDownloader;
			}

			// 创建新的下载器	
			{
				YooLogger.Log($"Beginning to download file : {bundleInfo.BundleName} URL : {bundleInfo.RemoteMainURL}");
				FileUtility.CreateFileDirectory(bundleInfo.GetCacheLoadPath());
				DownloaderBase newDownloader;
				if (bundleInfo.SizeBytes >= _breakpointResumeFileSize)
					newDownloader = new HttpDownloader(bundleInfo);
				else
					newDownloader = new FileDownloader(bundleInfo);
				newDownloader.SendRequest(failedTryAgain, timeout);
				_downloaderDic.Add(bundleInfo.Hash, newDownloader);
				return newDownloader;
			}
		}

		/// <summary>
		/// 获取下载器的总数
		/// </summary>
		public static int GetDownloaderTotalCount()
		{
			return _downloaderDic.Count;
		}

		/// <summary>
		/// 查询是否为验证文件
		/// 注意：被收录的文件完整性是绝对有效的
		/// </summary>
		public static bool ContainsVerifyFile(string hash)
		{
			if (_cachedHashList.ContainsKey(hash))
			{
				string filePath = SandboxHelper.MakeCacheFilePath(hash);
				if (File.Exists(filePath))
				{
					return true;
				}
				else
				{
					string bundleName = _cachedHashList[hash];
					_cachedHashList.Remove(hash);
					YooLogger.Error($"Cache file is missing : {bundleName} Hash : {hash}");
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 缓存验证过的文件
		/// </summary>
		public static void CacheVerifyFile(string hash, string bundleName)
		{
			if (_cachedHashList.ContainsKey(hash) == false)
			{
				YooLogger.Log($"Cache verify file : {bundleName} Hash : {hash}");
				_cachedHashList.Add(hash, bundleName);
			}
		}

		/// <summary>
		/// 验证文件完整性
		/// </summary>
		public static bool CheckContentIntegrity(string filePath, long size, string crc)
		{
			return CheckContentIntegrity(_verifyLevel, filePath, size, crc);
		}

		/// <summary>
		/// 验证文件完整性
		/// </summary>
		public static bool CheckContentIntegrity(EVerifyLevel verifyLevel, string filePath, long size, string crc)
		{
			try
			{
				if (File.Exists(filePath) == false)
					return false;

				// 先验证文件大小
				long fileSize = FileUtility.GetFileSize(filePath);
				if (fileSize != size)
					return false;

				// 再验证文件CRC
				if (verifyLevel == EVerifyLevel.High)
				{
					string fileCRC = HashUtility.FileCRC32(filePath);
					return fileCRC == crc;
				}
				else
				{
					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}