using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建内容打包")]
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

			// 模拟构建模式下跳过引擎构建
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
				return;

			// 构建内容
			var buildContent = new BundleBuildContent(buildMapContext.GetPipelineBuilds());

			// 开始构建
			IBundleBuildResults buildResults;
			var buildParameters = buildParametersContext.GetSBPBuildParameters();
			var taskList = SBPBuildTasks.Create(buildMapContext.ShadersBundleName);
			ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, buildContent, out buildResults, taskList);
			if (exitCode < 0)
			{
				throw new Exception($"构建过程中发生错误 : {exitCode}");
			}

			// 创建着色器信息
			// 说明：解决因为着色器资源包导致验证失败。
			// 例如：当项目里没有着色器，如果有依赖内置着色器就会验证失败。
			string shadersBundleName = buildMapContext.ShadersBundleName;
			if (buildResults.BundleInfos.ContainsKey(shadersBundleName))
			{
				buildMapContext.CreateShadersBundleInfo(shadersBundleName);
			}

			BuildLogger.Log("Unity引擎打包成功！");
			BuildResultContext buildResultContext = new BuildResultContext();
			buildResultContext.Results = buildResults;
			context.SetContextObject(buildResultContext);
		}
	}
}