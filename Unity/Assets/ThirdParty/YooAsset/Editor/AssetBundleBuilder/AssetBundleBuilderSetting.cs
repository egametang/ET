using System;
using UnityEngine;

namespace YooAsset.Editor
{
	public class AssetBundleBuilderSetting : ScriptableObject
	{
		/// <summary>
		/// 构建版本号
		/// </summary>
		public int BuildVersion = 0;

		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode = EBuildMode.ForceRebuild;

		/// <summary>
		/// 内置资源标签（首包资源标签）
		/// </summary>
		public string BuildTags = string.Empty;

		/// <summary>
		/// 压缩方式
		/// </summary>
		public ECompressOption CompressOption = ECompressOption.LZ4;

		/// <summary>
		/// 加密类名称
		/// </summary>
		public string EncyptionClassName = string.Empty;

		/// <summary>
		/// 附加后缀格式
		/// </summary>
		public bool AppendExtension = false;
	}
}