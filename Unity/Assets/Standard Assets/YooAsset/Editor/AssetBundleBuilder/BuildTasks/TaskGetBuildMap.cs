using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	[TaskAttribute("获取资源构建内容")]
	public class TaskGetBuildMap : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildParameters = buildParametersContext.Parameters;
			var buildMapContext = CreateBuildMap(buildParameters.BuildMode, buildParameters.ShareAssetPackRule, buildParameters.PackageName);
			context.SetContextObject(buildMapContext);
			BuildLogger.Log("构建内容准备完毕！");

			// 检测构建结果
			CheckBuildMapContent(buildMapContext);
		}

		/// <summary>
		/// 资源构建上下文
		/// </summary>
		public BuildMapContext CreateBuildMap(EBuildMode buildMode, IShareAssetPackRule packRule, string packageName)
		{
			Dictionary<string, BuildAssetInfo> allBuildAssetInfoDic = new Dictionary<string, BuildAssetInfo>(1000);

			// 1. 检测配置合法性
			AssetBundleCollectorSettingData.Setting.CheckConfigError();

			// 2. 获取所有收集器收集的资源
			var collectResult = AssetBundleCollectorSettingData.Setting.GetPackageAssets(buildMode, packageName);
			List<CollectAssetInfo> allCollectAssetInfos = collectResult.CollectAssets;

			// 3. 剔除未被引用的依赖项资源
			RemoveZeroReferenceAssets(allCollectAssetInfos);

			// 4. 录入所有收集器收集的资源
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				if (allBuildAssetInfoDic.ContainsKey(collectAssetInfo.AssetPath) == false)
				{
					var buildAssetInfo = new BuildAssetInfo(collectAssetInfo.CollectorType, collectAssetInfo.BundleName,
						collectAssetInfo.Address, collectAssetInfo.AssetPath, collectAssetInfo.IsRawAsset);
					buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
					buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
					allBuildAssetInfoDic.Add(collectAssetInfo.AssetPath, buildAssetInfo);
				}
				else
				{
					throw new Exception($"Should never get here !");
				}
			}

			// 5. 录入所有收集资源的依赖资源
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				string collectAssetBundleName = collectAssetInfo.BundleName;
				foreach (var dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (allBuildAssetInfoDic.ContainsKey(dependAssetPath))
					{
						allBuildAssetInfoDic[dependAssetPath].AddBundleTags(collectAssetInfo.AssetTags);
						allBuildAssetInfoDic[dependAssetPath].AddReferenceBundleName(collectAssetBundleName);
					}
					else
					{
						var buildAssetInfo = new BuildAssetInfo(dependAssetPath);
						buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
						buildAssetInfo.AddReferenceBundleName(collectAssetBundleName);
						allBuildAssetInfoDic.Add(dependAssetPath, buildAssetInfo);
					}
				}
			}

			// 6. 填充所有收集资源的依赖列表
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				var dependAssetInfos = new List<BuildAssetInfo>(collectAssetInfo.DependAssets.Count);
				foreach (var dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (allBuildAssetInfoDic.TryGetValue(dependAssetPath, out BuildAssetInfo value))
						dependAssetInfos.Add(value);
					else
						throw new Exception("Should never get here !");
				}
				allBuildAssetInfoDic[collectAssetInfo.AssetPath].SetAllDependAssetInfos(dependAssetInfos);
			}

			// 7. 记录关键信息
			BuildMapContext context = new BuildMapContext();
			context.AssetFileCount = allBuildAssetInfoDic.Count;
			context.EnableAddressable = collectResult.Command.EnableAddressable;
			context.UniqueBundleName = collectResult.Command.UniqueBundleName;
			context.ShadersBundleName = collectResult.Command.ShadersBundleName;

			// 8. 计算共享的资源包名
			var command = collectResult.Command;
			foreach (var buildAssetInfo in allBuildAssetInfoDic.Values)
			{
				buildAssetInfo.CalculateShareBundleName(packRule, command.UniqueBundleName, command.PackageName, command.ShadersBundleName);
			}

			// 9. 移除不参与构建的资源
			List<BuildAssetInfo> removeBuildList = new List<BuildAssetInfo>();
			foreach (var buildAssetInfo in allBuildAssetInfoDic.Values)
			{
				if (buildAssetInfo.HasBundleName() == false)
					removeBuildList.Add(buildAssetInfo);
			}
			foreach (var removeValue in removeBuildList)
			{
				allBuildAssetInfoDic.Remove(removeValue.AssetPath);
			}

			// 10. 构建资源包
			var allPackAssets = allBuildAssetInfoDic.Values.ToList();
			if (allPackAssets.Count == 0)
				throw new Exception("构建的资源列表不能为空");
			foreach (var assetInfo in allPackAssets)
			{
				context.PackAsset(assetInfo);
			}
			return context;
		}
		private void RemoveZeroReferenceAssets(List<CollectAssetInfo> allCollectAssetInfos)
		{
			// 1. 检测是否任何存在依赖资源
			bool hasAnyDependAsset = false;
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				var collectorType = collectAssetInfo.CollectorType;
				if (collectorType == ECollectorType.DependAssetCollector)
				{
					hasAnyDependAsset = true;
					break;
				}
			}
			if (hasAnyDependAsset == false)
				return;

			// 2. 获取所有主资源的依赖资源集合
			HashSet<string> allDependAsset = new HashSet<string>();
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				var collectorType = collectAssetInfo.CollectorType;
				if (collectorType == ECollectorType.MainAssetCollector || collectorType == ECollectorType.StaticAssetCollector)
				{
					foreach (var dependAsset in collectAssetInfo.DependAssets)
					{
						if (allDependAsset.Contains(dependAsset) == false)
							allDependAsset.Add(dependAsset);
					}
				}
			}

			// 3. 找出所有零引用的依赖资源集合
			List<CollectAssetInfo> removeList = new List<CollectAssetInfo>();
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				var collectorType = collectAssetInfo.CollectorType;
				if (collectorType == ECollectorType.DependAssetCollector)
				{
					if (allDependAsset.Contains(collectAssetInfo.AssetPath) == false)
						removeList.Add(collectAssetInfo);
				}
			}

			// 4. 移除所有零引用的依赖资源
			foreach (var removeValue in removeList)
			{
				BuildLogger.Log($"发现未被依赖的资源并自动移除 : {removeValue.AssetPath}");
				allCollectAssetInfos.Remove(removeValue);
			}
		}

		/// <summary>
		/// 检测构建结果
		/// </summary>
		private void CheckBuildMapContent(BuildMapContext buildMapContext)
		{
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				// 注意：原生文件资源包只能包含一个原生文件
				bool isRawFile = bundleInfo.IsRawFile;
				if (isRawFile)
				{
					if (bundleInfo.AllMainAssets.Count != 1)
						throw new Exception($"The bundle does not support multiple raw asset : {bundleInfo.BundleName}");
					continue;
				}

				// 注意：原生文件不能被其它资源文件依赖
				foreach (var assetInfo in bundleInfo.AllMainAssets)
				{
					if (assetInfo.AllDependAssetInfos != null)
					{
						foreach (var dependAssetInfo in assetInfo.AllDependAssetInfos)
						{
							if (dependAssetInfo.IsRawAsset)
								throw new Exception($"{assetInfo.AssetPath} can not depend raw asset : {dependAssetInfo.AssetPath}");
						}
					}
				}
			}
		}
	}
}