using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskUpdateBundleInfo_BBP : TaskUpdateBundleInfo, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            UpdateBundleInfo(context);
        }

        protected override string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var parameters = buildParametersContext.Parameters;
            var buildMode = parameters.BuildMode;
            if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
            {
                return "00000000000000000000000000000000"; //32位
            }
            else
            {
                var buildResult = context.GetContextObject<TaskBuilding_BBP.BuildResultContext>();
                var hash = buildResult.UnityManifest.GetAssetBundleHash(bundleInfo.BundleName);
                if (hash.isValid)
                {
                    return hash.ToString();
                }
                else
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleHash, $"Not found unity bundle hash : {bundleInfo.BundleName}");
                    throw new Exception(message);
                }
            }
        }
        protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var parameters = buildParametersContext.Parameters;
            var buildMode = parameters.BuildMode;
            if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
            {
                return 0;
            }
            else
            {
                string filePath = bundleInfo.BuildOutputFilePath;
                if (BuildPipeline.GetCRCForAssetBundle(filePath, out uint crc))
                {
                    return crc;
                }
                else
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleCRC, $"Not found unity bundle crc : {bundleInfo.BundleName}");
                    throw new Exception(message);
                }
            }
        }
        protected override string GetBundleFileHash(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            var buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
                return GetFilePathTempHash(filePath);
            else
                return HashUtility.FileMD5(filePath);
        }
        protected override string GetBundleFileCRC(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            var buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
                return "00000000"; //8位
            else
                return HashUtility.FileCRC32(filePath);
        }
        protected override long GetBundleFileSize(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            var buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
                return GetBundleTempSize(bundleInfo);
            else
                return FileUtility.GetFileSize(filePath);
        }
    }
}