
/// <summary>
/// 用户层反馈的操作方式
/// </summary>
public enum EPatchOperation
{
	/// <summary>
	/// 开始下载网络文件
	/// </summary>
	BeginDownloadWebFiles,

	/// <summary>
	/// 尝试再次更新静态版本
	/// </summary>
	TryUpdateStaticVersion,

	/// <summary>
	/// 尝试再次更新补丁清单
	/// </summary>
	TryUpdatePatchManifest,

	/// <summary>
	/// 尝试再次下载网络文件
	/// </summary>
	TryDownloadWebFiles,
}