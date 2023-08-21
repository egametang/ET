
namespace YooAsset
{
	/// <summary>
	/// 运行模式
	/// </summary>
	public enum EPlayMode
	{
		/// <summary>
		/// 编辑器下的模拟模式
		/// </summary>
		EditorSimulateMode,

		/// <summary>
		/// 离线运行模式
		/// </summary>
		OfflinePlayMode,

		/// <summary>
		/// 联机运行模式
		/// </summary>
		HostPlayMode,
	}

	/// <summary>
	/// 初始化参数
	/// </summary>
	public abstract class InitializeParameters
	{
		/// <summary>
		/// 文件解密服务接口
		/// </summary>
		public IDecryptionServices DecryptionServices = null;

		/// <summary>
		/// 内置文件的根路径
		/// 注意：当参数为空的时候会使用默认的根目录。
		/// </summary>
		public string BuildinRootDirectory = string.Empty;

		/// <summary>
		/// 沙盒文件的根路径
		/// 注意：当参数为空的时候会使用默认的根目录。
		/// </summary>
		public string SandboxRootDirectory = string.Empty;
		
		/// <summary>
		/// 资源加载每帧处理的最大时间片段
		/// 注意：默认值为MaxValue
		/// </summary>
		public long LoadingMaxTimeSlice = long.MaxValue;
		
		/// <summary>
		/// 下载失败尝试次数
		/// 注意：默认值为MaxValue
		/// </summary>
		public int DownloadFailedTryAgain = int.MaxValue;
	}

	/// <summary>
	/// 编辑器下模拟运行模式的初始化参数
	/// </summary>
	public class EditorSimulateModeParameters : InitializeParameters
	{
		/// <summary>
		/// 用于模拟运行的资源清单路径
		/// </summary>
		public string SimulateManifestFilePath = string.Empty;
	}

	/// <summary>
	/// 离线运行模式的初始化参数
	/// </summary>
	public class OfflinePlayModeParameters : InitializeParameters
	{
	}

	/// <summary>
	/// 联机运行模式的初始化参数
	/// </summary>
	public class HostPlayModeParameters : InitializeParameters
	{
		/// <summary>
		/// 内置资源查询服务接口
		/// </summary>
		public IQueryServices QueryServices = null;

		/// <summary>
		/// 远端资源地址查询服务类
		/// </summary>
		public IRemoteServices RemoteServices = null;
	}
}