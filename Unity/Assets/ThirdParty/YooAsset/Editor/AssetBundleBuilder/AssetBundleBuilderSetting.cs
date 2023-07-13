using System;
using UnityEngine;

namespace YooAsset.Editor
{
	[CreateAssetMenu(fileName = "AssetBundleBuilderSetting", menuName = "YooAsset/Create AssetBundle Builder Settings")]
	public class AssetBundleBuilderSetting : ScriptableObject
	{
		/// <summary>
		/// 构建管线
		/// </summary>
		public EBuildPipeline BuildPipeline = EBuildPipeline.BuiltinBuildPipeline;

		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode = EBuildMode.ForceRebuild;

		/// <summary>
		/// 构建的包裹名称
		/// </summary>
		public string BuildPackage = string.Empty;

		/// <summary>
		/// 压缩方式
		/// </summary>
		public ECompressOption CompressOption = ECompressOption.LZ4;

		/// <summary>
		/// 输出文件名称样式
		/// </summary>
		public EOutputNameStyle OutputNameStyle = EOutputNameStyle.HashName;

		/// <summary>
		/// 首包资源文件的拷贝方式
		/// </summary>
		public ECopyBuildinFileOption CopyBuildinFileOption = ECopyBuildinFileOption.None;

		/// <summary>
		/// 首包资源文件的标签集合
		/// </summary>
		public string CopyBuildinFileTags = string.Empty;

		/// <summary>
		/// 加密类名称
		/// </summary>
		public string EncyptionClassName = string.Empty;
	}
}