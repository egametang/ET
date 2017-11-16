using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Model
{
	public static class ResourcesHelper
	{
		public static string[] GetDependencies(string assetBundleName)
		{
			string[] dependencies = new string[0];
			if (!Define.IsAsync)
			{
#if UNITY_EDITOR
				dependencies = AssetDatabase.GetAssetBundleDependencies(assetBundleName, true);
#endif
			}
			else
			{
				dependencies = ResourcesComponent.AssetBundleManifestObject.GetAllDependencies(assetBundleName);
			}
			return dependencies;
		}
	}
}
