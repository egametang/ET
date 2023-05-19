using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset
{
	internal static class CacheSystem
	{
		private readonly static Dictionary<string, PackageCache> _cachedDic = new Dictionary<string, PackageCache>(1000);

		/// <summary>
		/// 初始化时的验证级别
		/// </summary>
		public static EVerifyLevel InitVerifyLevel { set; get; } = EVerifyLevel.Middle;

		/// <summary>
		/// 清空所有数据
		/// </summary>
		public static void ClearAll()
		{
			_cachedDic.Clear();
		}

		/// <summary>
		/// 清空指定包裹的所有缓存数据
		/// </summary>
		public static void ClearPackage(string packageName)
		{
			var cache = GetOrCreateCache(packageName);
			cache.ClearAll();
		}

		/// <summary>
		/// 获取缓存文件总数
		/// </summary>
		public static int GetCachedFilesCount(string packageName)
		{
			var cache = GetOrCreateCache(packageName);
			return cache.GetCachedFilesCount();
		}

		/// <summary>
		/// 查询是否为验证文件
		/// </summary>
		public static bool IsCached(string packageName, string cacheGUID)
		{
			var cache = GetOrCreateCache(packageName);
			return cache.IsCached(cacheGUID);
		}

		/// <summary>
		/// 录入验证的文件
		/// </summary>
		public static void RecordFile(string packageName, string cacheGUID, PackageCache.RecordWrapper wrapper)
		{
			//YooLogger.Log($"Record file : {packageName} = {cacheGUID}");
			var cache = GetOrCreateCache(packageName);
			cache.Record(cacheGUID, wrapper);
		}

		/// <summary>
		/// 丢弃验证的文件（同时删除文件）
		/// </summary>
		public static void DiscardFile(string packageName, string cacheGUID)
		{
			var cache = GetOrCreateCache(packageName);
			var wrapper = cache.TryGetWrapper(cacheGUID);
			if (wrapper == null)
				return;

			cache.Discard(cacheGUID);

			try
			{
				string dataFilePath = wrapper.DataFilePath;
				FileInfo fileInfo = new FileInfo(dataFilePath);
				if (fileInfo.Exists)
					fileInfo.Directory.Delete(true);
			}
			catch (Exception e)
			{
				YooLogger.Error($"Failed to delete cache file ! {e.Message}");
			}
		}

		/// <summary>
		/// 验证缓存文件（子线程内操作）
		/// </summary>
		public static EVerifyResult VerifyingCacheFile(VerifyCacheElement element)
		{
			try
			{
				if (InitVerifyLevel == EVerifyLevel.Low)
				{
					if (File.Exists(element.InfoFilePath) == false)
						return EVerifyResult.InfoFileNotExisted;
					if (File.Exists(element.DataFilePath) == false)
						return EVerifyResult.DataFileNotExisted;
					return EVerifyResult.Succeed;
				}
				else
				{
					if (File.Exists(element.InfoFilePath) == false)
						return EVerifyResult.InfoFileNotExisted;

					// 解析信息文件获取验证数据
					CacheFileInfo.ReadInfoFromFile(element.InfoFilePath, out element.DataFileCRC, out element.DataFileSize);
				}
			}
			catch (Exception)
			{
				return EVerifyResult.Exception;
			}

			return VerifyingInternal(element.DataFilePath, element.DataFileSize, element.DataFileCRC, InitVerifyLevel);
		}

		/// <summary>
		/// 验证下载文件（子线程内操作）
		/// </summary>
		public static EVerifyResult VerifyingTempFile(VerifyTempElement element)
		{
			return VerifyingInternal(element.TempDataFilePath, element.FileSize, element.FileCRC, EVerifyLevel.High);
		}

		/// <summary>
		/// 验证记录文件（主线程内操作）
		/// </summary>
		public static EVerifyResult VerifyingRecordFile(string packageName, string cacheGUID)
		{
			var cache = GetOrCreateCache(packageName);
			var wrapper = cache.TryGetWrapper(cacheGUID);
			if (wrapper == null)
				return EVerifyResult.CacheNotFound;

			EVerifyResult result = VerifyingInternal(wrapper.DataFilePath, wrapper.DataFileSize, wrapper.DataFileCRC, EVerifyLevel.High);
			return result;
		}

		/// <summary>
		/// 获取未被使用的缓存文件
		/// </summary>
		public static List<string> GetUnusedCacheGUIDs(ResourcePackage package)
		{
			var cache = GetOrCreateCache(package.PackageName);
			var keys = cache.GetAllKeys();
			List<string> result = new List<string>(keys.Count);
			foreach (var cacheGUID in keys)
			{
				if (package.IsIncludeBundleFile(cacheGUID) == false)
				{
					result.Add(cacheGUID);
				}
			}
			return result;
		}

		/// <summary>
		/// 获取所有的缓存文件
		/// </summary>
		public static List<string> GetAllCacheGUIDs(ResourcePackage package)
		{
			var cache = GetOrCreateCache(package.PackageName);
			return cache.GetAllKeys();
		}

		private static EVerifyResult VerifyingInternal(string filePath, long fileSize, string fileCRC, EVerifyLevel verifyLevel)
		{
			try
			{
				if (File.Exists(filePath) == false)
					return EVerifyResult.DataFileNotExisted;

				// 先验证文件大小
				long size = FileUtility.GetFileSize(filePath);
				if (size < fileSize)
					return EVerifyResult.FileNotComplete;
				else if (size > fileSize)
					return EVerifyResult.FileOverflow;

				// 再验证文件CRC
				if (verifyLevel == EVerifyLevel.High)
				{
					string crc = HashUtility.FileCRC32(filePath);
					if (crc == fileCRC)
						return EVerifyResult.Succeed;
					else
						return EVerifyResult.FileCrcError;
				}
				else
				{
					return EVerifyResult.Succeed;
				}
			}
			catch (Exception)
			{
				return EVerifyResult.Exception;
			}
		}
		private static PackageCache GetOrCreateCache(string packageName)
		{
			if (_cachedDic.TryGetValue(packageName, out PackageCache cache) == false)
			{
				cache = new PackageCache(packageName);
				_cachedDic.Add(packageName, cache);
			}
			return cache;
		}
	}
}