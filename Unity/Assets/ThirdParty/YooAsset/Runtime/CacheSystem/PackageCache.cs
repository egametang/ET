using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class PackageCache
	{
		internal class RecordWrapper
		{
			public string InfoFilePath { private set; get; }
			public string DataFilePath { private set; get; }
			public string DataFileCRC { private set; get; }
			public long DataFileSize { private set; get; }

			public RecordWrapper(string infoFilePath, string dataFilePath, string dataFileCRC, long dataFileSize)
			{
				InfoFilePath = infoFilePath;
				DataFilePath = dataFilePath;
				DataFileCRC = dataFileCRC;
				DataFileSize = dataFileSize;
			}
		}

		private readonly Dictionary<string, RecordWrapper> _wrappers = new Dictionary<string, RecordWrapper>();

		/// <summary>
		/// 包裹名称
		/// </summary>
		public string PackageName { private set; get; }


		public PackageCache(string packageName)
		{
			PackageName = packageName;
		}

		/// <summary>
		/// 清空所有数据
		/// </summary>
		public void ClearAll()
		{
			_wrappers.Clear();
		}

		/// <summary>
		/// 获取缓存文件总数
		/// </summary>
		public int GetCachedFilesCount()
		{
			return _wrappers.Count;
		}

		/// <summary>
		/// 查询缓存记录
		/// </summary>
		public bool IsCached(string cacheGUID)
		{
			return _wrappers.ContainsKey(cacheGUID);
		}

		/// <summary>
		/// 记录验证结果
		/// </summary>
		public void Record(string cacheGUID, RecordWrapper wrapper)
		{
			if (_wrappers.ContainsKey(cacheGUID) == false)
			{
				_wrappers.Add(cacheGUID, wrapper);
			}
			else
			{
				throw new Exception("Should never get here !");
			}
		}

		/// <summary>
		/// 丢弃验证结果
		/// </summary>
		public void Discard(string cacheGUID)
		{
			if (_wrappers.ContainsKey(cacheGUID))
			{
				_wrappers.Remove(cacheGUID);
			}
		}

		/// <summary>
		/// 获取记录对象
		/// </summary>
		public RecordWrapper TryGetWrapper(string cacheGUID)
		{
			if (_wrappers.TryGetValue(cacheGUID, out RecordWrapper value))
				return value;
			else
				return null;
		}

		internal List<string> GetAllKeys()
		{
			List<string> keys = new List<string>(_wrappers.Keys.Count);
			var keyCollection = _wrappers.Keys;
			foreach (var key in keyCollection)
			{
				keys.Add(key);
			}
			return keys;
		}
	}
}