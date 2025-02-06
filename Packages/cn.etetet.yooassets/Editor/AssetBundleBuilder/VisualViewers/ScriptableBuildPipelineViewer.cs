#if UNITY_2019_4_OR_NEWER
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class ScriptableBuildPipelineViewer : BuildPipelineViewerBase
    {
        public ScriptableBuildPipelineViewer(string packageName, BuildTarget buildTarget, VisualElement parent)
            : base(packageName, EBuildPipeline.ScriptableBuildPipeline, buildTarget, parent)
        {
        }

        /// <summary>
        /// 执行构建
        /// </summary>
        protected override void ExecuteBuild()
        {
            var buildMode = AssetBundleBuilderSetting.GetPackageBuildMode(PackageName, BuildPipeline);
            var fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(PackageName, BuildPipeline);
            var buildinFileCopyOption = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(PackageName, BuildPipeline);
            var buildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(PackageName, BuildPipeline);
            var compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(PackageName, BuildPipeline);

            ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = BuildPipeline.ToString();
            buildParameters.BuildTarget = BuildTarget;
            buildParameters.BuildMode = buildMode;
            buildParameters.PackageName = PackageName;
            buildParameters.PackageVersion = GetPackageVersion();
            buildParameters.EnableSharePackRule = true;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = fileNameStyle;
            buildParameters.BuildinFileCopyOption = buildinFileCopyOption;
            buildParameters.BuildinFileCopyParams = buildinFileCopyParams;
            buildParameters.EncryptionServices = CreateEncryptionInstance();
            buildParameters.CompressOption = compressOption;

            ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
        }

        protected override List<Enum> GetSupportBuildModes()
        {
            List<Enum> buildModeList = new List<Enum>();
            buildModeList.Add(EBuildMode.IncrementalBuild);
            buildModeList.Add(EBuildMode.SimulateBuild);
            return buildModeList;
        }
    }
}
#endif