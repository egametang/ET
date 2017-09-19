#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Model
{
	public static class ResourceHelper
	{
		public static UnityEngine.Object LoadResource(string bundleName, string prefab)
		{
#if  UNITY_EDITOR
			string[] realPath = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName.ToLower() + ".unity3d", prefab);
			UnityEngine.Object resource = AssetDatabase.LoadAssetAtPath(realPath[0], typeof(GameObject));
			return resource;
#else
			return null;
#endif
		}
	}
}
