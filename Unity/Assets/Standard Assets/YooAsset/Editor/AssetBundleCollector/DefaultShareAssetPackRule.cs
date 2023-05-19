using System;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
	public class DefaultShareAssetPackRule : IShareAssetPackRule
	{
		public PackRuleResult GetPackRuleResult(string assetPath)
		{
			string bundleName = Path.GetDirectoryName(assetPath);
			PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
			return result;
		}
	}
}