using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
	public class ManifestContext : IContextObject
	{
		internal PackageManifest Manifest;
	}

	[TaskAttribute("创建清单文件")]
	public class TaskCreateManifest : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			CreateManifestFile(context);
		}

		/// <summary>
		/// 创建补丁清单文件到输出目录
		/// </summary>
		private void CreateManifestFile(BuildContext context)
		{
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildParameters = buildParametersContext.Parameters;
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();

			// 创建新补丁清单
			PackageManifest manifest = new PackageManifest();
			manifest.FileVersion = YooAssetSettings.ManifestFileVersion;
			manifest.EnableAddressable = buildMapContext.EnableAddressable;
			manifest.OutputNameStyle = (int)buildParameters.OutputNameStyle;
			manifest.PackageName = buildParameters.PackageName;
			manifest.PackageVersion = buildParameters.PackageVersion;
			manifest.BundleList = GetAllPackageBundle(context);
			manifest.AssetList = GetAllPackageAsset(context, manifest);

			// 更新Unity内置资源包的引用关系
			if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				if (buildParameters.BuildMode == EBuildMode.IncrementalBuild)
				{
					var buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
					UpdateBuiltInBundleReference(manifest, buildResultContext, buildMapContext.ShadersBundleName);
				}
			}

			// 更新资源包之间的引用关系
			if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				if (buildParameters.BuildMode == EBuildMode.IncrementalBuild)
				{
					var buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
					UpdateScriptPipelineReference(manifest, buildResultContext);
				}
			}

			// 更新资源包之间的引用关系
			if (buildParameters.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
				{
					var buildResultContext = context.GetContextObject<TaskBuilding.BuildResultContext>();
					UpdateBuiltinPipelineReference(manifest, buildResultContext);
				}
			}

			// 创建补丁清单文本文件
			{
				string fileName = YooAssetSettingsData.GetManifestJsonFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				ManifestTools.SerializeToJson(filePath, manifest);
				BuildLogger.Log($"创建补丁清单文件：{filePath}");
			}

			// 创建补丁清单二进制文件
			string packageHash;
			{
				string fileName = YooAssetSettingsData.GetManifestBinaryFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				ManifestTools.SerializeToBinary(filePath, manifest);
				packageHash = HashUtility.FileMD5(filePath);
				BuildLogger.Log($"创建补丁清单文件：{filePath}");

				ManifestContext manifestContext = new ManifestContext();
				byte[] bytesData = FileUtility.ReadAllBytes(filePath);
				manifestContext.Manifest = ManifestTools.DeserializeFromBinary(bytesData);
				context.SetContextObject(manifestContext);
			}

			// 创建补丁清单哈希文件
			{
				string fileName = YooAssetSettingsData.GetPackageHashFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				FileUtility.CreateFile(filePath, packageHash);
				BuildLogger.Log($"创建补丁清单哈希文件：{filePath}");
			}

			// 创建补丁清单版本文件
			{
				string fileName = YooAssetSettingsData.GetPackageVersionFileName(buildParameters.PackageName);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				FileUtility.CreateFile(filePath, buildParameters.PackageVersion);
				BuildLogger.Log($"创建补丁清单版本文件：{filePath}");
			}
		}

		/// <summary>
		/// 获取资源包列表
		/// </summary>
		private List<PackageBundle> GetAllPackageBundle(BuildContext context)
		{
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			List<PackageBundle> result = new List<PackageBundle>(1000);
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				var packageBundle = bundleInfo.CreatePackageBundle();
				result.Add(packageBundle);
			}
			return result;
		}

		/// <summary>
		/// 获取资源列表
		/// </summary>
		private List<PackageAsset> GetAllPackageAsset(BuildContext context, PackageManifest manifest)
		{
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			List<PackageAsset> result = new List<PackageAsset>(1000);
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				var assetInfos = bundleInfo.GetAllMainAssetInfos();
				foreach (var assetInfo in assetInfos)
				{
					PackageAsset packageAsset = new PackageAsset();
					if (buildMapContext.EnableAddressable)
						packageAsset.Address = assetInfo.Address;
					else
						packageAsset.Address = string.Empty;
					packageAsset.AssetPath = assetInfo.AssetPath;
					packageAsset.AssetTags = assetInfo.AssetTags.ToArray();
					packageAsset.BundleID = GetAssetBundleID(assetInfo.BundleName, manifest);
					packageAsset.DependIDs = GetAssetBundleDependIDs(packageAsset.BundleID, assetInfo, manifest);
					result.Add(packageAsset);
				}
			}
			return result;
		}
		private int[] GetAssetBundleDependIDs(int mainBundleID, BuildAssetInfo assetInfo, PackageManifest manifest)
		{
			List<int> result = new List<int>();
			foreach (var dependAssetInfo in assetInfo.AllDependAssetInfos)
			{
				if (dependAssetInfo.HasBundleName())
				{
					int bundleID = GetAssetBundleID(dependAssetInfo.BundleName, manifest);
					if (mainBundleID != bundleID)
					{
						if (result.Contains(bundleID) == false)
							result.Add(bundleID);
					}
				}
			}
			return result.ToArray();
		}
		private int GetAssetBundleID(string bundleName, PackageManifest manifest)
		{
			for (int index = 0; index < manifest.BundleList.Count; index++)
			{
				if (manifest.BundleList[index].BundleName == bundleName)
					return index;
			}
			throw new Exception($"Not found bundle name : {bundleName}");
		}

		/// <summary>
		/// 更新Unity内置资源包的引用关系
		/// </summary>
		private void UpdateBuiltInBundleReference(PackageManifest manifest, TaskBuilding_SBP.BuildResultContext buildResultContext, string shadersBunldeName)
		{
			// 获取所有依赖着色器资源包的资源包列表
			List<string> shaderBundleReferenceList = new List<string>();
			foreach (var valuePair in buildResultContext.Results.BundleInfos)
			{
				if (valuePair.Value.Dependencies.Any(t => t == shadersBunldeName))
					shaderBundleReferenceList.Add(valuePair.Key);
			}

			// 注意：没有任何资源依赖着色器
			if (shaderBundleReferenceList.Count == 0)
				return;

			// 获取着色器资源包索引
			Predicate<PackageBundle> predicate = new Predicate<PackageBundle>(s => s.BundleName == shadersBunldeName);
			int shaderBundleId = manifest.BundleList.FindIndex(predicate);
			if (shaderBundleId == -1)
				throw new Exception("没有发现着色器资源包！");

			// 检测依赖交集并更新依赖ID
			foreach (var packageAsset in manifest.AssetList)
			{
				List<string> dependBundles = GetPackageAssetAllDependBundles(manifest, packageAsset);
				List<string> conflictAssetPathList = dependBundles.Intersect(shaderBundleReferenceList).ToList();
				if (conflictAssetPathList.Count > 0)
				{
					List<int> newDependIDs = new List<int>(packageAsset.DependIDs);
					if (newDependIDs.Contains(shaderBundleId) == false)
						newDependIDs.Add(shaderBundleId);
					packageAsset.DependIDs = newDependIDs.ToArray();
				}
			}
		}
		private List<string> GetPackageAssetAllDependBundles(PackageManifest manifest, PackageAsset packageAsset)
		{
			List<string> result = new List<string>();
			string mainBundle = manifest.BundleList[packageAsset.BundleID].BundleName;
			result.Add(mainBundle);
			foreach (var dependID in packageAsset.DependIDs)
			{
				string dependBundle = manifest.BundleList[dependID].BundleName;
				result.Add(dependBundle);
			}
			return result;
		}

		#region 资源包引用关系相关
		private readonly Dictionary<string, int> _cachedBundleID = new Dictionary<string, int>(10000);
		private readonly Dictionary<string, string[]> _cachedBundleDepends = new Dictionary<string, string[]>(10000);

		private void UpdateScriptPipelineReference(PackageManifest manifest, TaskBuilding_SBP.BuildResultContext buildResultContext)
		{
			int progressValue;
			int totalCount = manifest.BundleList.Count;

			// 缓存资源包ID
			_cachedBundleID.Clear();
			progressValue = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				int bundleID = GetAssetBundleID(packageBundle.BundleName, manifest);
				_cachedBundleID.Add(packageBundle.BundleName, bundleID);
				EditorTools.DisplayProgressBar("缓存资源包索引", ++progressValue, totalCount);
			}
			EditorTools.ClearProgressBar();

			// 缓存资源包依赖
			_cachedBundleDepends.Clear();
			progressValue = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				if (packageBundle.IsRawFile)
				{
					_cachedBundleDepends.Add(packageBundle.BundleName, new string[] { });
					continue;
				}

				if (buildResultContext.Results.BundleInfos.ContainsKey(packageBundle.BundleName) == false)
					throw new Exception($"Not found bundle in SBP build results : {packageBundle.BundleName}");

				var depends = buildResultContext.Results.BundleInfos[packageBundle.BundleName].Dependencies;
				_cachedBundleDepends.Add(packageBundle.BundleName, depends);
				EditorTools.DisplayProgressBar("缓存资源包依赖列表", ++progressValue, totalCount);
			}
			EditorTools.ClearProgressBar();

			// 计算资源包引用列表
			foreach (var packageBundle in manifest.BundleList)
			{
				packageBundle.ReferenceIDs = GetBundleRefrenceIDs(manifest, packageBundle);
				EditorTools.DisplayProgressBar("计算资源包引用关系", ++progressValue, totalCount);
			}
			EditorTools.ClearProgressBar();
		}
		private void UpdateBuiltinPipelineReference(PackageManifest manifest, TaskBuilding.BuildResultContext buildResultContext)
		{
			int progressValue;
			int totalCount = manifest.BundleList.Count;

			// 缓存资源包ID
			_cachedBundleID.Clear();
			progressValue = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				int bundleID = GetAssetBundleID(packageBundle.BundleName, manifest);
				_cachedBundleID.Add(packageBundle.BundleName, bundleID);
				EditorTools.DisplayProgressBar("缓存资源包索引", ++progressValue, totalCount);
			}
			EditorTools.ClearProgressBar();

			// 缓存资源包依赖
			_cachedBundleDepends.Clear();
			progressValue = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				if (packageBundle.IsRawFile)
				{
					_cachedBundleDepends.Add(packageBundle.BundleName, new string[] { } );
					continue;
				}

				var depends = buildResultContext.UnityManifest.GetDirectDependencies(packageBundle.BundleName);
				_cachedBundleDepends.Add(packageBundle.BundleName, depends);
				EditorTools.DisplayProgressBar("缓存资源包依赖列表", ++progressValue, totalCount);
			}
			EditorTools.ClearProgressBar();

			// 计算资源包引用列表
			progressValue = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				packageBundle.ReferenceIDs = GetBundleRefrenceIDs(manifest, packageBundle);
				EditorTools.DisplayProgressBar("计算资源包引用关系", ++progressValue, totalCount);
			}
			EditorTools.ClearProgressBar();
		}
		
		private int[] GetBundleRefrenceIDs(PackageManifest manifest, PackageBundle targetBundle)
		{
			List<string> referenceList = new List<string>();
			foreach (var packageBundle in manifest.BundleList)
			{
				string bundleName = packageBundle.BundleName;
				if (bundleName == targetBundle.BundleName)
					continue;

				string[] dependencies = GetCachedBundleDepends(bundleName);
				if (dependencies.Contains(targetBundle.BundleName))
				{
					referenceList.Add(bundleName);
				}
			}

			List<int> result = new List<int>();
			foreach (var bundleName in referenceList)
			{
				int bundleID = GetCachedBundleID(bundleName);
				if (result.Contains(bundleID) == false)
					result.Add(bundleID);
			}
			return result.ToArray();
		}
		private int GetCachedBundleID(string bundleName)
		{
			if (_cachedBundleID.TryGetValue(bundleName, out int value) == false)
			{
				throw new Exception($"Not found cached bundle ID : {bundleName}");
			}
			return value;
		}
		private string[] GetCachedBundleDepends(string bundleName)
		{
			if (_cachedBundleDepends.TryGetValue(bundleName, out string[] value) == false)
			{
				throw new Exception($"Not found cached bundle depends : {bundleName}");
			}
			return value;
		}
		#endregion
	}
}