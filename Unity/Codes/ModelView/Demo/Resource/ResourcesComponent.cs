using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class ABInfoAwakeSystem: AwakeSystem<ABInfo, string, AssetBundle>
    {
        public override void Awake(ABInfo self, string abName, AssetBundle a)
        {
            self.AssetBundle = a;
            self.Name = abName;
            self.RefCount = 1;
            self.AlreadyLoadAssets = false;
        }
    }

    [ObjectSystem]
    public class ABInfoDestroySystem: DestroySystem<ABInfo>
    {
        public override void Destroy(ABInfo self)
        {
            //Log.Debug($"desdroy assetbundle: {this.Name}");

            self.RefCount = 0;
            self.Name = "";
            self.AlreadyLoadAssets = false;
            self.AssetBundle = null;
        }
    }

    public class ABInfo: Entity
    {
        public string Name { get; set; }

        public int RefCount { get; set; }

        public AssetBundle AssetBundle;

        public bool AlreadyLoadAssets;

        public void Destroy(bool unload = true)
        {
            if (this.AssetBundle != null)
            {
                this.AssetBundle.Unload(unload);
            }

            this.Dispose();
        }
    }

    // 用于字符串转换，减少GC
    public static class AssetBundleHelper
    {
        public static async ETTask<AssetBundle> UnityLoadBundleAsync(string path)
        {
            var tcs = ETTask<AssetBundle>.Create(true);
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            request.completed += operation => { tcs.SetResult(request.assetBundle); };
            return await tcs;
        }

        public static async ETTask<UnityEngine.Object[]> UnityLoadAssetAsync(AssetBundle assetBundle)
        {
            var tcs = ETTask<UnityEngine.Object[]>.Create(true);
            AssetBundleRequest request = assetBundle.LoadAllAssetsAsync();
            request.completed += operation => { tcs.SetResult(request.allAssets); };
            return await tcs;
        }

        public static string IntToString(this int value)
        {
            string result;
            if (ResourcesComponent.Instance.IntToStringDict.TryGetValue(value, out result))
            {
                return result;
            }

            result = value.ToString();
            ResourcesComponent.Instance.IntToStringDict[value] = result;
            return result;
        }

        public static string StringToAB(this string value)
        {
            string result;
            if (ResourcesComponent.Instance.StringToABDict.TryGetValue(value, out result))
            {
                return result;
            }

            result = value + ".unity3d";
            ResourcesComponent.Instance.StringToABDict[value] = result;
            return result;
        }

        public static string IntToAB(this int value)
        {
            return value.IntToString().StringToAB();
        }

        public static string BundleNameToLower(this string value)
        {
            string result;
            if (ResourcesComponent.Instance.BundleNameToLowerDict.TryGetValue(value, out result))
            {
                return result;
            }

            result = value.ToLower();
            ResourcesComponent.Instance.BundleNameToLowerDict[value] = result;
            return result;
        }
    }

    [ObjectSystem]
    public class ResourcesComponentAwakeSystem: AwakeSystem<ResourcesComponent>
    {
        public override void Awake(ResourcesComponent self)
        {
            self.Awake();
        }
    }

    public class ResourcesComponent: Entity
    {
        public static ResourcesComponent Instance { get; set; }

        private AssetBundleManifest AssetBundleManifestObject { get; set; }

        public Dictionary<int, string> IntToStringDict = new Dictionary<int, string>();

        public Dictionary<string, string> StringToABDict = new Dictionary<string, string>();

        public Dictionary<string, string> BundleNameToLowerDict = new Dictionary<string, string>() { { "StreamingAssets", "StreamingAssets" } };

        private readonly Dictionary<string, Dictionary<string, UnityEngine.Object>> resourceCache =
                new Dictionary<string, Dictionary<string, UnityEngine.Object>>();

        private readonly Dictionary<string, ABInfo> bundles = new Dictionary<string, ABInfo>();

        public void Awake()
        {
            Instance = this;

            if (Define.IsAsync)
            {
                LoadOneBundle("StreamingAssets");
                AssetBundleManifestObject = (AssetBundleManifest) GetAsset("StreamingAssets", "AssetBundleManifest");
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            Instance = null;

            foreach (var abInfo in this.bundles)
            {
                abInfo.Value.Destroy();
            }

            this.bundles.Clear();
            this.resourceCache.Clear();
            this.IntToStringDict.Clear();
            this.StringToABDict.Clear();
            this.BundleNameToLowerDict.Clear();
        }

        private string[] GetDependencies(string assetBundleName)
        {
            string[] dependencies = Array.Empty<string>();
            if (DependenciesCache.TryGetValue(assetBundleName, out dependencies))
            {
                return dependencies;
            }

            if (!Define.IsAsync)
            {
                if (Define.IsEditor)
                {
                    dependencies = Define.GetAssetBundleDependencies(assetBundleName, true);
                }
            }
            else
            {
                dependencies = this.AssetBundleManifestObject.GetAllDependencies(assetBundleName);
            }

            DependenciesCache.Add(assetBundleName, dependencies);
            return dependencies;
        }

        private string[] GetSortedDependencies(string assetBundleName)
        {
            var info = new Dictionary<string, int>();
            var parents = new List<string>();
            CollectDependencies(parents, assetBundleName, info);
            string[] ss = info.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
            return ss;
        }

        private void CollectDependencies(List<string> parents, string assetBundleName, Dictionary<string, int> info)
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

        // 缓存包依赖，不用每次计算
        private readonly Dictionary<string, string[]> DependenciesCache = new Dictionary<string, string[]>();

        public bool Contains(string bundleName)
        {
            return this.bundles.ContainsKey(bundleName);
        }

        public Dictionary<string, UnityEngine.Object> GetBundleAll(string bundleName)
        {
            Dictionary<string, UnityEngine.Object> dict;
            if (!this.resourceCache.TryGetValue(bundleName.BundleNameToLower(), out dict))
            {
                throw new Exception($"not found asset: {bundleName}");
            }

            return dict;
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
        
        // 一帧卸载一个包，避免卡死
        public async ETTask UnloadBundleAsync(string assetBundleName, bool unload = true)
        {
            assetBundleName = assetBundleName.BundleNameToLower();

            string[] dependencies = GetSortedDependencies(assetBundleName);

            //Log.Debug($"-----------dep unload start {assetBundleName} dep: {dependencies.ToList().ListToString()}");
            foreach (string dependency in dependencies)
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, assetBundleName.GetHashCode()))
                {
                    this.UnloadOneBundle(dependency, unload);
                    
                    await TimerComponent.Instance.WaitFrameAsync();
                }
            }
            //Log.Debug($"-----------dep unload finish {assetBundleName} dep: {dependencies.ToList().ListToString()}");
        }

        // 只允许场景设置unload为false
        public void UnloadBundle(string assetBundleName, bool unload = true)
        {
            assetBundleName = assetBundleName.BundleNameToLower();

            string[] dependencies = GetSortedDependencies(assetBundleName);

            //Log.Debug($"-----------dep unload start {assetBundleName} dep: {dependencies.ToList().ListToString()}");
            foreach (string dependency in dependencies)
            {
                this.UnloadOneBundle(dependency, unload);
            }

            //Log.Debug($"-----------dep unload finish {assetBundleName} dep: {dependencies.ToList().ListToString()}");
        }

        private void UnloadOneBundle(string assetBundleName, bool unload = true)
        {
            assetBundleName = assetBundleName.BundleNameToLower();

            ABInfo abInfo;
            if (!this.bundles.TryGetValue(assetBundleName, out abInfo))
            {
                return;
            }

            //Log.Debug($"---------------unload one bundle {assetBundleName} refcount: {abInfo.RefCount - 1}");

            --abInfo.RefCount;

            if (abInfo.RefCount > 0)
            {
                return;
            }

            //Log.Debug($"---------------truly unload one bundle {assetBundleName} refcount: {abInfo.RefCount}");
            this.bundles.Remove(assetBundleName);
            this.resourceCache.Remove(assetBundleName);
            abInfo.Destroy(unload);
            // Log.Debug($"cache count: {this.cacheDictionary.Count}");
        }

        /// <summary>
        /// 同步加载assetbundle
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public void LoadBundle(string assetBundleName)
        {
            assetBundleName = assetBundleName.ToLower();

            string[] dependencies = GetSortedDependencies(assetBundleName);
            //Log.Debug($"-----------dep load start {assetBundleName} dep: {dependencies.ToList().ListToString()}");
            foreach (string dependency in dependencies)
            {
                if (string.IsNullOrEmpty(dependency))
                {
                    continue;
                }

                this.LoadOneBundle(dependency);
            }

            //Log.Debug($"-----------dep load finish {assetBundleName} dep: {dependencies.ToList().ListToString()}");
        }

        private void AddResource(string bundleName, string assetName, UnityEngine.Object resource)
        {
            Dictionary<string, UnityEngine.Object> dict;
            if (!this.resourceCache.TryGetValue(bundleName.BundleNameToLower(), out dict))
            {
                dict = new Dictionary<string, UnityEngine.Object>();
                this.resourceCache[bundleName] = dict;
            }

            dict[assetName] = resource;
        }

        private void LoadOneBundle(string assetBundleName)
        {
            assetBundleName = assetBundleName.BundleNameToLower();
            ABInfo abInfo;
            if (this.bundles.TryGetValue(assetBundleName, out abInfo))
            {
                ++abInfo.RefCount;
                //Log.Debug($"---------------load one bundle {assetBundleName} refcount: {abInfo.RefCount}");
                return;
            }

            if (!Define.IsAsync)
            {
                if (Define.IsEditor)
                {
                    string[] realPath = null;
                    realPath = Define.GetAssetPathsFromAssetBundle(assetBundleName);
                    foreach (string s in realPath)
                    {
                        string assetName = Path.GetFileNameWithoutExtension(s);
                        UnityEngine.Object resource = Define.LoadAssetAtPath(s);
                        AddResource(assetBundleName, assetName, resource);
                    }

                    if (realPath.Length > 0)
                    {
                        abInfo = this.AddChild<ABInfo, string, AssetBundle>(assetBundleName, null);
                        this.bundles[assetBundleName] = abInfo;
                        //Log.Debug($"---------------load one bundle {assetBundleName} refcount: {abInfo.RefCount}");
                    }
                    else
                    {
                        Log.Error($"assets bundle not found: {assetBundleName}");
                    }
                }
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
                // 获取资源的时候会抛异常，这个地方不直接抛异常，因为有些地方需要Load之后判断是否Load成功
                Log.Warning($"assets bundle not found: {assetBundleName}");
                return;
            }

            if (!assetBundle.isStreamedSceneAssetBundle)
            {
                // 异步load资源到内存cache住
                var assets = assetBundle.LoadAllAssets();
                foreach (UnityEngine.Object asset in assets)
                {
                    AddResource(assetBundleName, asset.name, asset);
                }
            }

            abInfo = this.AddChild<ABInfo, string, AssetBundle>(assetBundleName, assetBundle);
            this.bundles[assetBundleName] = abInfo;

            //Log.Debug($"---------------load one bundle {assetBundleName} refcount: {abInfo.RefCount}");
        }

        /// <summary>
        /// 异步加载assetbundle, 加载ab包分两部分，第一部分是从硬盘加载，第二部分加载all assets。两者不能同时并发
        /// </summary>
        public async ETTask LoadBundleAsync(string assetBundleName)
        {
            assetBundleName = assetBundleName.BundleNameToLower();

            string[] dependencies = GetSortedDependencies(assetBundleName);
            //Log.Debug($"-----------dep load async start {assetBundleName} dep: {dependencies.ToList().ListToString()}");

            using (var abInfos = ListComponent<ABInfo>.Create())
            {
                async ETTask LoadDependency(string dependency, List<ABInfo> abInfosList)
                {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, dependency.GetHashCode()))
                    {
                        ABInfo abInfo = await this.LoadOneBundleAsync(dependency);
                        if (abInfo == null || abInfo.RefCount > 1)
                        {
                            return;
                        }

                        abInfosList.Add(abInfo);
                    }
                }

                // LoadFromFileAsync部分可以并发加载
                using (var tasks = ListComponent<ETTask>.Create())
                {
                    foreach (string dependency in dependencies)
                    {
                        tasks.List.Add(LoadDependency(dependency, abInfos.List));
                    }

                    await ETTaskHelper.WaitAll(tasks.List);
                }

                // ab包从硬盘加载完成，可以再并发加载all assets
                using (var tasks = ListComponent<ETTask>.Create())
                {
                    foreach (ABInfo abInfo in abInfos.List)
                    {
                        tasks.List.Add(LoadOneBundleAllAssets(abInfo));
                    }

                    await ETTaskHelper.WaitAll(tasks.List);
                }
            }
        }

        private async ETTask<ABInfo> LoadOneBundleAsync(string assetBundleName)
        {
            assetBundleName = assetBundleName.BundleNameToLower();
            ABInfo abInfo;
            if (this.bundles.TryGetValue(assetBundleName, out abInfo))
            {
                ++abInfo.RefCount;
                //Log.Debug($"---------------load one bundle {assetBundleName} refcount: {abInfo.RefCount}");
                return null;
            }

            string p = "";
            AssetBundle assetBundle = null;

            if (!Define.IsAsync)
            {
                if (Define.IsEditor)
                {
                    string[] realPath = Define.GetAssetPathsFromAssetBundle(assetBundleName);
                    foreach (string s in realPath)
                    {
                        string assetName = Path.GetFileNameWithoutExtension(s);
                        UnityEngine.Object resource = Define.LoadAssetAtPath(s);
                        AddResource(assetBundleName, assetName, resource);
                    }

                    if (realPath.Length > 0)
                    {
                        abInfo = this.AddChild<ABInfo, string, AssetBundle>(assetBundleName, null);
                        this.bundles[assetBundleName] = abInfo;
                        //Log.Debug($"---------------load one bundle {assetBundleName} refcount: {abInfo.RefCount}");
                    }
                    else
                    {
                        Log.Error("Bundle not exist! BundleName: " + assetBundleName);
                    }

                    // 编辑器模式也不能同步加载
                    await TimerComponent.Instance.WaitAsync(100);
                    
                    return abInfo;
                }
            }

            p = Path.Combine(PathHelper.AppHotfixResPath, assetBundleName);
            if (!File.Exists(p))
            {
                p = Path.Combine(PathHelper.AppResPath, assetBundleName);
            }

            Log.Debug("Async load bundle BundleName : " + p);

            // if (!File.Exists(p))
            // {
            //     Log.Error("Async load bundle not exist! BundleName : " + p);
            //     return null;
            // }

            assetBundle = await AssetBundleHelper.UnityLoadBundleAsync(p);

            if (assetBundle == null)
            {
                // 获取资源的时候会抛异常，这个地方不直接抛异常，因为有些地方需要Load之后判断是否Load成功
                Log.Warning($"assets bundle not found: {assetBundleName}");
                return null;
            }

            abInfo = this.AddChild<ABInfo, string, AssetBundle>(assetBundleName, assetBundle);
            this.bundles[assetBundleName] = abInfo;
            return abInfo;
            //Log.Debug($"---------------load one bundle {assetBundleName} refcount: {abInfo.RefCount}");
        }

        // 加载ab包中的all assets
        private async ETTask LoadOneBundleAllAssets(ABInfo abInfo)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, abInfo.Name.GetHashCode()))
            {
                if (abInfo.IsDisposed || abInfo.AlreadyLoadAssets)
                {
                    return;
                }

                if (abInfo.AssetBundle != null && !abInfo.AssetBundle.isStreamedSceneAssetBundle)
                {
                    // 异步load资源到内存cache住
                    var assets = await AssetBundleHelper.UnityLoadAssetAsync(abInfo.AssetBundle);

                    foreach (UnityEngine.Object asset in assets)
                    {
                        AddResource(abInfo.Name, asset.name, asset);
                    }
                }

                abInfo.AlreadyLoadAssets = true;
            }
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