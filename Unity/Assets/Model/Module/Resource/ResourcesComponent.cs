using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ET
{
	
	public class ABInfoAwakeSystem : AwakeSystem<ABInfo, string, AssetBundle>
	{
		public override void Awake(ABInfo self, string abName, AssetBundle a)
		{
			self.AssetBundle = a;
			self.Name = abName;
			self.RefCount = 1;
		}
	}
	
	public class ABInfo : Entity
	{
		public string Name { get; set; }

		public int RefCount { get; set; }

		public AssetBundle AssetBundle;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			//Log.Debug($"desdroy assetbundle: {this.Name}");

			if (this.AssetBundle != null)
			{
				this.AssetBundle.Unload(true);
			}

			this.RefCount = 0;
			this.Name = "";
		}
	}
	
	// 用于字符串转换，减少GC
	public static class AssetBundleHelper
	{
		public static readonly Dictionary<int, string> IntToStringDict = new Dictionary<int, string>();
		
		public static readonly Dictionary<string, string> StringToABDict = new Dictionary<string, string>();

		public static readonly Dictionary<string, string> BundleNameToLowerDict = new Dictionary<string, string>() 
		{
			{ "StreamingAssets", "StreamingAssets" }
		};
		
		// 缓存包依赖，不用每次计算
		public static Dictionary<string, string[]> DependenciesCache = new Dictionary<string, string[]>();

		public static string IntToString(this int value)
		{
			string result;
			if (IntToStringDict.TryGetValue(value, out result))
			{
				return result;
			}

			result = value.ToString();
			IntToStringDict[value] = result;
			return result;
		}
		
		public static string StringToAB(this string value)
		{
			string result;
			if (StringToABDict.TryGetValue(value, out result))
			{
				return result;
			}

			result = value + ".unity3d";
			StringToABDict[value] = result;
			return result;
		}

		public static string IntToAB(this int value)
		{
			return value.IntToString().StringToAB();
		}
		
		public static string BundleNameToLower(this string value)
		{
			string result;
			if (BundleNameToLowerDict.TryGetValue(value, out result))
			{
				return result;
			}

			result = value.ToLower();
			BundleNameToLowerDict[value] = result;
			return result;
		}
		
		public static string[] GetDependencies(string assetBundleName)
		{
			string[] dependencies = new string[0];
			if (DependenciesCache.TryGetValue(assetBundleName,out dependencies))
			{
				return dependencies;
			}
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
			DependenciesCache.Add(assetBundleName, dependencies);
			return dependencies;
		}

		public static string[] GetSortedDependencies(string assetBundleName)
		{
			Dictionary<string, int> info = new Dictionary<string, int>();
			List<string> parents = new List<string>();
			CollectDependencies(parents, assetBundleName, info);
			string[] ss = info.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
			return ss;
		}

		public static void CollectDependencies(List<string> parents, string assetBundleName, Dictionary<string, int> info)
		{
			parents.Add(assetBundleName);
			string[] deps = GetDependencies(assetBundleName);
			foreach (string parent in parents)
			{
				if (!info.ContainsKey(parent))
				{
					info[parent] = 0;
				}
				info[parent] += deps.Length;
			}


			foreach (string dep in deps)
			{
				if (parents.Contains(dep))
				{
					throw new Exception($"包有循环依赖，请重新标记: {assetBundleName} {dep}");
				}
				CollectDependencies(parents, dep, info);
			}
			parents.RemoveAt(parents.Count - 1);
		}
	}
	
	public class ResourcesComponentAwakeSystem: AwakeSystem<ResourcesComponent>
	{
		public override void Awake(ResourcesComponent self)
		{
			ResourcesComponent.Instance = self;
		}
	}
	

	public class ResourcesComponent : Entity
	{
		public static ResourcesComponent Instance;
		
		public static AssetBundleManifest AssetBundleManifestObject { get; set; }

		private readonly Dictionary<string, Dictionary<string, UnityEngine.Object>> resourceCache = new Dictionary<string, Dictionary<string, UnityEngine.Object>>();

		private readonly Dictionary<string, ABInfo> bundles = new Dictionary<string, ABInfo>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (var abInfo in this.bundles)
			{
				abInfo.Value.Dispose();
			}

			this.bundles.Clear();
			this.resourceCache.Clear();
		}

		public UnityEngine.Object GetAsset(string bundleName, string prefab)
		{
			Dictionary<string, UnityEngine.Object> dict;
			if (!this.resourceCache.TryGetValue(bundleName.BundleNameToLower(), out dict))
			{
				throw new Exception($"not found asset: {bundleName} {prefab}");
			}

			UnityEngine.Object resource = null;
			if (!dict.TryGetValue(prefab, out resource))
			{
				throw new Exception($"not found asset: {bundleName} {prefab}");
			}

			return resource;
		}

		public void UnloadBundle(string assetBundleName)
		{
			assetBundleName = assetBundleName.BundleNameToLower();

			string[] dependencies = AssetBundleHelper.GetSortedDependencies(assetBundleName);

			//Log.Debug($"-----------dep unload {assetBundleName} dep: {dependencies.ToList().ListToString()}");
			foreach (string dependency in dependencies)
			{
				this.UnloadOneBundle(dependency);
			}
		}

		private void UnloadOneBundle(string assetBundleName)
		{
			assetBundleName = assetBundleName.BundleNameToLower();

			ABInfo abInfo;
			if (!this.bundles.TryGetValue(assetBundleName, out abInfo))
			{
				throw new Exception($"not found assetBundle: {assetBundleName}");
			}
			
			//Log.Debug($"---------- unload one bundle {assetBundleName} refcount: {abInfo.RefCount - 1}");

			--abInfo.RefCount;
            
			if (abInfo.RefCount > 0)
			{
				return;
			}


			this.bundles.Remove(assetBundleName);
			this.resourceCache.Remove(assetBundleName);
			abInfo.Dispose();
			//Log.Debug($"cache count: {this.cacheDictionary.Count}");
		}

		/// <summary>
		/// 同步加载assetbundle
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <returns></returns>
		public void LoadBundle(string assetBundleName)
		{
			assetBundleName = assetBundleName.ToLower();
			string[] dependencies = AssetBundleHelper.GetSortedDependencies(assetBundleName);
			//Log.Debug($"-----------dep load {assetBundleName} dep: {dependencies.ToList().ListToString()}");
			foreach (string dependency in dependencies)
			{
				if (string.IsNullOrEmpty(dependency))
				{
					continue;
				}
				this.LoadOneBundle(dependency);
			}
        }

		public void AddResource(string bundleName, string assetName, UnityEngine.Object resource)
		{
			Dictionary<string, UnityEngine.Object> dict;
			if (!this.resourceCache.TryGetValue(bundleName.BundleNameToLower(), out dict))
			{
				dict = new Dictionary<string, UnityEngine.Object>();
				this.resourceCache[bundleName] = dict;
			}

			dict[assetName] = resource;
		}

		public void LoadOneBundle(string assetBundleName)
		{
			//Log.Debug($"---------------load one bundle {assetBundleName}");
			ABInfo abInfo;
			if (this.bundles.TryGetValue(assetBundleName, out abInfo))
			{
				++abInfo.RefCount;
				return;
			}

			if (!Define.IsAsync)
			{
				string[] realPath = null;
#if UNITY_EDITOR
				realPath = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
				foreach (string s in realPath)
				{
					string assetName = Path.GetFileNameWithoutExtension(s);
					UnityEngine.Object resource = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
					AddResource(assetBundleName, assetName, resource);
				}

				abInfo = EntityFactory.CreateWithParent<ABInfo, string, AssetBundle>(this, assetBundleName, null);
				this.bundles[assetBundleName] = abInfo;
#endif
				return;
			}

			string p = Path.Combine(PathHelper.AppHotfixResPath, assetBundleName);
			AssetBundle assetBundle = null;
			if (File.Exists(p))
			{
				assetBundle = AssetBundle.LoadFromFile(p);
			}
			else
			{
				p = Path.Combine(PathHelper.AppResPath, assetBundleName);
				assetBundle = AssetBundle.LoadFromFile(p);
			}

			if (assetBundle == null)
			{
				throw new Exception($"assets bundle not found: {assetBundleName}");
			}

			if (!assetBundle.isStreamedSceneAssetBundle)
			{
				// 异步load资源到内存cache住
				UnityEngine.Object[] assets = assetBundle.LoadAllAssets();
				foreach (UnityEngine.Object asset in assets)
				{
					AddResource(assetBundleName, asset.name, asset);
				}
			}

			abInfo = EntityFactory.CreateWithParent<ABInfo, string, AssetBundle>(this, assetBundleName, assetBundle);
			this.bundles[assetBundleName] = abInfo;
		}

		/// <summary>
		/// 异步加载assetbundle
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <returns></returns>
		public async ETTask LoadBundleAsync(string assetBundleName)
		{
            assetBundleName = assetBundleName.ToLower();
			string[] dependencies = AssetBundleHelper.GetSortedDependencies(assetBundleName);
            // Log.Debug($"-----------dep load {assetBundleName} dep: {dependencies.ToList().ListToString()}");
            foreach (string dependency in dependencies)
			{
				if (string.IsNullOrEmpty(dependency))
				{
					continue;
				}
				await this.LoadOneBundleAsync(dependency);
			}
        }

		public async ETTask LoadOneBundleAsync(string assetBundleName)
		{
			ABInfo abInfo;
			if (this.bundles.TryGetValue(assetBundleName, out abInfo))
			{
				++abInfo.RefCount;
				return;
			}

            //Log.Debug($"---------------load one bundle {assetBundleName}");
            if (!Define.IsAsync)
			{
				string[] realPath = null;
#if UNITY_EDITOR
				realPath = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
				foreach (string s in realPath)
				{
					string assetName = Path.GetFileNameWithoutExtension(s);
					UnityEngine.Object resource = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
					AddResource(assetBundleName, assetName, resource);
				}

				abInfo = EntityFactory.CreateWithParent<ABInfo, string, AssetBundle>(this, assetBundleName, null);
				this.bundles[assetBundleName] = abInfo;
#endif
				return;
			}

			string p = Path.Combine(PathHelper.AppHotfixResPath, assetBundleName);
			AssetBundle assetBundle = null;
			if (!File.Exists(p))
			{
				p = Path.Combine(PathHelper.AppResPath, assetBundleName);
			}
			
			using (AssetsBundleLoaderAsync assetsBundleLoaderAsync = EntityFactory.Create<AssetsBundleLoaderAsync>(this.Domain))
			{
				assetBundle = await assetsBundleLoaderAsync.LoadAsync(p);
			}

			if (assetBundle == null)
			{
				throw new Exception($"assets bundle not found: {assetBundleName}");
			}

			if (!assetBundle.isStreamedSceneAssetBundle)
			{
				// 异步load资源到内存cache住
				UnityEngine.Object[] assets;
				using (AssetsLoaderAsync assetsLoaderAsync = EntityFactory.Create<AssetsLoaderAsync, AssetBundle>(this.Domain, assetBundle))
				{
					assets = await assetsLoaderAsync.LoadAllAssetsAsync();
				}
				foreach (UnityEngine.Object asset in assets)
				{
					AddResource(assetBundleName, asset.name, asset);
				}
			}

			abInfo = EntityFactory.CreateWithParent<ABInfo, string, AssetBundle>(this, assetBundleName, assetBundle);
			this.bundles[assetBundleName] = abInfo;
		}

		public string DebugString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (ABInfo abInfo in this.bundles.Values)
			{
				sb.Append($"{abInfo.Name}:{abInfo.RefCount}\n");
			}
			return sb.ToString();
		}
	}
}