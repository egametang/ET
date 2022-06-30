using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
	[Serializable]
	internal sealed class CacheData
	{
		/// <summary>
		/// 缓存的APP内置版本
		/// </summary>
		public string CacheAppVersion = string.Empty;

		/// <summary>
		/// 读取缓存文件
		/// 注意：如果文件不存在则创建新的缓存文件
		/// </summary>
		public static CacheData LoadCache()
		{
			string filePath = GetCacheDataFilePath();
			if (File.Exists(filePath))
			{
				string jsonData = FileUtility.ReadFile(filePath);
				var cacheData = JsonUtility.FromJson<CacheData>(jsonData);
				YooLogger.Log($"Load cache data : {cacheData.CacheAppVersion}");
				return cacheData;
			}
			else
			{
				YooLogger.Log($"Create cache data : {Application.version}");
				CacheData cacheData = new CacheData();
				cacheData.CacheAppVersion = Application.version;
				string jsonData = JsonUtility.ToJson(cacheData);
				FileUtility.CreateFile(filePath, jsonData);
				return cacheData;
			}
		}

		/// <summary>
		/// 更新缓存文件
		/// </summary>
		public static void UpdateCache()
		{
			YooLogger.Log($"Update cache data to disk : {Application.version}");
			CacheData cacheData = new CacheData();
			cacheData.CacheAppVersion = Application.version;
			string filePath = GetCacheDataFilePath();
			string jsonData = JsonUtility.ToJson(cacheData);
			FileUtility.CreateFile(filePath, jsonData);
		}

		private static string GetCacheDataFilePath()
		{
			return PathHelper.MakePersistentLoadPath("CacheData.bytes");
		}
	}
}