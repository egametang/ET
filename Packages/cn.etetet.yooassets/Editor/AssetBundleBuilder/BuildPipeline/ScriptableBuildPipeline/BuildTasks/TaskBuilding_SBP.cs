using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;

namespace YooAsset.Editor
{
    public class TaskBuilding_SBP : IBuildTask
    {
        public class BuildResultContext : IContextObject
        {
            public IBundleBuildResults Results;
        }

        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var scriptableBuildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;

            // 模拟构建模式下跳过引擎构建
            var buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.SimulateBuild)
                return;

            // 构建内容
            var buildContent = new BundleBuildContent(buildMapContext.GetPipelineBuilds());

            // 开始构建
            IBundleBuildResults buildResults;
            var buildParameters = scriptableBuildParameters.GetBundleBuildParameters();
            var taskList = SBPBuildTasks.Create(buildMapContext.Command.ShadersBundleName);
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, buildContent, out buildResults, taskList);
            if (exitCode < 0)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFailed, $"UnityEngine build failed ! ReturnCode : {exitCode}");
                throw new Exception(message);
            }

            // 创建着色器信息
            // 说明：解决因为着色器资源包导致验证失败。
            // 例如：当项目里没有着色器，如果有依赖内置着色器就会验证失败。
            string shadersBundleName = buildMapContext.Command.ShadersBundleName;
            if (buildResults.BundleInfos.ContainsKey(shadersBundleName))
            {
                buildMapContext.CreateShadersBundleInfo(shadersBundleName);
            }

            BuildLogger.Log("UnityEngine build success!");
            BuildResultContext buildResultContext = new BuildResultContext();
            buildResultContext.Results = buildResults;
            context.SetContextObject(buildResultContext);
        }
    }
}