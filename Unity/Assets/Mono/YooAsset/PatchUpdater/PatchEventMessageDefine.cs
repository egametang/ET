
public class PatchEventMessageDefine
{
	/// <summary>
	/// 补丁流程步骤改变
	/// </summary>
	public class PatchStatesChange : IEventMessage
	{
		public EPatchStates CurrentStates;
	}

	/// <summary>
	/// 发现更新文件
	/// </summary>
	public class FoundUpdateFiles : IEventMessage
	{
		public int TotalCount;
		public long TotalSizeBytes;
	}

	/// <summary>
	/// 下载进度更新
	/// </summary>
	public class DownloadProgressUpdate : IEventMessage
	{
		public int TotalDownloadCount;
		public int CurrentDownloadCount;
		public long TotalDownloadSizeBytes;
		public long CurrentDownloadSizeBytes;
	}

	/// <summary>
	/// 资源版本号更新失败
	/// </summary>
	public class StaticVersionUpdateFailed : IEventMessage
	{
	}

	/// <summary>
	/// 补丁清单更新失败
	/// </summary>
	public class PatchManifestUpdateFailed : IEventMessage
	{
	}

	/// <summary>
	/// 网络文件下载失败
	/// </summary>
	public class WebFileDownloadFailed : IEventMessage
	{
		public string FileName;
		public string Error;
	}
}