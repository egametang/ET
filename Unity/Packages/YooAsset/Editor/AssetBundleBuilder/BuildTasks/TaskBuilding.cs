using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建内容打包")]
	public class TaskBuilding : IBuildTask
	{
		public class BuildResultContext : IContextObject
		{
			public AssetBundleManifest UnityManifest;
		}

		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			// 模拟构建模式下跳过引擎构建
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
				return;

			// 开始构建
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			BuildAssetBundleOptions buildOptions = buildParametersContext.GetPipelineBuildOptions();
			AssetBundleManifest buildResults = BuildPipeline.BuildAssetBundles(pipelineOutputDirectory, buildMapContext.GetPipelineBuilds(), buildOptions, buildParametersContext.Parameters.BuildTarget);
			if (buildResults == null)
			{
				throw new Exception("构建过程中发生错误！");
			}

			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				string unityOutputManifestFilePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}";
				if (System.IO.File.Exists(unityOutputManifestFilePath) == false)
					throw new Exception("构建过程中发生严重错误！请查阅上下文日志！");
			}

			BuildLogger.Log("Unity引擎打包成功！");
			BuildResultContext buildResultContext = new BuildResultContext();
			buildResultContext.UnityManifest = buildResults;
			context.SetContextObject(buildResultContext);
		}
	}
}