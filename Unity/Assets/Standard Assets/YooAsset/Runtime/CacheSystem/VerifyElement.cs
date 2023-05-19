using System.IO;

namespace YooAsset
{
	/// <summary>
	/// 缓存文件验证元素
	/// </summary>
	internal class VerifyCacheElement
	{
		public string PackageName { private set; get; }
		public string CacheGUID { private set; get; }
		public string FileRootPath { private set; get; }
		public string DataFilePath { private set; get; }
		public string InfoFilePath { private set; get; }

		public EVerifyResult Result;
		public string DataFileCRC;
		public long DataFileSize;

		public VerifyCacheElement(string packageName, string cacheGUID, string fileRootPath, string dataFilePath, string infoFilePath)
		{
			PackageName = packageName;
			CacheGUID = cacheGUID;
			FileRootPath = fileRootPath;
			DataFilePath = dataFilePath;
			InfoFilePath = infoFilePath;
		}

		public void DeleteFiles()
		{
			if (File.Exists(DataFilePath))
			{
				File.Delete(DataFilePath);
			}

			if (File.Exists(InfoFilePath))
			{
				File.Delete(InfoFilePath);
			}
		}
	}

	/// <summary>
	/// 下载文件验证元素
	/// </summary>
	internal class VerifyTempElement
	{
		public string TempDataFilePath { private set; get; }
		public string FileCRC { private set; get; }
		public long FileSize { private set; get; }

		public bool IsDone = false;
		public EVerifyResult Result;

		public VerifyTempElement(string tempDataFilePath, string fileCRC, long fileSize)
		{
			TempDataFilePath = tempDataFilePath;
			FileCRC = fileCRC;
			FileSize = fileSize;
		}
	}
}