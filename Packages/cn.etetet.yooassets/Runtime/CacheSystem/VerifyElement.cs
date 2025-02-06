using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 缓存文件验证元素
    /// </summary>
    internal class VerifyCacheFileElement
    {
        public string PackageName { private set; get; }
        public string CacheGUID { private set; get; }
        public string FileRootPath { private set; get; }
        public string DataFilePath { private set; get; }
        public string InfoFilePath { private set; get; }

        public EVerifyResult Result;
        public string DataFileCRC;
        public long DataFileSize;

        public VerifyCacheFileElement(string packageName, string cacheGUID, string fileRootPath, string dataFilePath, string infoFilePath)
        {
            PackageName = packageName;
            CacheGUID = cacheGUID;
            FileRootPath = fileRootPath;
            DataFilePath = dataFilePath;
            InfoFilePath = infoFilePath;
        }

        public void DeleteFiles()
        {
            try
            {
                Directory.Delete(FileRootPath, true);
            }
            catch (System.Exception e)
            {
                YooLogger.Warning($"Failed delete cache bundle folder : {e}");
            }
        }
    }

    /// <summary>
    /// 下载文件验证元素
    /// </summary>
    internal class VerifyTempFileElement
    {
        public string TempDataFilePath { private set; get; }
        public string FileCRC { private set; get; }
        public long FileSize { private set; get; }

        public int Result = 0; // 注意：原子操作对象

        public VerifyTempFileElement(string tempDataFilePath, string fileCRC, long fileSize)
        {
            TempDataFilePath = tempDataFilePath;
            FileCRC = fileCRC;
            FileSize = fileSize;
        }
    }
}