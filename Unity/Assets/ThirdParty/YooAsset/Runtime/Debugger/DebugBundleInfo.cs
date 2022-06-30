using System;

namespace YooAsset
{
	[Serializable]
	internal class DebugBundleInfo
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount;

		/// <summary>
		/// 加载状态
		/// </summary>
		public int Status;
	}
}