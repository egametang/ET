using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[Serializable]
	public class ReportBundleInfo
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 文件名称
		/// </summary>
		public string FileName;

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public string FileHash;

		/// <summary>
		/// 文件校验码
		/// </summary>
		public string FileCRC;

		/// <summary>
		/// 文件大小（字节数）
		/// </summary>
		public long FileSize;

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile;

		/// <summary>
		/// 加载方法
		/// </summary>
		public EBundleLoadMethod LoadMethod;

		/// <summary>
		/// Tags
		/// </summary>
		public string[] Tags;

		/// <summary>
		/// 引用该资源包的ID列表
		/// </summary>
		public int[] ReferenceIDs;

		/// <summary>
		/// 该资源包内包含的所有资源
		/// </summary>
		public List<string> AllBuiltinAssets = new List<string>();

		/// <summary>
		/// 获取资源分类标签的字符串
		/// </summary>
		public string GetTagsString()
		{
			if (Tags != null)
				return String.Join(";", Tags);
			else
				return string.Empty;
		}
	}
}