using UnityEditor;
using UnityEngine;

namespace Model
{
	public static class ResourceHelper
	{
		public static Object LoadResource(string bundleName, string prefab)
		{
			string[] realPath = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName.ToLower() + ".unity3d", prefab);
			UnityEngine.Object resource = AssetDatabase.LoadAssetAtPath(realPath[0], typeof(GameObject));
			return resource;
		}
	}
}
