using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskPrepare_SBP : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;

            // 检测基础构建参数
            buildParametersContext.CheckBuildParameters();

            // 检测不被支持的构建模式
            if (buildParameters.BuildMode == EBuildMode.DryRunBuild)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildPipelineNotSupportBuildMode, $"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.DryRunBuild)} build mode !");
                throw new Exception(message);
            }
            if (buildParameters.BuildMode == EBuildMode.ForceRebuild)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildPipelineNotSupportBuildMode, $"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.ForceRebuild)} build mode !");
                throw new Exception(message);
            }
        }
    }
}