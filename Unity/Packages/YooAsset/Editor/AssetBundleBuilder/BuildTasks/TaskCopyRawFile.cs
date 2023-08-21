using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("拷贝原生文件")]
	public class TaskCopyRawFile : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				CopyRawBundle(buildMapContext, buildParametersContext);
			}
		}

		/// <summary>
		/// 拷贝原生文件
		/// </summary>
		private void CopyRawBundle(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext)
		{
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				if (bundleInfo.IsRawFile)
				{
					string dest = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
					foreach (var assetInfo in bundleInfo.AllMainAssets)
					{
						if (assetInfo.IsRawAsset)
							EditorTools.CopyFile(assetInfo.AssetPath, dest, true);
					}
				}
			}
		}
	}
}