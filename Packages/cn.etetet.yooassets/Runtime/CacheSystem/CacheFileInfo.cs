using System;
using System.IO;

namespace YooAsset
{
    internal class CacheFileInfo
    {
        private static readonly BufferWriter SharedBuffer = new BufferWriter(1024);

        /// <summary>
        /// 写入资源包信息
        /// </summary>
        public static void WriteInfoToFile(string filePath, string dataFileCRC, long dataFileSize)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                SharedBuffer.Clear();
                SharedBuffer.WriteUTF8(dataFileCRC);
                SharedBuffer.WriteInt64(dataFileSize);
                SharedBuffer.WriteToStream(fs);
                fs.Flush();
            }
        }

        /// <summary>
        /// 读取资源包信息
        /// </summary>
        public static void ReadInfoFromFile(string filePath, out string dataFileCRC, out long dataFileSize)
        {
            byte[] binaryData = FileUtility.ReadAllBytes(filePath);
            BufferReader buffer = new BufferReader(binaryData);
            dataFileCRC = buffer.ReadUTF8();
            dataFileSize = buffer.ReadInt64();
        }
    }
}