using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建内容打包")]
	public class TaskBuilding : IBuildTask
	{
		public class UnityManifestContext : IContextObject
		{
			public AssetBundleManifest UnityManifest;
		}

		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			// 模拟构建模式下跳过引擎构建
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
				return;

			BuildAssetBundleOptions opt = buildParametersContext.GetPipelineBuildOptions();
			AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(buildParametersContext.PipelineOutputDirectory, buildMapContext.GetPipelineBuilds(), opt, buildParametersContext.Parameters.BuildTarget);
			if (unityManifest == null)
				throw new Exception("构建过程中发生错误！");

			BuildRunner.Log("Unity引擎打包成功！");
			UnityManifestContext unityManifestContext = new UnityManifestContext();
			unityManifestContext.UnityManifest = unityManifest;
			context.SetContextObject(unityManifestContext);

			// 拷贝原生文件
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				CopyRawBundle(buildMapContext, buildParametersContext);
			}
		}

		/// <summary>
		/// 拷贝原生文件
		/// </summary>
		private void CopyRawBundle(BuildMapContext buildMapContext, AssetBundleBuilder.BuildParametersContext buildParametersContext)
		{
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				if (bundleInfo.IsRawFile)
				{
					string dest = $"{buildParametersContext.PipelineOutputDirectory}/{bundleInfo.BundleName}";
					foreach (var buildAsset in bundleInfo.BuildinAssets)
					{
						if (buildAsset.IsRawAsset)
							EditorTools.CopyFile(buildAsset.AssetPath, dest, true);
					}
				}
			}
		}
	}
}