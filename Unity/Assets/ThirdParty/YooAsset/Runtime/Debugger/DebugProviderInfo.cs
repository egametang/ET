using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	[Serializable]
	internal class DebugProviderInfo : IComparer<DebugProviderInfo>, IComparable<DebugProviderInfo>
	{
		/// <summary>
		/// 资源对象路径
		/// </summary>
		public string AssetPath;

		/// <summary>
		/// 资源出生的场景
		/// </summary>
		public string SpawnScene;

		/// <summary>
		/// 资源出生的时间
		/// </summary>
		public string SpawnTime;

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount;

		/// <summary>
		/// 加载状态
		/// </summary>
		public int Status;

		/// <summary>
		/// 依赖的资源包列表
		/// </summary>
		public List<DebugBundleInfo> BundleInfos;

		public int CompareTo(DebugProviderInfo other)
		{
			return Compare(this, other);
		}
		public int Compare(DebugProviderInfo a, DebugProviderInfo b)
		{
			return string.CompareOrdinal(a.AssetPath, b.AssetPath);
		}
	}
}