using System;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
	/// <summary>
	/// 零冗余的共享资源打包规则
	/// </summary>
	public class ZeroRedundancySharedPackRule : ISharedPackRule
	{
		public PackRuleResult GetPackRuleResult(string assetPath)
		{
			string bundleName = Path.GetDirectoryName(assetPath);
			PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
			return result;
		}
	}
	
	/// <summary>
	/// 全部冗余的共享资源打包规则
	/// </summary>
	public class FullRedundancySharedPackRule : ISharedPackRule
	{
		public PackRuleResult GetPackRuleResult(string assetPath)
		{
			PackRuleResult result = new PackRuleResult(string.Empty, string.Empty);
			return result;
		}
	}
}