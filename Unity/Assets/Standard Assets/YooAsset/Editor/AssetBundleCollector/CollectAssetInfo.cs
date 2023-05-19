using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	public class CollectAssetInfo
	{
		/// <summary>
		/// 收集器类型
		/// </summary>
		public ECollectorType CollectorType { private set; get; }

		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { private set; get; }
		
		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address { private set; get; }

		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// 是否为原生资源
		/// </summary>
		public bool IsRawAsset { private set; get; }

		/// <summary>
		/// 资源分类标签
		/// </summary>
		public List<string> AssetTags { private set; get; }

		/// <summary>
		/// 依赖的资源列表
		/// </summary>
		public List<string> DependAssets = new List<string>();


		public CollectAssetInfo(ECollectorType collectorType, string bundleName, string address, string assetPath, bool isRawAsset, List<string> assetTags)
		{
			CollectorType = collectorType;
			BundleName = bundleName;
			Address = address;
			AssetPath = assetPath;
			IsRawAsset = isRawAsset;
			AssetTags = assetTags;		
		}
	}
}