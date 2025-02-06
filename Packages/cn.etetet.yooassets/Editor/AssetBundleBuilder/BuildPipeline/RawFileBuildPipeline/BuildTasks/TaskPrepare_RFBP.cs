using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskPrepare_RFBP : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;

            // 检测基础构建参数
            buildParametersContext.CheckBuildParameters();

            // 检测不被支持的参数
            if (buildParameters.EnableSharePackRule)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildPipelineNotSupportSharePackRule, $"{nameof(EBuildPipeline.RawFileBuildPipeline)} not support share pack rule !");
                throw new Exception(message);
            }

            // 检测不被支持的构建模式
            if (buildParameters.BuildMode == EBuildMode.DryRunBuild)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildPipelineNotSupportBuildMode, $"{nameof(EBuildPipeline.RawFileBuildPipeline)} not support {nameof(EBuildMode.DryRunBuild)} build mode !");
                throw new Exception(message);
            }
            if (buildParameters.BuildMode == EBuildMode.IncrementalBuild)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildPipelineNotSupportBuildMode, $"{nameof(EBuildPipeline.RawFileBuildPipeline)} not support {nameof(EBuildMode.IncrementalBuild)} build mode !");
                throw new Exception(message);
            }
        }
    }
}