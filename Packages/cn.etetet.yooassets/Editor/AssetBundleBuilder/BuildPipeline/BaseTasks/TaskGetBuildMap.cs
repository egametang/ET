using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskGetBuildMap
    {
        /// <summary>
        /// 生成资源构建上下文
        /// </summary>
        public BuildMapContext CreateBuildMap(BuildParameters buildParameters)
        {
            BuildMapContext context = new BuildMapContext();
            var buildMode = buildParameters.BuildMode;
            var packageName = buildParameters.PackageName;

            Dictionary<string, BuildAssetInfo> allBuildAssetInfos = new Dictionary<string, BuildAssetInfo>(1000);

            // 1. 获取所有收集器收集的资源
            var collectResult = AssetBundleCollectorSettingData.Setting.GetPackageAssets(buildMode, packageName);
            List<CollectAssetInfo> allCollectAssets = collectResult.CollectAssets;

            // 2. 剔除未被引用的依赖项资源
            RemoveZeroReferenceAssets(context, allCollectAssets);

            // 3. 录入所有收集器主动收集的资源
            foreach (var collectAssetInfo in allCollectAssets)
            {
                if (allBuildAssetInfos.ContainsKey(collectAssetInfo.AssetInfo.AssetPath))
                {
                    throw new Exception($"Should never get here !");
                }

                if (collectAssetInfo.CollectorType != ECollectorType.MainAssetCollector)
                {
                    if (collectAssetInfo.AssetTags.Count > 0)
                    {
                        collectAssetInfo.AssetTags.Clear();
                        string warning = BuildLogger.GetErrorMessage(ErrorCode.RemoveInvalidTags, $"Remove asset tags that don't work, see the asset collector type : {collectAssetInfo.AssetInfo.AssetPath}");
                        BuildLogger.Warning(warning);
                    }
                }

                var buildAssetInfo = new BuildAssetInfo(collectAssetInfo.CollectorType, collectAssetInfo.BundleName, collectAssetInfo.Address, collectAssetInfo.AssetInfo);
                buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
                allBuildAssetInfos.Add(collectAssetInfo.AssetInfo.AssetPath, buildAssetInfo);
            }

            // 4. 录入所有收集资源依赖的其它资源
            foreach (var collectAssetInfo in allCollectAssets)
            {
                string bundleName = collectAssetInfo.BundleName;
                foreach (var dependAsset in collectAssetInfo.DependAssets)
                {
                    if (allBuildAssetInfos.ContainsKey(dependAsset.AssetPath))
                    {
                        allBuildAssetInfos[dependAsset.AssetPath].AddReferenceBundleName(bundleName);
                    }
                    else
                    {
                        var buildAssetInfo = new BuildAssetInfo(dependAsset);
                        buildAssetInfo.AddReferenceBundleName(bundleName);
                        allBuildAssetInfos.Add(dependAsset.AssetPath, buildAssetInfo);
                    }
                }
            }

            // 5. 填充所有收集资源的依赖列表
            foreach (var collectAssetInfo in allCollectAssets)
            {
                var dependAssetInfos = new List<BuildAssetInfo>(collectAssetInfo.DependAssets.Count);
                foreach (var dependAsset in collectAssetInfo.DependAssets)
                {
                    if (allBuildAssetInfos.TryGetValue(dependAsset.AssetPath, out BuildAssetInfo value))
                        dependAssetInfos.Add(value);
                    else
                        throw new Exception("Should never get here !");
                }
                allBuildAssetInfos[collectAssetInfo.AssetInfo.AssetPath].SetDependAssetInfos(dependAssetInfos);
            }

            // 6. 自动收集所有依赖的着色器
            if (collectResult.Command.AutoCollectShaders)
            {
                // 获取着色器打包规则结果
                PackRuleResult shaderPackRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
                string shaderBundleName = shaderPackRuleResult.GetBundleName(collectResult.Command.PackageName, collectResult.Command.UniqueBundleName);
                foreach (var buildAssetInfo in allBuildAssetInfos.Values)
                {
                    if (buildAssetInfo.CollectorType == ECollectorType.None)
                    {
                        if (buildAssetInfo.AssetInfo.IsShaderAsset())
                        {
                            buildAssetInfo.SetBundleName(shaderBundleName);
                        }
                    }
                }
            }

            // 7. 计算共享资源的包名
            if (buildParameters.EnableSharePackRule)
            {
                PreProcessPackShareBundle(buildParameters, collectResult.Command, allBuildAssetInfos);
                foreach (var buildAssetInfo in allBuildAssetInfos.Values)
                {
                    if (buildAssetInfo.HasBundleName() == false)
                    {
                        PackRuleResult packRuleResult = GetShareBundleName(buildAssetInfo);
                        if (packRuleResult.IsValid())
                        {
                            string shareBundleName = packRuleResult.GetShareBundleName(collectResult.Command.PackageName, collectResult.Command.UniqueBundleName);
                            buildAssetInfo.SetBundleName(shareBundleName);
                        }
                    }
                }
                PostProcessPackShareBundle();
            }

            // 8. 记录关键信息
            context.AssetFileCount = allBuildAssetInfos.Count;
            context.Command = collectResult.Command;

            // 9. 移除不参与构建的资源
            List<BuildAssetInfo> removeBuildList = new List<BuildAssetInfo>();
            foreach (var buildAssetInfo in allBuildAssetInfos.Values)
            {
                if (buildAssetInfo.HasBundleName() == false)
                    removeBuildList.Add(buildAssetInfo);
            }
            foreach (var removeValue in removeBuildList)
            {
                allBuildAssetInfos.Remove(removeValue.AssetInfo.AssetPath);
            }

            // 10. 构建资源列表
            var allPackAssets = allBuildAssetInfos.Values.ToList();
            if (allPackAssets.Count == 0)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.PackAssetListIsEmpty, "The pack asset info is empty !");
                throw new Exception(message);
            }
            foreach (var assetInfo in allPackAssets)
            {
                context.PackAsset(assetInfo);
            }

            return context;
        }
        private void RemoveZeroReferenceAssets(BuildMapContext context, List<CollectAssetInfo> allCollectAssets)
        {
            // 1. 检测依赖资源收集器是否存在
            if (allCollectAssets.Exists(x => x.CollectorType == ECollectorType.DependAssetCollector) == false)
                return;

            // 2. 获取所有主资源的依赖资源集合
            HashSet<string> allDependAsset = new HashSet<string>();
            foreach (var collectAsset in allCollectAssets)
            {
                var collectorType = collectAsset.CollectorType;
                if (collectorType == ECollectorType.MainAssetCollector || collectorType == ECollectorType.StaticAssetCollector)
                {
                    foreach (var dependAsset in collectAsset.DependAssets)
                    {
                        if (allDependAsset.Contains(dependAsset.AssetPath) == false)
                            allDependAsset.Add(dependAsset.AssetPath);
                    }
                }
            }

            // 3. 找出所有零引用的依赖资源集合
            List<CollectAssetInfo> removeList = new List<CollectAssetInfo>();
            foreach (var collectAssetInfo in allCollectAssets)
            {
                var collectorType = collectAssetInfo.CollectorType;
                if (collectorType == ECollectorType.DependAssetCollector)
                {
                    if (allDependAsset.Contains(collectAssetInfo.AssetInfo.AssetPath) == false)
                        removeList.Add(collectAssetInfo);
                }
            }

            // 4. 移除所有零引用的依赖资源
            foreach (var removeValue in removeList)
            {
                string warning = BuildLogger.GetErrorMessage(ErrorCode.FoundUndependedAsset, $"Found undepended asset and remove it : {removeValue.AssetInfo.AssetPath}");
                BuildLogger.Warning(warning);

                var independAsset = new ReportIndependAsset();
                independAsset.AssetPath = removeValue.AssetInfo.AssetPath;
                independAsset.AssetGUID = removeValue.AssetInfo.AssetGUID;
                independAsset.AssetType = removeValue.AssetInfo.AssetType.ToString();
                independAsset.FileSize = FileUtility.GetFileSize(removeValue.AssetInfo.AssetPath);
                context.IndependAssets.Add(independAsset);

                allCollectAssets.Remove(removeValue);
            }
        }

        #region 共享资源打包规则
        /// <summary>
        /// 共享资源打包前置处理
        /// </summary>
        protected virtual void PreProcessPackShareBundle(BuildParameters buildParameters, CollectCommand command, Dictionary<string, BuildAssetInfo> allBuildAssetInfos)
        {
        }

        /// <summary>
        /// 共享资源打包后置处理
        /// </summary>
        protected virtual void PostProcessPackShareBundle()
        {
        }

        /// <summary>
        /// 获取共享资源包名称
        /// </summary>
        protected virtual PackRuleResult GetShareBundleName(BuildAssetInfo buildAssetInfo)
        {
            string bundleName = Path.GetDirectoryName(buildAssetInfo.AssetInfo.AssetPath);
            PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
        #endregion
    }
}