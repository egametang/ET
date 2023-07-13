using UniFramework.Event;

public class UserEventDefine
{
	/// <summary>
	/// 用户尝试再次初始化资源包
	/// </summary>
	public class UserTryInitialize : IEventMessage
	{
		public static void SendEventMessage()
		{
			var msg = new UserTryInitialize();
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 用户开始下载网络文件
	/// </summary>
	public class UserBeginDownloadWebFiles : IEventMessage
	{
		public static void SendEventMessage()
		{
			var msg = new UserBeginDownloadWebFiles();
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 用户尝试再次更新静态版本
	/// </summary>
	public class UserTryUpdatePackageVersion : IEventMessage
	{
		public static void SendEventMessage()
		{
			var msg = new UserTryUpdatePackageVersion();
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 用户尝试再次更新补丁清单
	/// </summary>
	public class UserTryUpdatePatchManifest : IEventMessage
	{
		public static void SendEventMessage()
		{
			var msg = new UserTryUpdatePatchManifest();
			UniEvent.SendMessage(msg);
		}
	}

	/// <summary>
	/// 用户尝试再次下载网络文件
	/// </summary>
	public class UserTryDownloadWebFiles : IEventMessage
	{
		public static void SendEventMessage()
		{
			var msg = new UserTryDownloadWebFiles();
			UniEvent.SendMessage(msg);
		}
	}
}