
/// <summary>
/// 补丁系统更新状态
/// </summary>
public enum EPatchStates
{
	/// <summary>
	/// 更新静态的资源版本
	/// </summary>
	UpdateStaticVersion,

	/// <summary>
	/// 更新补丁清单
	/// </summary>
	UpdateManifest,

	/// <summary>
	/// 创建下载器
	/// </summary>
	CreateDownloader,

	/// <summary>
	/// 下载远端文件
	/// </summary>
	DownloadWebFiles,

	/// <summary>
	/// 补丁流程完毕
	/// </summary>
	PatchDone,
}