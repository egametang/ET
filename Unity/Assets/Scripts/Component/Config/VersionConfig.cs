using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public class FileVersionInfo
	{
		public string File;
		public string MD5;
		public long Size;
	}

	public class VersionConfig : AConfig
	{
		public int Version;
		
		public long TotalSize;

		public List<FileVersionInfo> FileVersionInfos = new List<FileVersionInfo>();
		
		[BsonIgnore]
		public Dictionary<string, FileVersionInfo> FileInfoDict = new Dictionary<string, FileVersionInfo>();

		public override void EndInit()
		{
			base.EndInit();

			foreach (FileVersionInfo fileVersionInfo in FileVersionInfos)
			{
				this.FileInfoDict.Add(fileVersionInfo.File, fileVersionInfo);
				this.TotalSize += fileVersionInfo.Size;
			}
		}
	}
}