﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Model
{
	public class ABInfo: Disposer
	{
		private int refCount;
		public string Name { get; }

		public int RefCount
		{
			get
			{
				return this.refCount;
			}
			set
			{
				Log.Debug($"{this.Name} refcount: {value}");
				this.refCount = value;
			}
		}

		public AssetBundle AssetBundle { get; }

		public ABInfo(string name, AssetBundle ab)
		{
			this.Name = name;
			this.AssetBundle = ab;
			this.RefCount = 1;
			Log.Debug($"load assetbundle: {this.Name}");
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			
			base.Dispose();
			
			Log.Debug($"desdroy assetbundle: {this.Name}");
			
			this.AssetBundle?.Unload(true);
		}
	}

	public class ResourcesComponent : Component
	{
		public static AssetBundleManifest AssetBundleManifestObject { get; set; }

		private readonly Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();

		private readonly Dictionary<string, ABInfo> bundles = new Dictionary<string, ABInfo>();
		
		// lru缓存队列
		private readonly QueueDictionary<string, ABInfo> cacheDictionary = new QueueDictionary<string, ABInfo>();

		public K GetAsset<K>(string bundleName, string prefab) where K : class
		{
			string path = $"{bundleName}/{prefab}".ToLower();

			UnityEngine.Object resource = null;
			if (!this.resourceCache.TryGetValue(path, out resource))
			{
				throw new Exception($"not found asset: {path}");
			}
			
			return resource as K;
		}

		public void UnloadBundle(string assetBundleName)
		{
			assetBundleName = assetBundleName.ToLower();
			
			this.UnloadOneBundle(assetBundleName);

			string[] dependencies = ResourcesHelper.GetSortedDependencies(assetBundleName);

			//Log.Debug($"-----------dep unload {assetBundleName} dep: {dependencies.ToList().ListToString()}");
			foreach (string dependency in dependencies)
			{
				this.UnloadOneBundle(dependency);
			}
		}

		private void UnloadOneBundle(string assetBundleName)
		{
			assetBundleName = assetBundleName.ToLower();
			
			//Log.Debug($"unload bundle {assetBundleName}");
			ABInfo abInfo;
			if (!this.bundles.TryGetValue(assetBundleName, out abInfo))
			{
				throw new Exception($"not found assetBundle: {assetBundleName}");
			}

			--abInfo.RefCount;
			if (abInfo.RefCount > 0)
			{
				return;
			}
			
			
			this.bundles.Remove(assetBundleName);
			
			// 缓存10个包
			this.cacheDictionary.Enqueue(assetBundleName, abInfo);
			if (this.cacheDictionary.Count > 10)
			{
				abInfo = this.cacheDictionary[this.cacheDictionary.FirstKey];
				this.cacheDictionary.Dequeue();
				abInfo.Dispose();
			}
			Log.Debug($"cache count: {this.cacheDictionary.Count}");
		}
		
		/// <summary>
		/// 同步加载assetbundle
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <returns></returns>
		public void LoadBundle(string assetBundleName)
		{
			assetBundleName = assetBundleName.ToLower();
			this.LoadOneBundle(assetBundleName);

			string[] dependencies = ResourcesHelper.GetSortedDependencies(assetBundleName);

			Log.Debug($"-----------dep load {assetBundleName} dep: {dependencies.ToList().ListToString()}");
			foreach (string dependency in dependencies)
			{
				if (string.IsNullOrEmpty(dependency))
				{
					continue;
				}
				this.LoadOneBundle(dependency);
			}
		}
		
		public void LoadOneBundle(string assetBundleName)
		{
			ABInfo abInfo;
			if (this.bundles.TryGetValue(assetBundleName, out abInfo))
			{
				++abInfo.RefCount;
				return;
			}


			if (this.cacheDictionary.ContainsKey(assetBundleName))
			{
				abInfo = this.cacheDictionary[assetBundleName];
				++abInfo.RefCount;
				this.bundles[assetBundleName] = abInfo;
				this.cacheDictionary.Remove(assetBundleName);
				return;
			}
			

			if (!Define.IsAsync)
			{
#if UNITY_EDITOR
				string[] realPath = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
				foreach (string s in realPath)
				{
					string assetName = Path.GetFileNameWithoutExtension(s);
					string path = $"{assetBundleName}/{assetName}".ToLower();
					UnityEngine.Object resource = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
					this.resourceCache[path] = resource;
				}
				
				this.bundles[assetBundleName] = new ABInfo(assetBundleName, null);
				return;
#endif
			}

			AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(PathHelper.AppHotfixResPath, assetBundleName));

			if (!assetBundle.isStreamedSceneAssetBundle)
			{
				// 异步load资源到内存cache住
				UnityEngine.Object[] assets = assetBundle.LoadAllAssets();
				foreach (UnityEngine.Object asset in assets)
				{
					string path = $"{assetBundleName}/{asset.name}".ToLower();
					this.resourceCache[path] = asset;
				}
			}
			
			this.bundles[assetBundleName] = new ABInfo(assetBundleName, assetBundle);
		}

		/// <summary>
		/// 异步加载assetbundle
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <returns></returns>
		public async Task LoadBundleAsync(string assetBundleName)
		{
			assetBundleName = assetBundleName.ToLower();
			await this.LoadOneBundleAsync(assetBundleName);

			string[] dependencies = ResourcesHelper.GetSortedDependencies(assetBundleName);

			//Log.Debug($"-----------dep load {assetBundleName} dep: {dependencies.ToList().ListToString()}");
			foreach (string dependency in dependencies)
			{
				if (string.IsNullOrEmpty(dependency))
				{
					continue;
				}
				await this.LoadOneBundleAsync(dependency);
			}
		}

		public async Task LoadOneBundleAsync(string assetBundleName)
		{
			ABInfo abInfo;
			if (this.bundles.TryGetValue(assetBundleName, out abInfo))
			{
				++abInfo.RefCount;
				return;
			}


			if (this.cacheDictionary.ContainsKey(assetBundleName))
			{
				abInfo = this.cacheDictionary[assetBundleName];
				++abInfo.RefCount;
				this.bundles[assetBundleName] = abInfo;
				this.cacheDictionary.Remove(assetBundleName);
				return;
			}
			

			if (!Define.IsAsync)
			{
#if UNITY_EDITOR
				string[] realPath = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
				foreach (string s in realPath)
				{
					string assetName = Path.GetFileNameWithoutExtension(s);
					string path = $"{assetBundleName}/{assetName}".ToLower();
					UnityEngine.Object resource = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
					this.resourceCache[path] = resource;
				}
				
				this.bundles[assetBundleName] = new ABInfo(assetBundleName, null);
				return;
#endif
			}

			AssetBundle assetBundle;
			using (AssetsBundleLoaderAsync assetsBundleLoaderAsync = EntityFactory.Create<AssetsBundleLoaderAsync>())
			{
				assetBundle = await assetsBundleLoaderAsync.LoadAsync(assetBundleName);
			}

			if (!assetBundle.isStreamedSceneAssetBundle)
			{
				// 异步load资源到内存cache住
				UnityEngine.Object[] assets;
				using (AssetsLoaderAsync assetsLoaderAsync = EntityFactory.Create<AssetsLoaderAsync, AssetBundle>(assetBundle))
				{
					assets = await assetsLoaderAsync.LoadAllAssetsAsync();
				}
				foreach (UnityEngine.Object asset in assets)
				{
					string path = $"{assetBundleName}/{asset.name}".ToLower();
					this.resourceCache[path] = asset;
				}
			}
			
			this.bundles[assetBundleName] = new ABInfo(assetBundleName, assetBundle);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (var abInfo in this.bundles)
			{
				abInfo.Value?.AssetBundle?.Unload(true);
			}
			
			this.bundles.Clear();
			this.cacheDictionary.Clear();
			this.resourceCache.Clear();
		}
	}
}