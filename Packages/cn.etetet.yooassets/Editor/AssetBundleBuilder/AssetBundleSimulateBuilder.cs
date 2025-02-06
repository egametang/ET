using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public static class AssetBundleSimulateBuilder
    {
        /// <summary>
        /// 模拟构建
        /// </summary>
        public static string SimulateBuild(string buildPipelineName, string packageName)
        {
            if (buildPipelineName == EBuildPipeline.BuiltinBuildPipeline.ToString())
            {
                BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
                buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                buildParameters.BuildPipeline = buildPipelineName;
                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildParameters.BuildMode = EBuildMode.SimulateBuild;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = "Simulate";
                buildParameters.FileNameStyle = EFileNameStyle.HashName;
                buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
                buildParameters.BuildinFileCopyParams = string.Empty;

                BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
                var buildResult = pipeline.Run(buildParameters, false);
                if (buildResult.Success)
                {
                    string manifestFileName = YooAssetSettingsData.GetManifestBinaryFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                    string manifestFilePath = $"{buildResult.OutputPackageDirectory}/{manifestFileName}";
                    return manifestFilePath;
                }
                else
                {
                    return null;
                }
            }
            else if (buildPipelineName == EBuildPipeline.ScriptableBuildPipeline.ToString())
            {
                ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
                buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                buildParameters.BuildPipeline = buildPipelineName;
                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildParameters.BuildMode = EBuildMode.SimulateBuild;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = "Simulate";
                buildParameters.FileNameStyle = EFileNameStyle.HashName;
                buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
                buildParameters.BuildinFileCopyParams = string.Empty;

                ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
                var buildResult = pipeline.Run(buildParameters, true);
                if (buildResult.Success)
                {
                    string manifestFileName = YooAssetSettingsData.GetManifestBinaryFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                    string manifestFilePath = $"{buildResult.OutputPackageDirectory}/{manifestFileName}";
                    return manifestFilePath;
                }
                else
                {
                    return null;
                }
            }
            else if (buildPipelineName == EBuildPipeline.RawFileBuildPipeline.ToString())
            {
                RawFileBuildParameters buildParameters = new RawFileBuildParameters();
                buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                buildParameters.BuildPipeline = buildPipelineName;
                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildParameters.BuildMode = EBuildMode.SimulateBuild;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = "Simulate";
                buildParameters.FileNameStyle = EFileNameStyle.HashName;
                buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
                buildParameters.BuildinFileCopyParams = string.Empty;

                RawFileBuildPipeline pipeline = new RawFileBuildPipeline();
                var buildResult = pipeline.Run(buildParameters, true);
                if (buildResult.Success)
                {
                    string manifestFileName = YooAssetSettingsData.GetManifestBinaryFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                    string manifestFilePath = $"{buildResult.OutputPackageDirectory}/{manifestFileName}";
                    return manifestFilePath;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new System.NotImplementedException(buildPipelineName);
            }
        }
    }
}