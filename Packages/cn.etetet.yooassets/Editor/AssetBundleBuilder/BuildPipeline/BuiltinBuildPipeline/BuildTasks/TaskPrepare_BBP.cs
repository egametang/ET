using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskPrepare_BBP : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;
            var builtinBuildParameters = buildParameters as BuiltinBuildParameters;

            // 检测基础构建参数
            buildParametersContext.CheckBuildParameters();

            // 检测Unity版本
#if UNITY_2021_3_OR_NEWER
            if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
            {
                string warning = BuildLogger.GetErrorMessage(ErrorCode.RecommendScriptBuildPipeline, $"Starting with UnityEngine2021, recommend use script build pipeline (SBP) !");
                BuildLogger.Warning(warning);
            }
#endif
        }
    }
}