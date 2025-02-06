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
    internal class BuiltinBuildPipelineViewer : BuildPipelineViewerBase
    {
        public BuiltinBuildPipelineViewer(string packageName, BuildTarget buildTarget, VisualElement parent)
            : base(packageName, EBuildPipeline.BuiltinBuildPipeline, buildTarget, parent)
        {
        }

        /// <summary>
        /// Ö´ÐÐ¹¹½¨
        /// </summary>
        protected override void ExecuteBuild()
        {
            var buildMode = AssetBundleBuilderSetting.GetPackageBuildMode(PackageName, BuildPipeline);
            var fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(PackageName, BuildPipeline);
            var buildinFileCopyOption = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(PackageName, BuildPipeline);
            var buildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(PackageName, BuildPipeline);
            var compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(PackageName, BuildPipeline);

            BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
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

            BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
        }

        protected override List<Enum> GetSupportBuildModes()
        {
            List<Enum> buildModeList = new List<Enum>();
            buildModeList.Add(EBuildMode.ForceRebuild);
            buildModeList.Add(EBuildMode.IncrementalBuild);
            buildModeList.Add(EBuildMode.DryRunBuild);
            buildModeList.Add(EBuildMode.SimulateBuild);
            return buildModeList;
        }
    }
}
#endif