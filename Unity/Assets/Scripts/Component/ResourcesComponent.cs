using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Model
{
	// 用来实例化资源,暂时直接加载,之后可以预先加载
	public class ResourcesComponent: Component
	{
		public static AssetBundleManifest AssetBundleManifestObject { get; set; }

		private readonly Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();

		private readonly Dictionary<string, AssetBundle> bundleCaches = new Dictionary<string, AssetBundle>();

		public K GetUnitRefrenceById<K>(string unitId, EntityType entityType) where K : class
		{
			string assetBundleName = $"unit/{AssetBundleHelper.GetBundleNameById(unitId, entityType)}";
			return GetAsset<K>(assetBundleName, unitId);
		}

		public K GetReference<K>(string bundle, string prefab, string key) where K : class
		{
			GameObject gameObject = this.GetAsset<GameObject>(bundle, prefab);
			return gameObject.GetComponent<ReferenceCollector>().Get<K>(key);
		}

		public K GetAsset<K>(string bundleName, string prefab) where K : class
		{
			string path = $"{bundleName}.unity3d/{prefab}".ToLower();

			UnityEngine.Object resource = null;
			if (this.resourceCache.TryGetValue(path, out resource))
			{
				return resource as K;
			}

			if (Define.LoadResourceType == LoadResourceType.Async)
			{
				if (!this.bundleCaches.ContainsKey($"{bundleName}.unity3d".ToLower()))
				{
					return null;
				}

				throw new ConfigException($"异步加载资源,资源不在resourceCache中: {bundleName} {path}");
			}

			try
			{
#if UNITY_EDITOR
				string[] realPath = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName.ToLower() + ".unity3d", prefab);
				resource = AssetDatabase.LoadAssetAtPath(realPath[0], typeof (GameObject));
				this.resourceCache.Add(path, resource);
#endif
			}
			catch (Exception e)
			{
				throw new ConfigException($"加载资源出错,输入路径:{path}", e);
			}

			return resource as K;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (var assetBundle in bundleCaches)
			{
				assetBundle.Value?.Unload(true);
			}
		}
	}
}